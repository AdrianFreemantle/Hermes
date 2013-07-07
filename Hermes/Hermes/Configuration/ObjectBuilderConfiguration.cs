namespace Hermes.Configuration
{
    public static class ObjectBuilderConfiguration
    {
        public static Configure ObjectBuilder(this Configure config, IObjectBuilder objectBuilder)
        {
            Settings.Builder = objectBuilder;
            return config;
        }
    }
}