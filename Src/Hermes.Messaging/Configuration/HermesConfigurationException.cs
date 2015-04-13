using System;
using System.Runtime.Serialization;

namespace Hermes.Messaging.Configuration
{
    [Serializable]
    public class HermesConfigurationException : Exception
    {
        public HermesConfigurationException()
        {
        }

        public HermesConfigurationException(string message) : base(message)
        {
        }

        public HermesConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected HermesConfigurationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}