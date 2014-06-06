using System;
using System.Linq.Expressions;
using Hermes.Logging;

namespace Hermes.Messaging.ProcessManagement
{
    public abstract class ProcessManager
    {
        protected readonly ILog Logger;

        public bool IsComplete { get; protected set; }
        public bool IsNew { get; protected set; }
        public IMessageBus Bus { get; set; }
        public IPersistProcessManagers ProcessManagerPersistence { get; set; }
        internal abstract void Save();
        internal abstract Guid Id { get; }

        protected ProcessManager()
        {
            Logger = LogFactory.BuildLogger(GetType());
        }
    }

    public abstract class ProcessManager<T> : ProcessManager, IProcessManager<T> where T : class, IContainProcessManagerData, new()
    {
        public T State { get; protected set; }

        internal override Guid Id
        {
            get { return State.Id; }
        }
        
        protected virtual void Begin()
        {
            Begin(SequentialGuid.New());
        }

        protected virtual void Begin(Guid id)
        {
            Logger.Debug("Beginning ProcessManager with Id {0}", id);

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
            Logger.Debug("Attempting to continue ProcessManager through an expression.");

            State = ProcessManagerPersistence.Find(expression);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(this);
            }
        }

        protected virtual void Continue(Guid id)
        {
            Logger.Debug("Continuing ProcessManager with Id {0}", id);

            State = ProcessManagerPersistence.Get<T>(id);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(id, this);
            }
        }

        protected virtual void BeginOrContinue(Expression<Func<T, bool>> expression)
        {
            Logger.Debug("Attempting to begin or continue a ProcessManager through an expression.");

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

        protected void Send(object command)
        {
            Bus.Send(State.Id, command);
        }

        protected void Publish(object @event)
        {
            Bus.Publish(State.Id, @event);
        }

        protected void Timeout(TimeSpan timeSpan, object command)
        {
            Bus.Defer(timeSpan, State.Id, command);
        }

        internal override void Save()
        {
            State.Version++;

            if (IsNew && IsComplete)
            {
                return;
            }

            if (IsNew)
            {
                Logger.Debug("Creating ProcessManager with Id {0}", State.Id);
                ProcessManagerPersistence.Create(State);
            }
            else if (IsComplete)
            {
                Logger.Debug("Completeing ProcessManager with Id {0}", State.Id);
                ProcessManagerPersistence.Complete<T>(State.Id);
            }
            else
            {
                Logger.Debug("Updating ProcessManager with Id {0}", State.Id);
                ProcessManagerPersistence.Update(State);
            }
        }        

        protected virtual void Complete()
        {
            IsComplete = true;
        }
    }
}
