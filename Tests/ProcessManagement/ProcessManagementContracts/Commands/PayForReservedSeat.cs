using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Commands
{
    [DataContract]
    public class PayForReservedSeat : ICommand
    {
        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public PayForReservedSeat(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}