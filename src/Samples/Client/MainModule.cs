﻿using System.Collections.Generic;
using Autofac;
using kino.Client;
using kino.Connectivity;
using kino.Framework;
using kino.Messaging;
using TypedConfigProvider;

namespace Client
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new kino.Client.MainModule());

            builder.RegisterType<ConfigProvider>()
                   .As<IConfigProvider>()
                   .SingleInstance();

            builder.RegisterType<AppConfigTargetProvider>()
                   .As<IConfigTargetProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigProvider>().GetConfiguration<ApplicationConfiguration>())
                   .As<ApplicationConfiguration>()
                   .SingleInstance();

            builder.RegisterType<ExpirableItemCollection<CorrelationId>>()
                   .As<IExpirableItemCollection<CorrelationId>>();

            builder.RegisterType<MessageHubConfigurationProvider>()
                   .As<IMessageHubConfigurationProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IMessageHubConfigurationProvider>().GetConfiguration())
                   .As<IMessageHubConfiguration>()
                   .SingleInstance();

            builder.RegisterType<RouterConfigurationProvider>()
                   .As<IRouterConfigurationProvider>()
                   .SingleInstance();
            builder.Register(c => c.Resolve<IRouterConfigurationProvider>().GetConfiguration())
                   .As<RouterConfiguration>()
                   .SingleInstance();

            builder.RegisterType<ExpirableItemCollectionConfigurationProvider>()
                   .As<IExpirableItemCollectionConfigurationProvider>()
                   .SingleInstance();
            builder.Register(c => c.Resolve<IExpirableItemCollectionConfigurationProvider>().GetConfiguration())
                   .As<IExpirableItemCollectionConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IRendezvousEndpointsProvider>().GetConfiguration())
                   .As<IEnumerable<RendezvousEndpoints>>()
                   .SingleInstance();

            builder.RegisterType<RendezvousEndpointsProvider>()
                   .As<IRendezvousEndpointsProvider>()
                   .SingleInstance();

            builder.RegisterType<ClusterTimingConfigurationProvider>()
                   .As<IClusterTimingConfigurationProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IClusterTimingConfigurationProvider>().GetConfiguration())
                   .As<ClusterTimingConfiguration>()
                   .SingleInstance();
        }
    }
}