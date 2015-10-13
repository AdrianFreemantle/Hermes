using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainEventEntityIdException : Exception
    {
        public DomainEventEntityIdException(Type eventType)
            : base(GetErrorMessage(eventType))
        {
        }

        private static string GetErrorMessage(Type eventType)
        {
            return String.Format("Domain event {0} is handled by an entity and should therefore have a single entity identity property that has attribute {1}.", eventType.FullName, (typeof(EntityIdAttribute).FullName));
        }
    }
}