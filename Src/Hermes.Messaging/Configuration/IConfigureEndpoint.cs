using System;
using System.Reflection;

using Hermes.Logging;

namespace Hermes.Messaging.Configuration
{
    public interface IConfigureEndpoint
    {
        IConfigureEndpoint NumberOfWorkers(int numberOfWorkers);
        IConfigureEndpoint ScanForHandlersIn(params Assembly[] assemblies);
        IConfigureEndpoint RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureEndpoint SubscribeToEvent<T>();
        IConfigureEndpoint UseDistributedTransaction();
        IConfigureEndpoint FirstLevelRetryPolicy(int attempts, TimeSpan delay);
        IConfigureEndpoint SecondLevelRetryPolicy(int attempts, TimeSpan delay);
        IConfigureEndpoint UseConsoleWindowLogger();
        IConfigureEndpoint Logger(Func<Type, ILog> buildLogger);
        void Start();
        void Stop();
    }
}