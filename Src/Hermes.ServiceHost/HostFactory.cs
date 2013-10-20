using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            GetHostableService();

            return Topshelf.HostFactory.New(configurator =>
            {
                configurator.SetServiceName(hostableService.Assembly.GetVersionFormattedName());
                configurator.SetDisplayName(hostableService.Assembly.GetVersionFormattedName());
                configurator.SetDescription(GetDescription());
                configurator.RunAsPrompt();

                configurator.Service<ServiceHost>(s =>
                {
                    s.ConstructUsing(() => new ServiceHost(hostableService));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });
        }

        private static string GetDescription()
        {
            var descriptionAttribute = hostableService
                .Assembly
                .GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault();

            return descriptionAttribute != null ? descriptionAttribute.Description : hostableService.Assembly.GetVersionFormattedName();
        }

        private static void GetHostableService()
        {
            Type[] serviceTypes = FindAllServiceTypes();

            ValidateThatOnlyOneServiceIsPresent(serviceTypes);
            ValidateServiceTypeImplementsDefaultConstructor(serviceTypes.First());
            hostableService = serviceTypes.First();
        }

        private static Type[] FindAllServiceTypes()
        {
            using (var scanner = new AssemblyScanner())
            {
                return scanner.GetConcreteTypesOf<IService>().ToArray();
            }
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