namespace Clientele.Core.Domain
{
    public abstract class RestorableTypedAggregate<TAggregateState> : TypedAggregate<TAggregateState> where TAggregateState : class, IMemento, new()
    {
        protected RestorableTypedAggregate(IHaveIdentity identity)
            : base(identity)
        {
            State.Identity = identity;
        }

        protected override void RestoreSnapshot(IMemento memento)
        {
            State = (TAggregateState)memento;
        }

        protected override IMemento GetSnapshot()
        {
            return State;
        }
    }
}