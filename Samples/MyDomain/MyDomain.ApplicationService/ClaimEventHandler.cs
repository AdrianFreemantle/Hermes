using Hermes.Logging;
using Hermes.Messaging;
using MyDomain.ApplicationService.Commands;
using MyDomain.Domain.Models;
using MyDomain.Infrastructure;

namespace MyDomain.ApplicationService
{
    public class ClaimEventHandler 
        : IHandleMessage<IntimateClaim>
        , IHandleMessage<CloseClaim>
        , IHandleMessage<OpenClaim>
    {
        private readonly IEventStoreRepository repository;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(ClaimEventHandler));

        public ClaimEventHandler(IEventStoreRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(IntimateClaim command)
        {
            Logger.Info("Handling IntimateClaim");
            var claimEvent = ClaimEvent.Intimate(command.ClaimEventId);

            repository.Save(claimEvent, command.CommandId, objects => { });
        }

        public void Handle(CloseClaim command)
        {
            Logger.Info("Handling CloseClaimEvent");
            var claimEvent = repository.GetById<ClaimEvent>(command.ClaimEventId);
            claimEvent.Close();

            repository.Save(claimEvent, command.CommandId, objects => { });
        }

        public void Handle(OpenClaim command)
        {
            Logger.Info("Handling OpenClaimEvent");
            var claimEvent = repository.GetById<ClaimEvent>(command.ClaimEventId);
            claimEvent.Open();

            repository.Save(claimEvent, command.CommandId, objects => { });
        }
    }   
}
