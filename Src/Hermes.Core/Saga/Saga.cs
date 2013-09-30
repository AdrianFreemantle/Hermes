using System;

using Hermes.Messaging;
using Hermes.Saga;

namespace Hermes.Core.Saga
{
    public abstract class Saga<T> : ISaga<T> where T : class, IContainSagaData, new()
    {
        private readonly IPersistSagas sagaPersistence;
        protected readonly IMessageBus Bus;

        protected Saga(IPersistSagas sagaPersistence, IMessageBus bus)
        {
            this.sagaPersistence = sagaPersistence;
            Bus = bus;
        }

        public T State { get; set; }

        protected virtual void Begin()
        {
            Begin(Bus.CurrentMessageContext.CorrelationId);
        }

        protected virtual void Begin(Guid id)
        {
            var state = sagaPersistence.Get<T>(Bus.CurrentMessageContext.CorrelationId);

            if (state == null)
            {
                State = new T
                {
                    Id = id,
                    OriginalMessageId = Bus.CurrentMessageContext.MessageId,
                    Originator = Bus.CurrentMessageContext.ReplyToAddress.ToString()
                };

                sagaPersistence.Create(State);
            }
            else
            {
                State = state;
            }
        }

        protected virtual void Save()
        {
            sagaPersistence.Update(State);
        }

        protected virtual void Continue()
        {
            Continue(Bus.CurrentMessageContext.CorrelationId);
        }

        protected virtual void Continue(Guid sagaId)
        {
            State = sagaPersistence.Get<T>(sagaId);
        }

        protected virtual void Complete(Guid sagaId)
        {
            sagaPersistence.Complete(sagaId);
        }
    }
}
