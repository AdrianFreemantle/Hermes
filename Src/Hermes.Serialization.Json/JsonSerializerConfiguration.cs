using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Serialization.Json
{
    public static class JsonSerializerConfiguration
    {
        public static IConfigureEndpoint UseJsonSerialization(this IConfigureEndpoint config)
        {
            config.RegisterDependencies(new JsonSerializerDependencyRegistrar());
            return config;
        }

        private class JsonSerializerDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<JsonObjectSerializer>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
