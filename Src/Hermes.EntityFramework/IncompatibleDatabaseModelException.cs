using System;
using System.Runtime.Serialization;

namespace Hermes.EntityFramework
{
    [Serializable]
    public class IncompatibleDatabaseModelException : Exception
    {
        public IncompatibleDatabaseModelException()
        {
        }

        public IncompatibleDatabaseModelException(string message) : base(message)
        {
        }

        public IncompatibleDatabaseModelException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IncompatibleDatabaseModelException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}