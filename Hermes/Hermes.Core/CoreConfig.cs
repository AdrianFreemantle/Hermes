using Hermes.Configuration;
using Hermes.Ioc;

namespace Hermes.Core
{
    public static class CoreConfig
    {
        public static IConfigureBus UsingUnicastBus(this IConfigureBus config)
        {
            Settings.Builder.RegisterType<MessageHandlerFactory>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageDispatcher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageProcessor>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageTransport>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageBus>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<MessageRouter>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
