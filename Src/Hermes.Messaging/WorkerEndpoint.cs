using System;
using System.Reflection;
using System.Threading;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public abstract class WorkerEndpoint<TContainerBuilder> : IService
        where TContainerBuilder : IContainerBuilder, new()
    {
        private readonly Configure configuration;
        private bool disposed;

        protected WorkerEndpoint()
        {
            var containerBuilder = new TContainerBuilder();
            string endpointName = Assembly.GetAssembly(GetType()).GetName().Name;
            configuration = Configure.WorkerEndpoint(endpointName, containerBuilder);
            ConfigureEndpoint(configuration);
            Settings.RootContainer = containerBuilder.BuildContainer();
        }

        protected abstract void ConfigureEndpoint(IConfigureWorker configuration);

        public void Run(CancellationToken token)
        {
            configuration.Start();

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }

            configuration.Stop();
        }

        ~WorkerEndpoint()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                configuration.Stop();
            }

            disposed = true;
        }
    }
}