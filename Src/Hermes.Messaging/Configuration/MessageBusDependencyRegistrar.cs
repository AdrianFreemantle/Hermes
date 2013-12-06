using Hermes.Ioc;
using Hermes.Messaging.Bus;
using Hermes.Messaging.Callbacks;
using Hermes.Messaging.Monitoring;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Timeouts;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Configuration
{
    internal class MessageBusDependencyRegistrar : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<CallBackManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);            
            containerBuilder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<IncomingMessageContext>(DependencyLifecycle.InstancePerDependency);
            containerBuilder.RegisterType<OutgoingMessageContext>(DependencyLifecycle.InstancePerDependency);

            if (!Settings.IsClientEndpoint)
            {
                containerBuilder.RegisterType<MessageErrorModule>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<AuditModule>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
