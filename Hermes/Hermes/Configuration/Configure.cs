using System;

using Hermes.Logging;

namespace Hermes.Configuration
{
    public class Configure
    {
        private static readonly Configure instance;

        static Configure()
        {
            instance = new Configure();
        }

        private Configure()
        {
            
        }

        public static Configure With()
        {
            return instance;
        }

        public Configure ObjectBuilder(IObjectBuilder objectBuilder)
        {
            Settings.Builder = objectBuilder;
            return this;
        }

        public Configure NumberOfWorkers(int numberOfWorkers)
        {
            Settings.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        public Configure ConsoleWindowLogger()
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            return this;
        }

        public Configure Logger(Func<Type, ILog> buildLogger)
        {
            LogFactory.BuildLogger = buildLogger;
            return this;
        }
    }
}