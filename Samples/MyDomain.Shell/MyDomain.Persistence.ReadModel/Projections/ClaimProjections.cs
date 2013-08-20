using System;
using Hermes.Logging;
using Hermes.Messages;
using MyDomain.Domain.Events;
using MyDomain.Persistence.ReadModel.Models;

namespace MyDomain.Persistence.ReadModel.Projections
{
    public class ClaimProjections : IHandleMessage<ClaimRegistered>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(ClaimProjections)); 

        private readonly IRepository<Claim> repository;

        public ClaimProjections(IUnitOfWork unitOfWork)
        {
            repository = unitOfWork.GetRepository<Claim>();
        }

        public void Handle(ClaimRegistered @event)
        {
            Logger.Info("Projecting ClaimRegistered event");

            TestError.Throw();

            repository.Add(new Claim
            {
                Id = @event.ClaimId,
                ClaimEventId = @event.ClaimEventId,
                Amount = @event.Amount
            });

            TestError.Throw();
        }
    }
}