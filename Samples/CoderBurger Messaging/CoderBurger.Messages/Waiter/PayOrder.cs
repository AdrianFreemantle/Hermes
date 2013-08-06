using System;
using Hermes.Messages;

namespace CoderBurger.Messages
{
    public class PayOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}