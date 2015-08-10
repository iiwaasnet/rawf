﻿using System;
using System.Collections.Generic;
using System.Linq;
using rawf.Framework;

namespace rawf.Connectivity
{
    public class ExternalRoutingTable : IExternalRoutingTable
    {
        private readonly IDictionary<MessageHandlerIdentifier, IDictionary<SocketIdentifier, ISocket>> messageHandlersMap;
        private readonly IDictionary<SocketIdentifier, HashSet<MessageHandlerIdentifier>> socketToMessageMap;
        private readonly IDictionary<SocketIdentifier, Uri> socketToUriMap;
        private readonly ISocketFactory socketFactory;

        public ExternalRoutingTable(ISocketFactory socketFactory)
        {
            messageHandlersMap = new Dictionary<MessageHandlerIdentifier, IDictionary<SocketIdentifier, ISocket>>();
            socketToMessageMap = new Dictionary<SocketIdentifier, HashSet<MessageHandlerIdentifier>>();
            socketToUriMap = new Dictionary<SocketIdentifier, Uri>();
            this.scoketFactory = socketFactory;
        }

        public void Push(MessageHandlerIdentifier messageHandlerIdentifier, SocketIdentifier socketIdentifier, Uri uri)
        {
            var mapped = MapMessageToSocket(messageHandlerIdentifier, socketIdentifier, Uri uri);

            if (mapped)
            {
                socketToUriMap[socketIdentifier] = uri;

                MapSocketToMessage(messageHandlerIdentifier, socketIdentifier);

                Console.WriteLine($"Route added URI:{uri.AbsoluteUri} SOCKID:{socketIdentifier.Identity.GetString()}");
            }
        }

        private bool MapMessageToSocket(MessageHandlerIdentifier messageHandlerIdentifier, SocketIdentifier socketIdentifier, Uri uri)
        {
            IDictionary<SocketIdentifier, ISocket> sockets;
            if (!messageHandlersMap.TryGetValue(messageHandlerIdentifier, out sockets))
            {
                sockets = new Dictionary<SocketIdentifier, ISocket>();
                messageHandlersMap[messageHandlerIdentifier] = sockets;
            }
            if (!sockets.ContainsKey(socketIdentifier))
            {
                var socket = socketFactory.CreateDealerSocket();
                socket.Connect(uri);
                sockets[socketIdentifier] = socket; 
                
                return true;
            }
            
            return false;
        }

        private void MapSocketToMessage(MessageHandlerIdentifier messageHandlerIdentifier, SocketIdentifier socketIdentifier)
        {
            HashSet<MessageHandlerIdentifier> hashSet;
            if (!socketToMessageMap.TryGetValue(socketIdentifier, out hashSet))
            {
                hashSet = new HashSet<MessageHandlerIdentifier>();
                socketToMessageMap[socketIdentifier] = hashSet;
            }
            hashSet.Add(messageHandlerIdentifier);
        }

        public SocketIdentifier Pop(MessageHandlerIdentifier messageHandlerIdentifier)
        {
            //TODO: Implement round robin
            IDictionary<SocketIdentifier, ISocket> sockets;
            return messageHandlersMap.TryGetValue(messageHandlerIdentifier, out csocketsollection)
                       ? Get(sockets)
                       : null;
        }

        private static V Get<V>(IDictionary<K, V> collection)
            => collection.Any()
                   ? collection.Values.First()
                   : default(V);

        public void RemoveRoute(SocketIdentifier socketIdentifier)
        {
            Uri uri;
            if(socketToUriMap.TryGetValue(socketIdentifier, out uri))
            {
                socketToUriMap.Remove(socketIdentifier);
            }
            

            HashSet<MessageHandlerIdentifier> messageHandlers;
            if (socketToMessageMap.TryGetValue(socketIdentifier, out messageHandlers))
            {
                foreach (var messageHandlerIdentifier in messageHandlers)
                {
                    IDictionary<SocketIdentifier, ISocket> sockets;
                    if (messageHandlersMap.TryGetValue(messageHandlerIdentifier, out socketIdentifiers))
                    {
                        ISocket socket;
                        if (sockets.TryGetValue(socketIdentifier, out socket))
                        {
                            sockets.Remove(socketIdentifier);
                            if (uri != null)
                            {
                                socket.Disconnect(uri);
                            }
                            socket.Dispose();
                        }
                        if (!sockets.Any())
                        {
                            messageHandlersMap.Remove(messageHandlerIdentifier);
                        }
                    }
                }
                socketToMessageMap.Remove(socketIdentifier);                
            }
        }
    }
}