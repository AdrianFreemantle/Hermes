using System;
using System.Linq;
using System.Text;
using Hermes.Reflection;

namespace Hermes.ServiceHost
{
    static class HostFactory
    {
        public static HostableService GetHostableService()
        {
            Type[] serviceTypes = FindAllServiceTypes();

            ValidateThatOnlyOneServiceIsPresent(serviceTypes);
            ValidateServiceTypeImplementsDefaultConstructor(serviceTypes.First());
            return new HostableService(serviceTypes.First());
        }
      
        private static Type[] FindAllServiceTypes()
        {
            return AssemblyScanner.GetConcreteTypesOf<IEndpoint>().ToArray();
        }

        private static void ValidateThatOnlyOneServiceIsPresent(Type[] serviceTypes)
        {      
            if (!serviceTypes.Any())
            {
                throw new TypeLoadException("Unable to locate any concrete implementations of IService");
            }

            if (serviceTypes.Count() != 1)
            {
                var services = new StringBuilder("Only one service is allowed per service host. The following services were detected:\n");

                foreach (var serviceType in serviceTypes)
                {
                    services.AppendLine(serviceType.FullName);
                }

                throw new InvalidOperationException(services.ToString());
            }
        }

        private static void ValidateServiceTypeImplementsDefaultConstructor(Type serviceType)
        {
            if (!ObjectFactory.HasDefaultConstructor(serviceType))
            {
                throw new NotSupportedException(
                    String.Format("Service type {0} must implement a default constructor for it to be hostable",
                        serviceType.FullName));
            }
        }
    }
}