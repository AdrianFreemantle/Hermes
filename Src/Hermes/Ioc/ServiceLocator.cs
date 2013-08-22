using System;
using System.Threading;

namespace Hermes.Ioc
{
    public class ServiceLocator : IServiceProvider
    {
        private static readonly ThreadLocal<ServiceLocator> instance = new ThreadLocal<ServiceLocator>();
        private static readonly object SyncRoot = new Object();
        private IServiceProvider serviceProvider;

        private ServiceLocator()
        {
            SetCurrentLifetimeScope(new DisposedProvider());
        }

        public static ServiceLocator Current
        {
            get { return GetInstance(); }
        }

        private static ServiceLocator GetInstance()
        {
            if (!instance.IsValueCreated)
            {
                lock (SyncRoot)
                {
                    if (!instance.IsValueCreated)
                    {
                        instance.Value = new ServiceLocator();
                    }
                }
            }

            return instance.Value;
        }

        public void SetCurrentLifetimeScope(IServiceProvider provider)
        {
            serviceProvider = provider ?? new DisposedProvider();
        }

        public object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }

        public TService GetService<TService>() where TService : class
        {
            return (TService)GetService(typeof(TService));
        }

        public bool HasServiceProvider()
        {
            return serviceProvider != null && !(serviceProvider is DisposedProvider);
        }
    }
}