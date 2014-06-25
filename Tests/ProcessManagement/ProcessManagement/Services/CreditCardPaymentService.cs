using System;
using Hermes.Logging;
using Hermes.Messaging;
using ProcessManagement.Contracts.Commands;
using ProcessManagement.Contracts.Messages;

namespace ProcessManagement.Services
{
    public class CreditCardPaymentService : IHandleMessage<DebitCustomerCreditCard>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CreditCardPaymentService));
        private readonly IMessageBus bus;

        public CreditCardPaymentService(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(DebitCustomerCreditCard message)
        {
            Logger.Info("Debiting customer account for payment {0}.", message.PaymentId);

            var result = DateTime.Now.Ticks % 2 == 0;

            if (result)
            {
                Logger.Info("Debiting completed");
            }
            else
            {
                Logger.Warn("Debiting failed");
            }

            bus.Reply(new CreditCardDebitResult
            {
                PaymentCompleted = result
            });
        }
    }
}