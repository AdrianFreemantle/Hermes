using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Ioc
{
    public static class ServiceLocatorExtensions
    {
        public static bool TryGetInstance<TService>(this IServiceLocator serviceLocator, out object service)
        {
            return TryGetInstance(serviceLocator, typeof(TService), out service);
        }

        public static bool TryGetInstance(this IServiceLocator serviceLocator, Type serviceType, out object service)
        {
            service = null;

            try
            {
                service = serviceLocator.GetInstance(serviceType);
                return service != null;
            }
            catch (HermesComponentRegistrationException)
            {
                return false;
            }
        }

        public static bool TryGetAllInstances<TService>(this IServiceLocator serviceLocator, out IEnumerable<object> services)
        {
            return TryGetAllInstances(serviceLocator, typeof(TService), out services);
        }

        public static bool TryGetAllInstances(this IServiceLocator serviceLocator, Type serviceType, out IEnumerable<object> services)
        {
            services = null;

            try
            {
                services = serviceLocator.GetAllInstances(serviceType);
                return true;
            }
            catch (HermesComponentRegistrationException)
            {
                return false;
            }
        }
    }
}