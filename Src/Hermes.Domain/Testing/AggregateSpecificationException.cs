using System;
using System.Runtime.Serialization;

namespace Hermes.Domain.Testing
{
    [Serializable]
    public class AggregateSpecificationException : Exception
    {
        public AggregateSpecificationException()
        {
        }

        public AggregateSpecificationException(string message)
            : base(message)
        {
        }

        public AggregateSpecificationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AggregateSpecificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}