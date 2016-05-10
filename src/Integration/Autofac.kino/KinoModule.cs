﻿using kino.Actors;
using kino.Actors.Diagnostics;
using kino.Client;
using kino.Core.Connectivity;
using kino.Core.Connectivity.ServiceMessageHandlers;
using kino.Core.Framework;
using kino.Core.Sockets;

namespace Autofac.kino
{
    public class KinoModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterServiceMessageHandlers(builder);
            RegisterFrameworkActors(builder);

            builder.RegisterType<MessageRouter>()
                   .As<IMessageRouter>()
                   .SingleInstance();

            builder.Register(c => new SocketFactory(c.ResolveOptional<SocketConfiguration>()))
                   .As<ISocketFactory>()
                   .SingleInstance();

            builder.RegisterType<InternalRoutingTable>()
                   .As<IInternalRoutingTable>()
                   .SingleInstance();

            builder.RegisterType<ExternalRoutingTable>()
                   .As<IExternalRoutingTable>()
                   .SingleInstance();

            builder.RegisterType<ClusterMonitor>()
                   .As<IClusterMonitor>()
                   .SingleInstance();

            builder.RegisterType<ClusterMonitorProvider>()
                   .As<IClusterMonitorProvider>()
                   .SingleInstance();

            builder.RegisterType<ClusterMembership>()
                   .As<IClusterMembership>()
                   .SingleInstance();

            builder.RegisterType<ClusterMessageSender>()
                   .As<IClusterMessageSender>()
                   .SingleInstance();

            builder.RegisterType<ClusterMessageListener>()
                   .As<IClusterMessageListener>()
                   .SingleInstance();

            builder.RegisterType<RouteDiscovery>()
                   .As<IRouteDiscovery>()
                   .SingleInstance();

            builder.RegisterType<RendezvousCluster>()
                   .As<IRendezvousCluster>()
                   .SingleInstance();

            builder.RegisterType<CallbackHandlerStack>()
                   .As<ICallbackHandlerStack>()
                   .SingleInstance();

            builder.RegisterType<MessageHub>()
                   .As<IMessageHub>()
                   .SingleInstance();

            builder.RegisterType<ActorHostManager>()
                   .As<IActorHostManager>()
                   .SingleInstance();

            builder.RegisterType<RendezvousClusterConfigurationReadonlyStorage>()
                   .As<IConfigurationStorage<RendezvousClusterConfiguration>>()
                   .SingleInstance();
        }

        private void RegisterFrameworkActors(ContainerBuilder builder)
        {
            builder.RegisterType<KnownMessageRoutesActor>()
                   .As<IActor>()
                   .SingleInstance();

            builder.RegisterType<ExceptionHandlerActor>()
                   .As<IActor>()
                   .SingleInstance();
        }

        private void RegisterServiceMessageHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<ExternalMessageRouteRegistrationHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();

            builder.RegisterType<InternalMessageRouteRegistrationHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();

            builder.RegisterType<MessageRouteDiscoveryHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();

            builder.RegisterType<MessageRouteUnregistrationHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();

            builder.RegisterType<RoutesRegistrationRequestHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();

            builder.RegisterType<RouteUnregistrationHandler>()
                   .As<IServiceMessageHandler>()
                   .SingleInstance();
        }
    }
}