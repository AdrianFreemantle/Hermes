﻿using System.Reflection;

namespace Hermes.Configuration
{
    public interface IConfigureBus
    {
        IConfigureBus NumberOfWorkers(int numberOfWorkers);
        IConfigureBus ScanForHandlersIn(params Assembly[] assemblies);
        IConfigureBus RegisterMessageRoute<TMessage>(Address endpointAddress);
        IConfigureBus SubscribeToEvent<T>();
        void Start();
        void Stop();
    }
}