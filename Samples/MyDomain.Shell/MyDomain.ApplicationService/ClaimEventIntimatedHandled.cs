using System;
using Hermes;
using Hermes.Messaging;

using MyDomain.Domain.Events;

namespace MyDomain.ApplicationService
{
    public class ClaimEventIntimatedHandled : IHandleMessage<ClaimEventIntimated>
    {
        private readonly IMessageBus messageBus;

        public ClaimEventIntimatedHandled(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(ClaimEventIntimated @event)
        {
            //messageBus.Send(new RegisterClaim
            //{
            //    ClaimId = Guid.NewGuid(),
            //    ClaimEventId = @event.Id,
            //    Amount = 100
            //});

            TestError.Throw();
        }
    }
}