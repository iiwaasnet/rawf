﻿using System;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using rawf.Actors;
using rawf.Connectivity;
using rawf.Messaging;
using rawf.Tests.Actor.Setup;

namespace rawf.Tests.Actor
{
    [TestFixture]
    public class ActorHostTests
    {
        [Test]
        public void TestAssignActor_RegistersActorHandlers()
        {
            var actorHandlersMap = new ActorHandlersMap();

            var actorHost = new ActorHost(actorHandlersMap, new ConnectivityProvider(), new HostConfiguration(string.Empty));
            actorHost.AssignActor(new EchoActor());

            var registration = actorHandlersMap.GetRegisteredIdentifiers().First();
            CollectionAssert.AreEqual(EmptyMessage.MessageIdentity, registration.Identity);
            CollectionAssert.AreEqual(Message.CurrentVersion, registration.Version);
        }

        [Test]
        public void TestStartingActorHost_SendsActorRegistrationMessage()
        {
            var actorHandlersMap = new ActorHandlersMap();
            var connectivityProvider = new Mock<IConnectivityProvider>();
            var socket = new StubSocket();
            connectivityProvider.Setup(m => m.CreateDealerSocket()).Returns(socket);

            var actorHost = new ActorHost(actorHandlersMap, connectivityProvider.Object, new HostConfiguration(string.Empty));
            actorHost.AssignActor(new EchoActor());
            actorHost.Start();

            var registration = socket.GetSentMessages().First();
            var payload = new RegisterMessageHandlers
                          {
                              SocketIdentity = socket.GetIdentity(),
                              Registrations = actorHandlersMap
                                  .GetRegisteredIdentifiers()
                                  .Select(mh => new MessageHandlerRegistration
                                                {
                                                    Identity = mh.Identity,
                                                    Version = mh.Version,
                                                    IdentityType = IdentityType.Actor
                                                })
                                  .ToArray()
                          };
            var regMessage = Message.Create(payload, RegisterMessageHandlers.MessageIdentity);

            CollectionAssert.AreEqual(registration.Body, regMessage.Body);
        }
    }
}