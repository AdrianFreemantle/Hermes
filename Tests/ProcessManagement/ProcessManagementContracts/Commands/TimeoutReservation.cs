using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Commands
{
    [DataContract]
    public class TimeoutReservation : ICommand
    {
        [DataMember(Name = "TimeoutId")]
        public Guid TimeoutId { get; protected set; }

        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public TimeoutReservation(Guid reservationId, Guid timeoutId)
        {
            this.TimeoutId = timeoutId;
            ReservationId = reservationId;
        }
    }
}