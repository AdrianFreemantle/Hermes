using Hermes.Configuration;
using Hermes.Ioc;

namespace Hermes.Serialization.Json
{
    public static class JsonSerializerConfiguration
    {
        public static IConfigureBus UsingJsonSerialization(this IConfigureBus config)
        {
            Settings.Builder.RegisterType<JsonObjectSerializer>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<JsonMessageSerializer>(DependencyLifecycle.SingleInstance);

            return config;
        }
    }
}
