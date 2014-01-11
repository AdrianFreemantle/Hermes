using Hermes.Ioc;
using Hermes.Messaging.Bus;
using Hermes.Messaging.Callbacks;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Pipeline.Modules;
using Hermes.Messaging.Routing;
using Hermes.Messaging.Timeouts;
using Hermes.Messaging.Transports;
using Hermes.Pipes;
using Hermes.Reflection;

namespace Hermes.Messaging.Configuration
{
    internal class MessageBusDependencyRegistrar : IRegisterDependencies
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<Transport>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Router>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<StorageDrivenPublisher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Receiver>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<CallBackManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<LocalBus>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<SubscriptionManager>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<Dispatcher>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TimeoutProcessor>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<TypeMapper>(DependencyLifecycle.SingleInstance);

            containerBuilder.RegisterType<MessageErrorModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<AuditModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<ExtractMessagesModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageMutatorModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<DispatchMessagesModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<CallBackHandlerModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<MessageSerializationModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<HeaderBuilderModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<SendMessageModule>(DependencyLifecycle.SingleInstance);
            containerBuilder.RegisterType<UnitOfWorkModule>(DependencyLifecycle.InstancePerUnitOfWork);

            containerBuilder.RegisterType<OutgoingMessageContext>(DependencyLifecycle.InstancePerDependency);

            if (Settings.IsSendOnly)
            {
                var incommingPipeline = new ModuleStack<IncomingMessageContext>();
                containerBuilder.RegisterSingleton(incommingPipeline);
            }
            else if (Settings.IsClientEndpoint)
            {
                var incommingPipeline = new ModuleStack<IncomingMessageContext>()
                    .Add<ExtractMessagesModule>()
                    .Add<MessageMutatorModule>()
                    .Add<UnitOfWorkModule>()
                    .Add<DispatchMessagesModule>()
                    .Add<CallBackHandlerModule>();

                containerBuilder.RegisterSingleton(incommingPipeline);
            }
            else
            {
                var incommingPipeline = new ModuleStack<IncomingMessageContext>()
                    .Add<MessageErrorModule>()
                    .Add<AuditModule>()
                    .Add<ExtractMessagesModule>()
                    .Add<MessageMutatorModule>()
                    .Add<UnitOfWorkModule>()
                    .Add<DispatchMessagesModule>()
                    .Add<CallBackHandlerModule>();

                containerBuilder.RegisterSingleton(incommingPipeline);
            }

            var outgoingPipeline = new ModuleStack<OutgoingMessageContext>()
                .Add<MessageMutatorModule>()
                .Add<MessageSerializationModule>()
                .Add<HeaderBuilderModule>()
                .Add<SendMessageModule>();

            containerBuilder.RegisterSingleton(outgoingPipeline);
        }
    }
}
