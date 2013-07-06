using System.Collections.Generic;

namespace Hermes
{
    public static class Configuration
    {
        public const string ConnectionString = "Hermes.ConnectionString";

        private static readonly Dictionary<string,object> settings = new Dictionary<string, object>();
 
        public static T GetSetting<T>(string settingKey)
        {
            if (settings.ContainsKey(settingKey))
            {
                return (T)settings[settingKey];
            }

            throw new ConfigurationSettingNotFoundException(settingKey);
        }

        public static void AddSetting(string settingKey, object value)
        {
            if (settings.ContainsKey(settingKey))
            {
                settings.Remove(settingKey);
            }

            settings.Add(settingKey, value);
        }
    }
}