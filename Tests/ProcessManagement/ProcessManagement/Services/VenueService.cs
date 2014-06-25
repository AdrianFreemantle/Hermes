using Hermes.Logging;
using Hermes.Messaging;
using ProcessManagement.Contracts.Events;

namespace ProcessManagement.Services
{
    public class VenueService : 
        IHandleMessage<IReservationConfirmed>,
        IHandleMessage<IReservationCancelled>,
        IHandleMessage<ISeatReservationRequested>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(VenueService));

        public void Handle(IReservationConfirmed message)
        {
            Logger.Info("Seat has been booked for reservation {0}.", message.ReservationId);
        }

        public void Handle(IReservationCancelled message)
        {
            Logger.Warn("Seat is no longer reserved for reservation {0}.", message.ReservationId);
        }

        public void Handle(ISeatReservationRequested message)
        {
            Logger.Info("Seat is reserved for reservation {0}.", message.ReservationId);
        }
    }
}