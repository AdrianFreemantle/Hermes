using System.Configuration;

namespace Hermes.ServiceHost
{
    public class ServiceHostConfig : ConfigurationSection
    {
        static ServiceHostConfig serviceHostConfig;

        internal static ServiceHostConfig Settings
        {
            get { return serviceHostConfig; }
        }

        internal static void Create(string path)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(path);
            serviceHostConfig = (ServiceHostConfig)config.GetSection("ServiceHostConfig");

            if (serviceHostConfig == null)
            {
                serviceHostConfig = new ServiceHostConfig();
                config.Sections.Add("ServiceHostConfig", serviceHostConfig);
                config.Save(ConfigurationSaveMode.Modified);
            }
        }

        [ConfigurationProperty("name", DefaultValue = "HermesService", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string ServiceName
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("displayName", DefaultValue = "Hermes Service", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        public string DisplayName
        {
            get { return (string)this["displayName"]; }
            set { this["displayName"] = value; }
        }

        [ConfigurationProperty("description", DefaultValue = "Description", IsRequired = true)]
        public string Description
        {
            get { return (string)this["description"]; }
            set { this["description"] = value; }
        }
    }
}