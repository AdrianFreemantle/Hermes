using System;
using System.Reflection;

namespace Hermes.Domain
{
    [Serializable]
    public class EventHandlerInvocationException : Exception
    {
        public EventHandlerInvocationException(EntityBase entity, IDomainEvent @event, TargetInvocationException ex)
            : base(GetErrorMessage(entity, @event), ex.InnerException ?? ex)
        {
        }

        private static string GetErrorMessage(EntityBase entity, IDomainEvent @event)
        {
            return String.Format("An error occured while invoking event handler {0} on {1}", @event.GetType().FullName, entity.GetType().FullName);
        }
    }
}