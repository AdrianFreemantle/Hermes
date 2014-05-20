using System;
using Hermes.Domain;

namespace Hermes.EntityFramework
{
    internal struct AggregateCommitAction : IEquatable<AggregateCommitAction>
    {
        internal enum CommitActionType
        {
            Add,
            Update,
            Remove
        }

        private readonly IIdentity identity;
        private readonly CommitActionType actionType;

        public IIdentity Identity
        {
            get { return identity; }
        }

        public CommitActionType ActionType
        {
            get { return actionType; }
        }

        private AggregateCommitAction(IIdentity identity, CommitActionType actionType)
        {
            this.identity = identity;
            this.actionType = actionType;
        }

        public static AggregateCommitAction Add(IIdentity identity)
        {
            return new AggregateCommitAction(identity, CommitActionType.Add);
        }

        public static AggregateCommitAction Update(IIdentity identity)
        {
            return new AggregateCommitAction(identity, CommitActionType.Update);
        }

        public static AggregateCommitAction Remove(IIdentity identity)
        {
            return new AggregateCommitAction(identity, CommitActionType.Remove);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Identity.GetHashCode() * 397) ^ ActionType.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is AggregateCommitAction && Equals((AggregateCommitAction)obj);
        }

        public bool Equals(AggregateCommitAction other)
        {
            return other.identity.Equals(identity) && other.actionType == actionType;
        }

        public static bool operator ==(AggregateCommitAction left, AggregateCommitAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AggregateCommitAction left, AggregateCommitAction right)
        {
            return !Equals(left, right);
        }
    }
}