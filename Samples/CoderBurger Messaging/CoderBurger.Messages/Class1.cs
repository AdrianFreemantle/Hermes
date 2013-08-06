using System;
using Hermes.Messages;

namespace CoderBurger.Messages
{
    public class OrderReady : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class OrderCollected : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class CustomerRefunded : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class OrderPlaced : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class OrderPaid : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class OrderCanceled : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class BurgerPrepared : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class FriesPrepared : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class DrinkPrepared : IEvent
    {
        public Guid OrderId { get; set; }
    }

    public class OrderAbandoned : IEvent
    {
        public Guid OrderId { get; set; }
    }
}
