using System;
using System.Runtime.Serialization;

namespace Hermes.Ioc
{
    [Serializable]
    public class HermesComponentRegistrationException : Exception
    {
        public HermesComponentRegistrationException()
        {
        }

        public HermesComponentRegistrationException(string message) : base(message)
        {
        }

        public HermesComponentRegistrationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected HermesComponentRegistrationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}