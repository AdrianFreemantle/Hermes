using System;
using System.Linq;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public abstract class ClientEndpoint<TContainerBuilder> : IDisposable
        where TContainerBuilder : IContainerBuilder, new()
    {
        private readonly Configure configuration;
        private bool disposed;

        public IMessageBus MessageBus { get { return Settings.RootContainer.GetInstance<IMessageBus>(); } }

        protected ClientEndpoint()
        {
            var containerBuilder = new TContainerBuilder();
            string endpointName = Assembly.GetAssembly(GetType()).GetName().Name;
            configuration = Configure.ClientEndpoint(endpointName, containerBuilder);
            ConfigureEndpoint(configuration);
            Settings.RootContainer = containerBuilder.BuildContainer();
        }

        protected abstract void ConfigureEndpoint(IConfigureEndpoint configuration);

        public void Start()
        {
            configuration.Start();
        }

        public void Stop()
        {
            configuration.Stop();
        }

        ~ClientEndpoint()
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
