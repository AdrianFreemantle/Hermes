using System;

namespace Hermes
{
    public class EnvironmentConfigurationException : Exception
    {
        public EnvironmentConfigurationException(string message)
            :base(message)
        {
        }
    }
}