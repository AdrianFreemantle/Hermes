using System;
using System.Collections.Generic;
using System.Configuration;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Management;
using Hermes.Messaging.Storage.MsSql;
using Hermes.Messaging.Transports.SqlTransport;

namespace Hermes.Messaging.Transports.Msmq
{
    public static class MsmqTransportConfiguration 
    {
        public static IConfigureEndpoint UseMsmqTransport(this IConfigureEndpoint config)
        {
            config.RegisterDependencies(new MsmqMessagingDependencyRegistrar());

            return config;
        }

        private class MsmqMessagingDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<MsmqReceiver>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<MsmqMessageSender>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<MsmqQueueCreator>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<MsmqSettings>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<MsmqUnitOfWork>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<StubPersistence>(DependencyLifecycle.SingleInstance);
            }
        }
    }

    public class StubPersistence : IStoreSubscriptions, IPersistTimeouts
    {
        public void Subscribe(Address client, params Type[] messageTypes)
        {
            
        }

        public void Unsubscribe(Address client, params Type[] messageTypes)
        {
        }

        public IEnumerable<Address> GetSubscribersForMessageTypes(ICollection<Type> messageTypes)
        {
            return new Address[0];
        }

        public void Add(ITimeoutData timeout)
        {
        }

        public void Purge()
        {
        }

        public bool TryFetchNextTimeout(out ITimeoutData timeoutData)
        {
            timeoutData = null;

            return false;
        }

        public void Remove(Guid correlationId)
        {
        }
    }
}
