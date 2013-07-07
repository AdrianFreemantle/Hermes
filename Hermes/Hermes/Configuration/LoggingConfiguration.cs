using System;

using Hermes.Logging;

namespace Hermes.Configuration
{
    public static class LoggingConfiguration
    {
        public static Configure ConsoleWindowLogger(this Configure config)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            return config;
        }

        public static Configure Logger(this Configure config, Func<Type, ILog> buildLogger)
        {
            LogFactory.BuildLogger = buildLogger;
            return config;
        }
    }
}
