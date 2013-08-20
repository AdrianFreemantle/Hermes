using Hermes.Logging;
using Hermes.Messages;
using MyDomain.Domain.Models;
using MyDomain.Infrastructure;

namespace MyDomain.ApplicationService
{
    public class RegisterClaimHandler : IHandleMessage<RegisterClaim>
    {
        private readonly EventStoreRepository repository;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(RegisterClaimHandler));

        public RegisterClaimHandler(EventStoreRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(RegisterClaim command)
        {
            Logger.Info("Handling RegisterClaim");
            var claimEvent = repository.GetById<ClaimEvent>(command.ClaimEventId);
            var claim = claimEvent.RegisterClaim(command.Amount);
                       
            Logger.Info("Saving Claim to event store");
            repository.Save(claim, claim.Id, objects => { });

            TestError.Throw();
        }
    }
}