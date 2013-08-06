using System;
using Hermes.Messages;

namespace CoderBurger.Messages.Waiter
{
    public class PlaceOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}