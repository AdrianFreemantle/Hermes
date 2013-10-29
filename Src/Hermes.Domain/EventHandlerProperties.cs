using System;
using System.Linq;
using System.Reflection;

namespace Hermes.Domain
{
    internal class EventHandlerProperties
    {
        private readonly MethodInfo handler;
        private readonly PropertyInfo aggregateIdentityProperty;
        private readonly PropertyInfo entityIdentityProperty;
        private readonly PropertyInfo versionProperty;
        private readonly PropertyBag eventProperties;
        private readonly Type eventType;
        private readonly Type entityType;
        private readonly bool eventMutatesEntityState = false;

        private EventHandlerProperties(MethodInfo method, Type entityType)
        {
            handler = method;
            this.entityType = entityType;
            eventType = method.GetParameters().First().ParameterType;
            
            eventProperties = new PropertyBag(eventType);
            aggregateIdentityProperty = GetAggregateIdentityProperty();
            entityIdentityProperty = GetEntityIdentityProperty();
            versionProperty = GetAggregateVersionProperty();

            eventMutatesEntityState = eventType.GetCustomAttribute<EventDoesNotMutateStateAttribute>() == null;
        }

        public static EventHandlerProperties CreateFromMethodInfo(MethodInfo method, Type entityType)
        {
            return new EventHandlerProperties(method, entityType);
        }

        //[DebuggerStepThrough]
        public void InvokeHandler(IDomainEvent e, EntityBase entity)
        {
            if (eventMutatesEntityState)
            {
                handler.Invoke(entity, new object[] {e});
            }
        }

        public bool EventIsOwnedByEntity(IDomainEvent e, EntityBase entity)
        {
            if (entity.GetType() != entityType)
                return false;

            if (e.GetType() != eventType)
                return false;

            if (entityIdentityProperty == null)
            {
                object identity = aggregateIdentityProperty.GetValue(e);
                return entity.Identity.Equals(identity);
            }
            else
            {
                object identity = entityIdentityProperty.GetValue(e);
                return entity.Identity.Equals(identity);
            }
        }

        public bool CanHandleEvent(IDomainEvent e)
        {
            return e.GetType() == eventType;
        }

        public void UpdateEventDetails(IDomainEvent e, IAggregate aggregate, EntityBase source)
        {
            versionProperty.SetValue(e, aggregate.GetVersion());
            aggregateIdentityProperty.SetValue(e, aggregate.Identity.GetId());

            if (entityIdentityProperty != null)
            {
                entityIdentityProperty.SetValue(e, source.Identity.GetId());
            }
        }

        private PropertyInfo GetEntityIdentityProperty()
        {
            if (typeof(Aggregate).IsAssignableFrom(entityType))
            {
                return null;
            }

            try
            {
                return eventProperties.GetPropertyWithAttribute<EntityIdAttribute>();
            }
            catch
            {
                throw new DomainEventEntityIdException(eventType);
            }
        }

        private PropertyInfo GetAggregateIdentityProperty()
        {
            try
            {
                return eventProperties.GetPropertyWithAttribute<AggregateIdAttribute>();
            }
            catch
            {
                throw new DomainEventAggregateIdException(eventType);
            }
        }

        private PropertyInfo GetAggregateVersionProperty()
        {
            try
            {
                return eventProperties.GetPropertyWithName("Version");
            }
            catch
            {
                throw new DomainEventException(eventType);
            }
        }
    }
}