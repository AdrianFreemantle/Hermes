using System;

using Hermes.Logging;

namespace Hermes.Configuration
{
    public interface IConfigureEnvironment
    {
        IConfigureEnvironment ConsoleWindowLogger();
        IConfigureEnvironment Logger(Func<Type, ILog> buildLogger);
    }
}