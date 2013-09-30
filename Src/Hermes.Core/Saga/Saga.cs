using System;

using Hermes.Messaging;
using Hermes.Saga;

namespace Hermes.Core.Saga
{
    public abstract class Saga
    {
        protected internal abstract void Save();
    }

    public abstract class Saga<T> : Saga, ISaga<T> where T : class, IContainSagaData, new()
    {
        public IPersistSagas SagaPersistence { get; set; }
        public IMessageBus Bus { get; set; }
        public T State { get; protected set; }
        protected internal bool IsComplete { get; protected set; }

        protected virtual void Begin(Guid id)
        {
            State = new T
            {
                Id = id,
                OriginalMessageId = Bus.CurrentMessageContext.MessageId,
                Originator = Bus.CurrentMessageContext.ReplyToAddress.ToString()
            };

            SagaPersistence.Create(State);
        }

        protected virtual void Continue(Guid sagaId)
        {
            State = SagaPersistence.Get<T>(sagaId);
        }

        protected virtual void BeginOrContinue(Guid id)
        {
            var state = SagaPersistence.Get<T>(Bus.CurrentMessageContext.CorrelationId);

            if (state == null)
            {
                Begin(id);
            }
            else
            {
                State = state;
            }
        }

        protected internal override void Save()
        {
            if (IsComplete)
            {
                SagaPersistence.Complete(State.Id);
            }
            else
            {
                SagaPersistence.Update(State);
            }
        }        

        protected virtual void CompleteSaga()
        {
            IsComplete = true;
        }
    }
}
