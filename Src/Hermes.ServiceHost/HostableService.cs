using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Topshelf;

namespace Hermes.ServiceHost
{
    public class HostableService
    {
        private readonly Type hostableService;

        public HostableService(Type hostableService)
        {
            this.hostableService = hostableService;
        }

        public TopshelfExitCode Run()
        {
            var host = Topshelf.HostFactory.New(configurator =>
            {
                configurator.SetServiceName(GetServiceName());
                configurator.SetDisplayName(GetServiceName());
                configurator.SetDescription(GetDescription());
                configurator.RunAsPrompt();

                configurator.Service<ServiceHost>(s =>
                {
                    s.ConstructUsing(() => new ServiceHost(hostableService));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });

            return host.Run();
        }

        public string GetDescription()
        {
            var descriptionAttribute = GetServiceAttribute<AssemblyDescriptionAttribute>();

            if (descriptionAttribute == null || String.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                return "A service hosted by Hermes Service-Host";
            }

            return descriptionAttribute.Description;
        }

        public string GetServiceFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hostableService.Assembly.ManifestModule.Name);
        }

        public string GetServiceName()
        {
            var titleAttribute = GetServiceAttribute<AssemblyTitleAttribute>();

            if (titleAttribute == null || String.IsNullOrWhiteSpace(titleAttribute.Title))
            {
                return String.Format("Hermes.{0}", hostableService.Assembly.GetName().Name); 
            }

            return String.Format("Hermes.{0}", titleAttribute.Title); 
        }

        private T GetServiceAttribute<T>() where T : Attribute
        {
            return hostableService
                .Assembly
                .GetCustomAttributes(typeof (T), false)
                .OfType<T>()
                .FirstOrDefault();
        }

        public string GetConfigurationFilePath()
        {
            return GetServiceFilePath() + ".config";
        }
    }
}