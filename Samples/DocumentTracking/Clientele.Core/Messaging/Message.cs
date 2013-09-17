using System;

namespace Clientele.Core.Messaging
{
    public abstract class Message
    {
        public Guid MessageId { get; protected set; }
        public DateTime OccurredAt { get; protected set; }

        protected Message()
        {
            MessageId = CombFactory.NewComb();
            OccurredAt = DateTime.Now;
        }
    }
}