﻿using System;
using kino.Core.Framework;
using ProtoBuf;

namespace kino.Core.Messaging.Messages
{
    [ProtoContract]
    public class ExceptionMessage : Payload
    {
        private static readonly IMessageSerializer messageSerializer = new NewtonJsonMessageSerializer();

        private static readonly byte[] MessageIdentity = "EXCEPTION".GetBytes();
        private static readonly byte[] MessageVersion = Message.CurrentVersion;

        public ExceptionMessage()
            :base(messageSerializer)
        {
        }

        [ProtoMember(1)]
        public Exception Exception { get; set; }

        public override byte[] Version => Message.CurrentVersion;
        public override byte[] Identity => MessageIdentity;
    }
}