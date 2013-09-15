using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Events;
using MyDomain.Persistence.ReadModel.Models;

namespace MyDomain.Persistence.ReadModel.Projections
{
    public class ClaimEventProjections : IHandleMessage<ClaimEventIntimated>, IHandleMessage<ChangedIntimatedDate>
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
                Id = @event.Id,
                CreatedDate = @event.IntimatedTime
            });

            TestError.Throw();
        }

        public void Handle(ChangedIntimatedDate @event)
        {
            Logger.Info("Projecting ChangedIntimatedDate event");

            TestError.Throw();

            var claimEvent = repository.Get(@event.Id);
            claimEvent.CreatedDate = @event.IntimatedTime;

            TestError.Throw();
        }
    }
}

