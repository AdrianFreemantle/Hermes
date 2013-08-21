using System;

namespace Hermes.Configuration
{
    public class EnvironmentConfigurationException : Exception
    {
        public EnvironmentConfigurationException(string message)
            :base(message)
        {
        }
    }
}