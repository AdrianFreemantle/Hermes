using System;

namespace Hermes.Messaging.ProcessManagement
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
        protected internal bool IsNew { get; protected set; }

        protected virtual void Begin(Guid id)
        {
            State = new T
            {
                Id = id,
                OriginalMessageId = Bus.CurrentMessage.MessageId,
                Originator = Bus.CurrentMessage.ReplyToAddress.ToString(),
                Version = 1
            };

            IsNew = true;
        }

        protected virtual void Continue(Guid id)
        {
            State = ProcessManagerPersistence.Get<T>(id);
        }

        protected virtual void BeginOrContinue(Guid id)
        {
            var state = ProcessManagerPersistence.Get<T>(id);

            if (state == null)
            {
                Begin(id);
            }
            else
            {
                State = state;
                State.Version++;
            }
        }

        protected internal override void Save()
        {
            if (IsNew)
            {
                ProcessManagerPersistence.Create(State);
            }
            else if (IsComplete)
            {
                ProcessManagerPersistence.Complete<T>(State.Id);
            }
            else
            {
                ProcessManagerPersistence.Update(State);
            }
        }        

        protected virtual void Complete()
        {
            IsComplete = true;
        }
    }
}
