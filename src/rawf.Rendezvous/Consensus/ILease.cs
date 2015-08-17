﻿using System;

namespace rawf.Rendezvous.Consensus
{
    public interface ILease
    {
        byte[] OwnerIdentity { get; }
        OwnerEndpoint OwnerEndpoint { get; }
        DateTime ExpiresAt { get; }
    }
}