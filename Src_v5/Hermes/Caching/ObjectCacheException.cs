using System;
using System.Runtime.Serialization;

namespace Hermes.Caching
{
    [Serializable]
    public class ObjectCacheException : Exception
    {
        public ObjectCacheException()
        {
        }

        public ObjectCacheException(string message) : base(message)
        {
        }

        public ObjectCacheException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        protected ObjectCacheException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}