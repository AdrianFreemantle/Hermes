using System;
using System.Reflection;
using System.Threading;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging.EndPoints
{
    public abstract class ControlEndpoint<TContainerBuilder> : IService
        where TContainerBuilder : IContainerBuilder, new()
    {
        private readonly Configure configuration;
        private bool disposed;

        protected ControlEndpoint()
        {
            var containerBuilder = new TContainerBuilder();
            string endpointName = Assembly.GetAssembly(GetType()).GetName().Name;
            configuration = Configure.Initialize(endpointName, containerBuilder);
            ConfigureEndpoint(configuration);
            ConfigurePipeline(containerBuilder);
            Settings.RootContainer = containerBuilder.BuildContainer();
        }

        protected abstract void ConfigureEndpoint(IConfigureWorker configuration);

        protected abstract void ConfigurePipeline(TContainerBuilder containerBuilder);

        protected void RaiseEvent(IDomainEvent message)
        {
            try
            {
                using (IContainer childContainer = Settings.RootContainer.BeginLifetimeScope())
                {
                    ServiceLocator.Current.SetCurrentLifetimeScope(childContainer);
                    var messageTransport = childContainer.GetInstance<ITransportMessages>();
                    var incomingContext = new IncomingMessageContext(message, childContainer);
                    messageTransport.ProcessMessage(incomingContext);
                }
            }
            finally
            {
                ServiceLocator.Current.SetCurrentLifetimeScope(null);
            }
        }

        public void Run(CancellationToken token)
        {
            configuration.Start();

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }

            configuration.Stop();
        }

        ~ControlEndpoint()
        {
            Dispose(false);
        }

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