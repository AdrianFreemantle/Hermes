using System;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Models;
using MyDomain.Infrastructure;

namespace MyDomain.ApplicationService
{
    public class IntimateClaimEventHandler : IHandleMessage<IntimateClaimEvent>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IntimateClaimEventHandler)); 

        private readonly EventStoreRepository repository;

        public IntimateClaimEventHandler(EventStoreRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(IntimateClaimEvent command)
        {
            Logger.Info("Handling IntimateClaim");
            var claimEvent = ClaimEvent.Intimate(command.Id);

            Logger.Info("Saving ClaimEvent to event store");
            repository.Save(claimEvent, command.MessageId, objects => { });

            TestError.Throw();
        }
    }
}
