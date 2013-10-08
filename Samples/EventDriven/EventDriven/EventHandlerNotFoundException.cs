using System;

namespace EventDriven
{
    public class EventHandlerNotFoundException : Exception
    {
        public EventHandlerNotFoundException(EntityBase entity, DomainEvent @event)
            :base(GetErrorMessage(entity, @event))
        {
            
        }

        private static string GetErrorMessage(EntityBase entity, DomainEvent @event)
        {
            return String.Format("Unable to locate an event handler for {0} on {1}", @event.GetType().FullName, entity.GetType().FullName);
        }
    }
}