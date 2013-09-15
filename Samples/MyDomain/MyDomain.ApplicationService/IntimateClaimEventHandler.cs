using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService
{
    public class IntimateClaimEventHandler : IHandleMessage<IntimateClaimEvent>
    {
        private readonly IEventStoreRepository repository;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IntimateClaimEventHandler));

        public IntimateClaimEventHandler(IEventStoreRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(IntimateClaimEvent command)
        {
            Logger.Info("Handling IntimateClaim");
            var claimEvent = ClaimEvent.Intimate(command.Id);

            repository.Save(claimEvent, claimEvent.Id, objects => { });
        }


    }   
}
