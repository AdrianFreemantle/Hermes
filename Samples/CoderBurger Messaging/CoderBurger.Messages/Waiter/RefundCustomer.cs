using System;
using Hermes.Messages;

namespace CoderBurger.Messages
{
    public class RefundCustomer : ICommand
    {
        public Guid OrderId { get; set; }
    }
}