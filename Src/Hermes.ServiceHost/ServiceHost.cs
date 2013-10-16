using System;
using System.Threading;
using System.Threading.Tasks;

using Hermes.Logging;

namespace Hermes.ServiceHost
{
    class ServiceHost
    {
        static readonly ILog Logger = LogFactory.BuildLogger(typeof(ServiceHost));

        private readonly Type[] serviceTypes;
        private readonly object syncLock = new object();
        private CancellationTokenSource tokenSource;

        public ServiceHost(Type[] serviceTypes)
        {
            this.serviceTypes = serviceTypes;
        }

        public void Start()
        {
            lock (syncLock)
            {
                if (tokenSource != null && !tokenSource.IsCancellationRequested)
                {
                    Logger.Warn("Aborting service-host start request as the service is already running.");
                    return;
                }
                
                RunServices();
            }
        }

        public void Stop()
        {
            lock (syncLock)
            {
                Logger.Info("Sending termination signal to services.");
                tokenSource.Cancel();
            }
        }

        private void RunServices()
        {
            tokenSource = new CancellationTokenSource();

            try
            {
                foreach (var serviceType in serviceTypes)
                {
                    Logger.Verbose("Starting service {0}", serviceType.AssemblyQualifiedName);
                    var service = (IService)ObjectFactory.CreateInstance(serviceType);
                    Task.Factory.StartNew(() => service.Run(tokenSource.Token), tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                }
            }
            catch
            {
                tokenSource.Cancel();
                throw;
            }
        }
    }
}