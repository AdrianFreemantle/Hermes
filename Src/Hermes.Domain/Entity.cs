namespace Hermes.Domain
{
    public class Entity : EntityBase
    {
        protected Aggregate Parent;

        protected Entity(Aggregate parent, IHaveIdentity identity)
            : base(identity)
        {
            SetParent(parent);
        }

        private void SetParent(Aggregate aggregate)
        {
            Parent = aggregate;
            Parent.RegisterOwnedEntity(this);
        }

        internal override void SaveEvent(DomainEvent @event)
        {
            @event.SetEntityDetails(this);
            Parent.SaveEvent(@event);
        }
    }
}