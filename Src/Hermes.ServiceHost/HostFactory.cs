using System;
using System.IO;
using System.Linq;
using System.Text;

using Topshelf;

namespace Hermes.ServiceHost
{
    static class HostFactory
    {
        private static Type hostableService;

        public static string EndpointFile
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hostableService.Assembly.ManifestModule.Name); }
        }

        public static Host BuildHost()
        {
            hostableService = ScanForHostableServices();
            ValidateServiceTypeImplementsDefaultConstructor(hostableService);

            ServiceHostConfig.Create(EndpointFile);

            return Topshelf.HostFactory.New(configurator =>
            {
                configurator.SetServiceName(ServiceHostConfig.Settings.ServiceName);
                configurator.SetDisplayName(ServiceHostConfig.Settings.DisplayName);
                configurator.SetDescription(ServiceHostConfig.Settings.Description);
                configurator.RunAsPrompt();

                configurator.Service<ServiceHost>(s =>
                {
                    s.ConstructUsing(() => new ServiceHost(hostableService));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });
        }

        private static Type ScanForHostableServices()
        {
            Type[] serviceTypes = FindAllServiceTypes();

            ValidateThatAtLeastOneServiceIsPresent(serviceTypes);
            ValidateThatOnlyOneServiceIsPresent(serviceTypes);

            return serviceTypes.First();
        }

        private static Type[] FindAllServiceTypes()
        {
            Type[] serviceTypes;
            using (var scanner = new AssemblyScanner())
            {
                serviceTypes = scanner.GetConcreteTypesOf<IService>().ToArray();
            }
            return serviceTypes;
        }

        private static void ValidateThatAtLeastOneServiceIsPresent(Type[] serviceTypes)
        {
            if (serviceTypes == null || !serviceTypes.Any())
            {
                throw new TypeLoadException("Unable to locate any concrete implementations of IService.");
            }
        }

        private static void ValidateThatOnlyOneServiceIsPresent(Type[] serviceTypes)
        {
            if (serviceTypes.Count() != 1)
            {
                var services = new StringBuilder();
                services.AppendLine();

                foreach (var serviceType in serviceTypes)
                {
                    services.AppendLine(serviceType.FullName);
                }

                throw new InvalidOperationException(
                    String.Format(
                        "Only one service or endpoint is allowed per service host. The following services were detected: {0}",
                        services));
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