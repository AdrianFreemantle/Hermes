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

        private string GetDescription()
        {
            var descriptionAttribute = hostableService
                .Assembly
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault();

            return descriptionAttribute != null ? descriptionAttribute.Description : GetServiceName();
        }

        public string GetServiceFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hostableService.Assembly.ManifestModule.Name);
        }

        public string GetServiceName()
        {
            return String.Format("Hermes.{0}", hostableService.Assembly.GetName().Name);
        }

        public string GetConfigurationFilePath()
        {
            return GetServiceFilePath() + ".config";
        }
    }
}