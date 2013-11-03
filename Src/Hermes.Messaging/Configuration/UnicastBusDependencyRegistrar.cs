using Hermes.Ioc;
using Hermes.Messaging.Bus;
using Hermes.Messaging.Bus.Transports;
using Hermes.Messaging.Callbacks;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Timeouts;

namespace Hermes.Messaging.Configuration
{
    internal class UnicastBusDependencyRegistrar : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<ErrorHandler>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TransportMessageFactory>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<CallBackManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<IncomingMessageProcessor>(DependencyLifecycle.InstancePerUnitOfWork);

            containerBuilder.RegisterType<OutgoingMessagesProcessor>(DependencyLifecycle.InstancePerUnitOfWork);

            if (!Settings.IsClientEndpoint)
            {                
                containerBuilder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
