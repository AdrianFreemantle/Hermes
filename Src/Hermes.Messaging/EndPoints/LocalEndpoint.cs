using System;
using System.Reflection;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;
using Hermes.Messaging.Pipeline.Modules;
using Hermes.Pipes;

namespace Hermes.Messaging.EndPoints
{
    public abstract class LocalEndpoint<TContainerBuilder> : IDisposable
        where TContainerBuilder : IContainerBuilder, new()
    {
        private readonly Configure configuration;
        private bool disposed;

        public IMessageBus MessageBus { get { return Settings.RootContainer.GetInstance<IMessageBus>(); } }

        protected LocalEndpoint()
        {
            var containerBuilder = new TContainerBuilder();
            string endpointName = Assembly.GetAssembly(GetType()).GetName().Name;
            configuration = Configure.ClientEndpoint(endpointName, containerBuilder);
            ConfigureEndpoint(configuration);
            ConfigurePipeline(containerBuilder);
            Settings.IsSendOnly = true;
            Settings.AutoSubscribeEvents = false;
            Settings.RootContainer = containerBuilder.BuildContainer();
            Settings.FlushQueueOnStartup = true;
        }

        protected abstract void ConfigureEndpoint(IConfigureEndpoint configuration);

        public void Start()
        {
            configuration.Start();
        }

        protected virtual void ConfigurePipeline(TContainerBuilder containerBuilder)
        {
            var incomingPipeline = new ModulePipeFactory<IncomingMessageContext>()
                .Add<UnitOfWorkModule>()
                .Add<DispatchMessagesModule>();

            containerBuilder.RegisterSingleton(incomingPipeline); 
        }

        public void Stop()
        {
            configuration.Stop();
        }

        ~LocalEndpoint()
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