using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Commands
{
    [DataContract]
    public class ReserveSeat : ICommand
    {
        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public ReserveSeat(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}