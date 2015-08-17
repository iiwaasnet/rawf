﻿using System;
using System.Diagnostics;
using System.Threading;
using rawf.Diagnostics;
using rawf.Framework;

namespace rawf.Rendezvous.Consensus
{
    public partial class LeaseProvider : ILeaseProvider
    {
        private readonly IBallotGenerator ballotGenerator;
        private readonly ILeaseConfiguration config;
        private readonly IRendezvousConfiguration rendezvousConfig;
        private readonly Timer leaseTimer;
        private readonly ILogger logger;
        private readonly INode localNode;
        private readonly IRoundBasedRegister register;
        private readonly SemaphoreSlim renewGateway;
        private volatile ILease lastKnownLease;

        public LeaseProvider(IRoundBasedRegister register,
                             IBallotGenerator ballotGenerator,
                             ILeaseConfiguration config,
                             ISynodConfiguration synodConfig,
                             IRendezvousConfiguration rendezvousConfig,
                             ILogger logger)
        {
            ValidateConfiguration(config);

            WaitBeforeNextLeaseIssued(config);

            localNode = synodConfig.LocalNode;
            this.logger = logger;
            this.config = config;
            this.rendezvousConfig = rendezvousConfig;
            this.ballotGenerator = ballotGenerator;
            this.register = register;

            renewGateway = new SemaphoreSlim(1);
            leaseTimer = new Timer(state => ScheduledReadOrRenewLease(), null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
        }

        public void ResetLease()
        {
            Interlocked.Exchange(ref lastKnownLease, null);
        }

        public ILease GetLease()
        {
            var timer = new Stopwatch();
            timer.Start();

            var lease = GetLastKnownLease();

            timer.Stop();
            logger.InfoFormat("Lease received {0} in {1} msec",
                              lastKnownLease != null ? lastKnownLease.OwnerIdentity.GetString() : "null",
                              timer.ElapsedMilliseconds);

            return lease;
        }

        public void Dispose()
        {
            register.Dispose();
            leaseTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
            leaseTimer.Dispose();
            register.Dispose();
            renewGateway.Dispose();
        }

        private void ValidateConfiguration(ILeaseConfiguration config)
        {
            if (config.NodeResponseTimeout.TotalMilliseconds * 2 > config.MessageRoundtrip.TotalMilliseconds)
            {
                throw new Exception(string.Format("NodeResponseTimeout[{0} msec] should be at least 2 times shorter than MessageRoundtrip[{1} msec]!",
                                                  config.NodeResponseTimeout.TotalMilliseconds,
                                                  config.MessageRoundtrip.TotalMilliseconds));
            }
            if (config.MaxLeaseTimeSpan
                - TimeSpan.FromTicks(config.MessageRoundtrip.Ticks * 2)
                - config.ClockDrift <= TimeSpan.FromMilliseconds(0))
            {
                throw new Exception(string.Format("MaxLeaseTimeSpan[{0} msec] should be longer than (2 * MessageRoundtrip[{1} msec] + ClockDrift[{2} msec])",
                                                  config.MaxLeaseTimeSpan.TotalMilliseconds,
                                                  config.MessageRoundtrip.TotalMilliseconds,
                                                  config.ClockDrift.TotalMilliseconds));
            }
        }

        private void ScheduledReadOrRenewLease()
        {
            if (renewGateway.Wait(TimeSpan.FromMilliseconds(10)))
            {
                try
                {
                    ReadOrRenewLease();
                }
                catch (Exception err)
                {
                    logger.Error(err);
                }
                finally
                {
                    renewGateway.Release();
                }
            }
        }

        private void ReadOrRenewLease()
        {
            // TODO: Uncomment
            var now = DateTime.UtcNow;
            //var now = DateTime.UtcNow - TimeSpan.FromMinutes(1);

            var lease = AсquireOrLearnLease(ballotGenerator.New(localNode.SocketIdentity), now);

            if (ProcessBecameLeader(lease, lastKnownLease) || ProcessLostLeadership(lease, lastKnownLease))
            {
                var renewPeriod = CalcLeaseRenewPeriod(ProcessBecameLeader(lease, lastKnownLease));
                leaseTimer.Change(renewPeriod, renewPeriod);
            }

            lastKnownLease = lease;
        }

        private bool ProcessLostLeadership(ILease nextLease, ILease previousLease)
        {
            return (previousLease != null && previousLease.OwnerIdentity.Equals(localNode)
                    && nextLease != null && !nextLease.OwnerIdentity.Equals(localNode));
        }

        private bool ProcessBecameLeader(ILease nextLease, ILease previousLease)
        {
            return ((previousLease == null || !previousLease.OwnerIdentity.Equals(localNode))
                    && nextLease != null && nextLease.OwnerIdentity.Equals(localNode));
        }

        private TimeSpan CalcLeaseRenewPeriod(bool leader)
        {
            return (leader)
                       ? config.MaxLeaseTimeSpan
                         - TimeSpan.FromTicks(config.MessageRoundtrip.Ticks * 2)
                         - config.ClockDrift
                       : config.MaxLeaseTimeSpan;
        }

        private ILease GetLastKnownLease()
        {
            var now = DateTime.UtcNow;

            renewGateway.Wait();
            try
            {
                if (LeaseNullOrExpired(lastKnownLease, now))
                {
                    ReadOrRenewLease();
                }

                return lastKnownLease;
            }
            finally
            {
                renewGateway.Release();
            }
        }

        private ILease AсquireOrLearnLease(IBallot ballot, DateTime now)
        {
            var read = register.Read(ballot);
            if (read.TxOutcome == TxOutcome.Commit)
            {
                var lease = read.Lease;
                if (LeaseIsNotSafelyExpired(lease, now))
                {
                    LogStartSleep();
                    Sleep(config.ClockDrift);
                    LogAwake();

                    // TOOD: Add recursion exit condition
                    return AсquireOrLearnLease(ballotGenerator.New(localNode.SocketIdentity), DateTime.UtcNow);
                }

                if (LeaseNullOrExpired(lease, now) || IsLeaseOwner(lease))
                {
                    LogLeaseProlonged(lease);
                    var ownerEndpoint = new OwnerEndpoint {UnicastUri = rendezvousConfig.UnicastUri, MulticastUri = rendezvousConfig.MulticastUri};
                    lease = new Lease(localNode.SocketIdentity, ownerEndpoint, now + config.MaxLeaseTimeSpan);
                }

                logger.InfoFormat("Write lease: OwnerIdentity {0}", lease.OwnerIdentity);
                var write = register.Write(ballot, lease);
                if (write.TxOutcome == TxOutcome.Commit)
                {
                    return lease;
                }
            }

            return null;
        }

        private bool IsLeaseOwner(ILease lease)
        {
            return lease != null && lease.OwnerIdentity.Equals(localNode);
        }

        private static bool LeaseNullOrExpired(ILease lease, DateTime now)
        {
            return lease == null || lease.ExpiresAt < now;
        }

        private bool LeaseIsNotSafelyExpired(ILease lease, DateTime now)
        {
            return lease != null
                   && lease.ExpiresAt < now
                   && lease.ExpiresAt + config.ClockDrift > now;
        }

        private void WaitBeforeNextLeaseIssued(ILeaseConfiguration config)
        {
            Sleep(config.MaxLeaseTimeSpan);
        }

        private void Sleep(TimeSpan delay)
        {
            using (var @lock = new ManualResetEvent(false))
            {
                @lock.WaitOne(delay);
            }
        }

        //TODO: add Dispose() method???
    }
}