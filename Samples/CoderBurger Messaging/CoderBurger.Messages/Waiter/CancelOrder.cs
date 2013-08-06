using System;
using Hermes.Messages;

namespace CoderBurger.Messages.Waiter
{
    public class CancelOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}