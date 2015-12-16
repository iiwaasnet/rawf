﻿using System;
using kino.Core.Connectivity;
using kino.Core.Connectivity.ServiceMessageHandlers;
using kino.Core.Diagnostics;
using kino.Core.Messaging;
using kino.Core.Messaging.Messages;
using kino.Core.Sockets;
using Moq;
using NUnit.Framework;

namespace kino.Tests.Connectivity
{
    [TestFixture]
    public class ExternalMessageRouteRegistrationHandlerTests
    {
        [Test]
        public void TestIfRegisterExternalMessageRouteMessageReceived_NoYetConnectionMadeToRemotePeer()
        {
            var logger = new Mock<ILogger>().Object;
            var externalRoutingTable = new ExternalRoutingTable(logger);
            var handler = new ExternalMessageRouteRegistrationHandler(externalRoutingTable, logger);
            var socket = new Mock<ISocket>();
            var message = Message.Create(new RegisterExternalMessageRouteMessage
                                         {
                                             Uri = "tcp://127.0.0.1:80",
                                             MessageContracts = new[]
                                                                {
                                                                    new MessageContract
                                                                    {
                                                                        Identity = Guid.NewGuid().ToByteArray(),
                                                                        Version = Guid.NewGuid().ToByteArray()
                                                                    }
                                                                },
                                             SocketIdentity = Guid.NewGuid().ToByteArray()
                                         });

            handler.Handle(message, socket.Object);

            socket.Verify(m => m.Connect(It.IsAny<Uri>()), Times.Never());
        }
    }
}