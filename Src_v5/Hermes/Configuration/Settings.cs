using System;
using System.Collections.Generic;
using System.Configuration;

namespace Hermes.Configuration
{
    /// <summary>
    /// Used to store all infrastructure configuration settings
    /// </summary>
    public static class Settings
    {
        private static readonly Dictionary<string, object> ConfigurationSettings = new Dictionary<string, object>();

        public static T Get<T>(string settingKey)
        {
            return (T)Get(settingKey);
        }

        public static object Get(string settingKey)
        {
            if (ConfigurationSettings.ContainsKey(settingKey))
            {
                return ConfigurationSettings[settingKey];
            }

            if (ConfigurationManager.AppSettings[settingKey] != null)
            {
                return ConfigurationManager.AppSettings[settingKey];
            }            

            throw new ConfigurationSettingNotFoundException(settingKey);
        }

        public static void Add(string key, object value)
        {
            ConfigurationSettings[key] = value;
        }
    }
}