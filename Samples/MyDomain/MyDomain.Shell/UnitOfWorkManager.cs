using System.Collections.Generic;

using Hermes;
using Hermes.Configuration;

namespace MyDomain.Shell
{
    public class UnitOfWorkManager : IManageUnitOfWork
    {
        private readonly IUnitOfWork unitOfWork;

        public UnitOfWorkManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void Commit()
        {
            unitOfWork.Commit();
        }

        public void Rollback()
        {
            unitOfWork.Rollback();
        }
    }

    public interface IEventsToPublishUnitOfWork : IManageUnitOfWork
    {
        void AddEvent(object @event);
    }

    public class EventsToPublishUnitOfWork : IEventsToPublishUnitOfWork
    {
        private readonly HashSet<object> EventsToPublish = new HashSet<object>(); 
        
        public void AddEvent(object @event)
        {
            EventsToPublish.Add(@event);
        }

        public void Commit()
        {
            foreach (var @event in EventsToPublish)
            {
                Settings.MessageBus.Publish(@event);
            }
        }

        public void Rollback()
        {
            //no operation
        }
    }
}