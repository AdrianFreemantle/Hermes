using System;
using System.Collections.Generic;
using Hermes.Enums;
using Hermes.Logging;
using Hermes.Messaging;
using ProcessManagement.Contracts.Commands;
using ProcessManagement.Contracts.Messages;

namespace ProcessManagement.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var endpoint = new Endpoint();
            endpoint.Start();

            var reservationIds = new List<Guid>();

            for (int i = 0; i < 5000; i++)
            {
                reservationIds.Add(SequentialGuid.New());
            }

            foreach (var reservationId in reservationIds)
            {
                endpoint.MessageBus.Send(new PayForReservedSeat(reservationId));
            }

            reservationIds.Reverse();

            foreach (var reservationId in reservationIds)
            {
                endpoint.MessageBus.Send(new ReserveSeat(reservationId));
            }

            Console.ReadKey();
        }
    }

    public class Stub : IHandleMessage<ReservationResult>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (Stub));
        private readonly IMessageBus messageBus;

        public Stub(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Handle(ReservationResult message)
        {
            if (message.Result == ReservationResultCode.PaymentFailed)
            {
                Logger.Warn("Resending payment for reservation {0}", message.ReservationId);
                messageBus.Send(new PayForReservedSeat(message.ReservationId));
            }
            else
            {
                Logger.Info("Reservation {0} : {1}", message.ReservationId, message.Result.GetDescription());
            }
        }
    }
}
