using System;
using System.Reflection;

using Hermes.Messaging;

namespace Hermes.Configuration
{
    public interface IConfigureBus
    {
        IConfigureBus NumberOfWorkers(int numberOfWorkers);
        IConfigureBus ScanForHandlersIn(params Assembly[] assemblies);
        IConfigureBus RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureBus SubscribeToEvent<T>();
        IConfigureBus UseDistributedTransaction();
        IConfigureBus FirstLevelRetry(int attempts, TimeSpan delay);
        IConfigureBus SecondLevelRetry(int attempts, TimeSpan delay);
        void Start();
        void Stop();
    }
}