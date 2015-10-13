using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainEventAggregateIdException : Exception
    {
        public DomainEventAggregateIdException(Type eventType)
            : base(GetErrorMessage(eventType))
        {
        }

        private static string GetErrorMessage(Type eventType)
        {
            return String.Format("Domain event {0} should have a single aggregate identity property that is decorated with attribute {1}.", eventType.FullName,  (typeof(AggregateIdAttribute).FullName));
        }
    }
}