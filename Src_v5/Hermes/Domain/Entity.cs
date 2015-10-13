namespace Hermes.Domain
{
    public class Entity : EntityBase
    {
        protected Aggregate Parent;

        protected Entity(Aggregate parent, IIdentity identity)
            : base(identity)
        {
            SetParent(parent);
        }

        private void SetParent(Aggregate aggregate)
        {
            Parent = aggregate;
            Parent.RegisterOwnedEntity(this);
        }

        internal protected override void SaveEvent(IAggregateEvent @event, EntityBase source)
        {
            Parent.SaveEvent(@event, source);
        }
    }
}