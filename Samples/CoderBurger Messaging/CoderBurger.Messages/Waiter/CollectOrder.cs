using System;
using Hermes.Messages;

namespace CoderBurger.Messages.Waiter
{
    public class CollectOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}