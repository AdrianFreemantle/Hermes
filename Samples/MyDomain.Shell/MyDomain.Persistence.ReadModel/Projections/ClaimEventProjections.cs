using Hermes.Logging;
using Hermes.Messages;
using MyDomain.Domain.Events;
using MyDomain.Persistence.ReadModel.Models;

namespace MyDomain.Persistence.ReadModel.Projections
{
    public class ClaimEventProjections : IHandleMessage<ClaimEventIntimated>
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
    }
}

