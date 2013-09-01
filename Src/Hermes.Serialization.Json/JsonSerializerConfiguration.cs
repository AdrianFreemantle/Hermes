using Hermes.Configuration;
using Hermes.Ioc;

namespace Hermes.Serialization.Json
{
    public static class JsonSerializerConfiguration
    {
        public static IConfigureEndpoint UseJsonSerialization(this IConfigureEndpoint config)
        {
            Settings.Builder.RegisterType<JsonObjectSerializer>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
