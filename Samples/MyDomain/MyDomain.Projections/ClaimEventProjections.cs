using System;
using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;
using MyDomain.Domain.Events;
using MyDomain.Infrastructure;
using MyDomain.Persistence.ReadModel.Models;

namespace MyDomain.Projections
{
    public class ClaimEventProjections 
        : IHandleMessage<ClaimEventIntimated>
        , IHandleMessage<ClaimEventOpened>
        , IHandleMessage<ClaimEventClosed>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(ClaimEventProjections)); 

        private readonly IRepository<ClaimEvent> repository;

        public ClaimEventProjections(IUnitOfWork unitOfWork)
        {
            repository = unitOfWork.GetRepository<ClaimEvent>();
        }

        public void Handle(ClaimEventIntimated @event)
        {
            Logger.Info("Projecting ClaimEventIntimated event");

            TestError.Throw();

            repository.Add(new ClaimEvent
            {
                Id = (Guid)@event.AggregateId.GetId(),
                CreatedDate = @event.IntimatedTime,
                Open = true
            });

            TestError.Throw();
        }

        public void Handle(ClaimEventOpened message)
        {
            Logger.Info("Projecting ClaimEventOpened event");

            TestError.Throw();

            ClaimEvent claimEvent = repository.Get(message.AggregateId.GetId());
            claimEvent.Open = true;

            TestError.Throw();
        }

        public void Handle(ClaimEventClosed message)
        {
            Logger.Info("Projecting ClaimEventClosed event");

            TestError.Throw();

            ClaimEvent claimEvent = repository.Get(message.AggregateId.GetId());
            claimEvent.Open = false;

            TestError.Throw();
        }
    }
}

