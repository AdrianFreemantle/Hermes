using System;
using System.Linq;

using Topshelf;

namespace Hermes.ServiceHost
{
    static class HostFactory
    {
        public static Host BuildHost()
        {
            Type[] hostableServices = ScanForHostableServices();
            ValidateServiceTypesImplementDefaultConstructor(hostableServices);

            return Topshelf.HostFactory.New(configurator =>
            {
                configurator.SetServiceName("ClaimsApplicatonService");
                configurator.SetDisplayName("Claims Applicaton Service");
                configurator.SetDescription("Performs maintenance functions and message processing for the claims system");
                configurator.RunAsPrompt();

                configurator.Service<ServiceHost>(s =>
                {
                    s.ConstructUsing(() => new ServiceHost(hostableServices));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });
        }

        private static Type[] ScanForHostableServices()
        {
            Type[] serviceTypes;

            using (var scanner = new AssemblyScanner())
            {
                serviceTypes = scanner.GetConcreteTypesOf<IService>().ToArray();
            }

            if (serviceTypes == null || !serviceTypes.Any())
            {
                throw new TypeLoadException("Unable to locate any concrete implementations of IService.");
            }

            return serviceTypes;
        }

        private static void ValidateServiceTypesImplementDefaultConstructor(Type[] serviceTypes)
        {
            foreach (var serviceType in serviceTypes)
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
}