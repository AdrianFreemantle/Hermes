using System;
using System.Linq.Expressions;
using Hermes.Logging;
using Hermes.Scheduling;

namespace Hermes.Messaging.ProcessManagement
{
    public abstract class ProcessManager<T> : IProcessManager 
        where T : class, IContainProcessManagerData, new()
    {
        protected readonly ILog Logger;
        protected T State;

        public virtual IMessageBus Bus { get; set; }
        public virtual IPersistProcessManagers ProcessManagerPersistence { get; set; }
        public virtual bool IsComplete { get; private set; }
        public virtual bool IsNew { get; private set; }

        protected ProcessManager()
        {
            Logger = LogFactory.BuildLogger(GetType());
        }

        IContainProcessManagerData IProcessManager.GetCurrentState()
        {
            if (State == null)
                throw new ProcessManagerNotInitializedException(this);

            return State;
        }

        protected virtual void Begin()
        {
            Begin(Bus.CurrentMessage.CorrelationId);
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

            State = ((IProcessManager)this).ProcessManagerPersistence.Find(expression);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(this);
            }
        }

        protected virtual void Continue()
        {
            Continue(Bus.CurrentMessage.CorrelationId);
        }

        protected virtual void Continue(Guid id)
        {
            Logger.Debug("Continuing ProcessManager with Id {0}", id);

            State = ((IProcessManager)this).ProcessManagerPersistence.Get<T>(id);

            if (State == null)
            {
                throw new ProcessManagerDataNotFoundException(id, this);
            }
        }

        protected virtual void BeginOrContinue(Expression<Func<T, bool>> expression)
        {
            Logger.Debug("Attempting to begin or continue a ProcessManager through an expression.");

            State = ((IProcessManager)this).ProcessManagerPersistence.Find(expression);

            if (State == null)
            {
                Begin();
            }
        }

        protected virtual void BeginOrContinue()
        {
            BeginOrContinue(Bus.CurrentMessage.CorrelationId);
        }

        protected virtual void BeginOrContinue(Guid id)
        {
            State = ((IProcessManager)this).ProcessManagerPersistence.Get<T>(id);

            if (State == null)
            {
                Begin(id);
            }
        }

        protected void Send(object command)
        {
            var currentState = ((IProcessManager)this).GetCurrentState();

            Bus.Send(currentState.Id, command);
        }

        protected void ReplyToOriginator(object message)
        {
            var currentState = ((IProcessManager)this).GetCurrentState();

            Bus.Reply(Address.Parse(currentState.Originator), currentState.OriginalMessageId, message);
        }

        protected void Publish(object @event)
        {
            var currentState = ((IProcessManager)this).GetCurrentState();

            Bus.Publish(currentState.Id, @event);
        }

        protected void Timeout(TimeSpan timeSpan, object command)
        {
            var currentState = ((IProcessManager)this).GetCurrentState();

            Bus.Defer(timeSpan, currentState.Id, command);
        }

        protected void Timeout(TimeSpan timeSpan, CronSchedule schedule, object command)
        {
            var currentState = ((IProcessManager)this).GetCurrentState();
            var timeoutTime = schedule.GetTimeUntilNextOccurrence();
            Bus.Defer(timeoutTime, currentState.Id, command);
        }

        protected void Complete(Expression<Func<T, bool>> expression)
        {
            BeginOrContinue(expression);
            Complete();
        }
        
        protected void Complete(Guid id)
        {
            BeginOrContinue(id);
            Complete();
        }              

        protected void Complete()
        {
            IsComplete = true;
        }

        void IProcessManager.Save()
        {
            var currentState = ((IProcessManager)this).GetCurrentState();

            currentState.Version++;

            if (IsNew && IsComplete)
            {
                return;
            }

            if (IsNew)
            {
                Logger.Debug("Creating ProcessManager with Id {0}", State.Id);
                ((IProcessManager)this).ProcessManagerPersistence.Create(State);
            }
            else if (IsComplete)
            {
                Logger.Debug("Completeing ProcessManager with Id {0}", State.Id);
                ((IProcessManager)this).ProcessManagerPersistence.Complete<T>(State.Id);
            }
            else
            {
                Logger.Debug("Updating ProcessManager with Id {0}", State.Id);
                ((IProcessManager)this).ProcessManagerPersistence.Update(State);
            }
        }        
    }
}
