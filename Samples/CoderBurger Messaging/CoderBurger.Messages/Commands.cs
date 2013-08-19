using System;
using Hermes.Messages;

namespace CoderBurger.Messages
{
    public class AbandonOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }

    public class PayOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }

    public class PlaceOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }

    public class RefundCustomer : ICommand
    {
        public Guid OrderId { get; set; }
    }

    public class CollectOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }

    public class CancelOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}