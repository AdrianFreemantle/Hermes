namespace Clientele.Core.Domain
{
    public abstract class TypedAggregate<TAggregateState> : Aggregate where TAggregateState : class, new()
    {
        protected TAggregateState State;

        protected TypedAggregate(IHaveIdentity identity)
            : base(identity)
        {
            State = new TAggregateState();
        }

        protected override void ApplyEvent(IDomainEvent @event)
        {
            ((dynamic)State).When((dynamic)@event);
        }
    }
}