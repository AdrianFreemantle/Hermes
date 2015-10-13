using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainEventException : Exception
    {
        public DomainEventException(Type eventType)
            : base(GetErrorMessage(eventType))
        {
        }

        private static string GetErrorMessage(Type eventType)
        {
            return String.Format("Aggregate event {0} should implement {1}", eventType.FullName, typeof(IAggregateEvent).FullName);
        }
    }
}