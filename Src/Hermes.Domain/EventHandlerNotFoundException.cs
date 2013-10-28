using System;

namespace Hermes.Domain
{
    [Serializable]
    public class EventHandlerNotFoundException : Exception
    {
        public EventHandlerNotFoundException(EntityBase entity, IDomainEvent @event)
            :base(GetErrorMessage(entity, @event))
        {
            
        }

        private static string GetErrorMessage(EntityBase entity, IDomainEvent @event)
        {
            return String.Format("Unable to locate an event handler for {0} on {1}", @event.GetType().FullName, entity.GetType().FullName);
        }
    }
}