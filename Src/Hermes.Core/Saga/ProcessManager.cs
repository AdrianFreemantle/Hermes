using System;

using Hermes.Messaging;
using Hermes.Saga;

namespace Hermes.Core.Saga
{
    public abstract class ProcessManager
    {
        protected internal abstract void Save();
    }

    public abstract class ProcessManager<T> : ProcessManager, IProcessManager<T> where T : class, IContainProcessManagerData, new()
    {
        public IPersistProcessManagers ProcessManagerPersistence { get; set; }
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

            ProcessManagerPersistence.Create(State);
        }

        protected virtual void Continue(Guid sagaId)
        {
            State = ProcessManagerPersistence.Get<T>(sagaId);
        }

        protected virtual void BeginOrContinue(Guid id)
        {
            var state = ProcessManagerPersistence.Get<T>(Bus.CurrentMessageContext.CorrelationId);

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
                ProcessManagerPersistence.Complete(State.Id);
            }
            else
            {
                ProcessManagerPersistence.Update(State);
            }
        }        

        protected virtual void CompleteSaga()
        {
            IsComplete = true;
        }
    }
}
