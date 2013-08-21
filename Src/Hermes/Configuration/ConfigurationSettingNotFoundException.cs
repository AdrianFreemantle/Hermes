using System;

namespace Hermes.Configuration
{
    public class ConfigurationSettingNotFoundException : Exception
    {
        public ConfigurationSettingNotFoundException(string settingKey)
            :base(GetMessage(settingKey))
        {
        }

        private static string GetMessage(string settingKey)
        {
            return String.Format("No settings have been provided for {0}", settingKey);
        }
    }
}