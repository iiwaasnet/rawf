﻿using kino.Core;
using kino.Messaging;

namespace kino.Routing.Kafka.ServiceMessageHandlers
{
    public interface IKafkaServiceMessageHandler
    {
        void Handle(IMessage message);

        MessageIdentifier TargetMessage { get; }
    }
}