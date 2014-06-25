using Hermes.Logging;
using Hermes.Messaging;
using ProcessManagement.Contracts.Events;

namespace ProcessManagement.Services
{
    public class EmailService :
        IHandleMessage<IReservationConfirmed>,
        IHandleMessage<IReservationCancelled>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(EmailService));

        public void Handle(IReservationConfirmed message)
        {
            Logger.Info("Sending confirmation email of booking for reservation {0}.", message.ReservationId);
        }

        public void Handle(IReservationCancelled message)
        {
            Logger.Warn("Sending cancellation email for reservation {0}.", message.ReservationId);
        }
    }
}