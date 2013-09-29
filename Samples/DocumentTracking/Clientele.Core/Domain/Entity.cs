using System.Diagnostics;

namespace Clientele.Core.Domain
{
    [DebuggerStepThrough]
    public class Entity : EntityBase
    {
        private IAggregate parent;

        protected Entity(IAggregate parent, IHaveIdentity identity)
            : base(identity)
        {
            SetParent(parent);
        }

        private void SetParent(IAggregate aggregate)
        {
            parent = aggregate;
            parent.RegisterOwnedEntity(this);
        }

        protected override void SaveEvent(IDomainEvent @event)
        {
            @event.EntityId = Identity;
            parent.SaveEvent(@event);
        }
    }
}