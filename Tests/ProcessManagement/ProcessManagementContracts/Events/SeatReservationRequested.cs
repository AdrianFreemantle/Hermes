using System;
using System.Runtime.Serialization;

namespace ProcessManagement.Contracts.Events
{
    public interface ISeatReservationRequested : IEvent
    {
        Guid ReservationId { get; }
    }

    [DataContract]
    public class SeatReservationRequested : ISeatReservationRequested
    {
        [DataMember(Name = "ReservationId")]
        public Guid ReservationId { get; protected set; }

        public SeatReservationRequested(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}