using System;
using Hermes.Messaging;
using Hermes.Messaging.ProcessManagement;
using ProcessManagement.Contracts.Commands;
using ProcessManagement.Contracts.Events;
using ProcessManagement.Contracts.Messages;

namespace ProcessManagement.ProcessManager
{
    public class SeatReservationManager : ProcessManager<SeatReservationProcessManagerState>,
        IHandleMessage<ReserveSeat>,
        IHandleMessage<PayForReservedSeat>,
        IHandleMessage<TimeoutReservation>,
        IHandleMessage<CreditCardDebitResult>
    {
        public void Handle(ReserveSeat message)
        {
            Begin(message.ReservationId);

            Bus.Publish(new SeatReservationRequested(message.ReservationId));

            Timeout();
        }

        public void Handle(PayForReservedSeat message)
        {
            Continue(message.ReservationId);

            Bus.Send(State.Id, new DebitCustomerCreditCard(message.ReservationId));
        }

        public void Handle(CreditCardDebitResult message)
        {
            Continue(Bus.CurrentMessage.CorrelationId);

            if (message.PaymentCompleted)
            {
                Bus.Publish(new ReservationConfirmed(State.Id));
                ReplyWithResult(ReservationResultCode.Completed);
                Complete();
            }
            else
            {
                ReplyWithResult(ReservationResultCode.PaymentFailed);
                Timeout();
            }
        }

        public void Handle(TimeoutReservation message)
        {
            Continue(message.ReservationId);

            if (message.TimeoutId == State.TimeoutId)
            {
                Publish(new ReservationCancelled(State.Id));

                ReplyWithResult(ReservationResultCode.Timeout);

                Complete();
            }
        }

        private void ReplyWithResult(ReservationResultCode result)
        {
            ReplyToOriginator(new ReservationResult
            {
                Result = result,
                ReservationId = State.Id
            });
        }

        private void Timeout()
        {
            State.TimeoutId = Guid.NewGuid();
            Timeout(TimeSpan.FromMinutes(20), new TimeoutReservation(State.Id, State.TimeoutId));
        }
    }
}
