using System;

namespace Hermes.Messaging.Configuration
{
    public class EnvironmentConfigurationException : Exception
    {
        public EnvironmentConfigurationException(string message)
            :base(message)
        {
        }       
    }
}