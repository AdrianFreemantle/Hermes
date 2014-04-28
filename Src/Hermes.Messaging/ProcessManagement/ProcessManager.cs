using System;
using System.Linq.Expressions;

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

        protected virtual void Begin()
        {
            Begin(SequentialGuid.New());
        }

        protected virtual void Begin(Guid id)
        {
            State = new T
            {
                Id = id,
                OriginalMessageId = Bus.CurrentMessage.MessageId,
                Originator = Bus.CurrentMessage.ReplyToAddress.ToString(),
                Version = 0
            };

            IsNew = true;
        }

        protected virtual void Continue(Expression<Func<T, bool>> expression)
        {
            State = ProcessManagerPersistence.Find(expression);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(this);
            }
        }

        protected virtual void Continue(Guid id)
        {
            State = ProcessManagerPersistence.Get<T>(id);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(id, this);
            }
        }

        protected virtual void BeginOrContinue(Expression<Func<T, bool>> expression)
        {
            State = ProcessManagerPersistence.Find(expression);

            if (State == null)
            {
                Begin();
            }
        }

        protected virtual void BeginOrContinue(Guid id)
        {
            State = ProcessManagerPersistence.Get<T>(id);

            if (State == null)
            {
                Begin(id);
            }
        }

        protected internal override void Save()
        {
            State.Version++;

            if (IsNew && IsComplete)
            {
                return;
            }

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
