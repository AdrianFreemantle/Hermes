using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Serialization.Json
{
    public static class JsonSerializerConfiguration
    {
        public static IConfigureEndpoint UseJsonSerialization(this IConfigureEndpoint config)
        {
            config.RegisterDependancies(new JsonSerializerDependancyRegistrar());
            return config;
        }

        private class JsonSerializerDependancyRegistrar : IRegisterDependancies
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
