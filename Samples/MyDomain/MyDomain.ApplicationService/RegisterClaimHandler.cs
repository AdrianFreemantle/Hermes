using System.Collections.Generic;
using System.Linq;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService
{
    public class RegisterClaimHandler : IHandleMessage<RegisterClaim>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(RegisterClaimHandler));

        public void Handle(RegisterClaim command)
        {
            Logger.Info("Handling RegisterClaim");

            TestError.Throw();

            var claim = new Claim(command.Amount, command.ClaimEventId);

            IEnumerable<object> uncommittedEvents = ((IAggregate)claim).GetUncommittedEvents();
            
            foreach (var uncommittedEvent in uncommittedEvents)
            {
                DomainEvent.Current.Raise(uncommittedEvent);
            }

            ((IAggregate)claim).ClearUncommittedEvents();

            TestError.Throw();
        }
    }
}