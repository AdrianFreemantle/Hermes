using System;
using System.Collections.Generic;

namespace Hermes.Configuration
{
    /// <summary>
    /// Used to store all infrastructure configuration settings
    /// </summary>
    public static class Settings
    {
        private static readonly Dictionary<string,object> settings = new Dictionary<string, object>();
        static IObjectBuilder builder;

        public static IObjectBuilder Builder
        {
            get
            {
                if (builder == null)
                    throw new InvalidOperationException("Please add a call to Configure.DefaultBuilder() or any of the other supported builders to set one up");

                return builder;
            }

            internal set { builder = value; }
        } 
 
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