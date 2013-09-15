using System.Collections.Generic;
using System.Linq;

using Hermes.Core;
using Hermes.Logging;
using Hermes.Messaging;

using MyDomain.Domain.Models;

namespace MyDomain.ApplicationService
{
    public class RegisterClaimHandler : IHandleMessage<RegisterClaim>
    {
        private readonly IEventStoreRepository repository;

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(RegisterClaimHandler));

        public RegisterClaimHandler(IEventStoreRepository repository)
        {
            this.repository = repository;
        }

        public void Handle(RegisterClaim command)
        {
            Logger.Info("Handling RegisterClaim {0}", command.ClaimId);

            var claim = new Claim(command.ClaimId, command.Amount, command.ClaimEventId);
          
            repository.Save(claim, claim.Id, objects => { });
            TestError.Throw();
        }


    }
}