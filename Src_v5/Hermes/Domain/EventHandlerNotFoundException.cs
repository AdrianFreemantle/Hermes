using System;

namespace Hermes.Domain
{
    [Serializable]
    public class EventHandlerNotFoundException : Exception
    {
        public EventHandlerNotFoundException(EntityBase entity, IAggregateEvent @event)
            :base(GetErrorMessage(entity, @event))
        {
            
        }

        private static string GetErrorMessage(EntityBase entity, IAggregateEvent @event)
        {
            return String.Format("Unable to locate an event handler for {0} on {1}", @event.GetType().FullName, entity.GetType().FullName);
        }
    }
}