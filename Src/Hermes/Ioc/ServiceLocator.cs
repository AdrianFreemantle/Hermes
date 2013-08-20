using System;

namespace Hermes.Ioc
{
    public class ServiceLocator : IServiceProvider
    {
        [ThreadStatic]
        private static volatile ServiceLocator instance;
        private static readonly object SyncRoot = new Object();
        private IServiceProvider serviceProvider;

        private ServiceLocator()
        {
            SetServiceProvider(null);
        }

        public static ServiceLocator Current
        {
            get { return GetInstance(); }
        }

        private static ServiceLocator GetInstance()
        {
            if (instance == null)
            {
                lock (SyncRoot)
                {
                    if (instance == null)
                    {
                        instance = new ServiceLocator();
                    }
                }
            }

            return instance;
        }

        public void SetServiceProvider(IServiceProvider provider)
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
            return serviceProvider != null;
        }
    }
}