﻿using System;
using NUnit.Framework;
using rawf.Connectivity;
using rawf.Messaging;
using rawf.Messaging.Messages;
using rawf.Tests.Backend.Setup;

namespace rawf.Tests
{
    [TestFixture]
    public class MessageHandlerIdentifierTests
    {
        [Test]
        public void TestTwoMessageHandlerIdentifiers_AreComparedByVersionIdentity()
        {
            var firstIdentifier = new MessageHandlerIdentifier(Message.CurrentVersion, SimpleMessage.MessageIdentity);
            var secondIdentifier = new MessageHandlerIdentifier(Message.CurrentVersion, SimpleMessage.MessageIdentity);

            Assert.AreEqual(firstIdentifier, secondIdentifier);
            Assert.IsTrue(firstIdentifier.Equals((object)secondIdentifier));

            var thirdIdentifier = new MessageHandlerIdentifier(Guid.NewGuid().ToByteArray(), SimpleMessage.MessageIdentity);

            Assert.AreNotEqual(firstIdentifier, thirdIdentifier);
            Assert.IsFalse(firstIdentifier.Equals((object)thirdIdentifier));
        }
    }
}