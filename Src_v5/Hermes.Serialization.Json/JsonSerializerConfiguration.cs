using Hermes.Ioc;

namespace Hermes.Serialization.Json
{
    public static class JsonSerializerConfiguration
    {
        public static IEndpoint UseJsonSerialization(this IEndpoint endpoint)
        {
            endpoint.RegisterDependencies<JsonSerializerDependencyRegistrar>();
            return endpoint;
        }

        private class JsonSerializerDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<JsonObjectSerializer>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
