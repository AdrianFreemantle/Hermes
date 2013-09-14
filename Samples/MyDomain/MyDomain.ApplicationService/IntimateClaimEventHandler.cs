using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService
{
    public class IntimateClaimEventHandler : IHandleMessage<IntimateClaimEvent>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(IntimateClaimEventHandler)); 

        public void Handle(IntimateClaimEvent command)
        {
            Logger.Info("Handling IntimateClaim");
            var claimEvent = ClaimEvent.Intimate(command.Id);

            TestError.Throw();

            IEnumerable<object> uncommittedEvents = ((IAggregate)claimEvent).GetUncommittedEvents();

            foreach (var uncommittedEvent in uncommittedEvents)
            {
                DomainEvent.Current.Raise(uncommittedEvent);
            }
                
            ((IAggregate)claimEvent).ClearUncommittedEvents();

            TestError.Throw();
        }
    }
}
