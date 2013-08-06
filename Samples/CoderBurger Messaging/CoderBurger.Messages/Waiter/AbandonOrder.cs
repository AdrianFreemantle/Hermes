using System;
using Hermes.Messages;

namespace CoderBurger.Messages
{
    public class AbandonOrder : ICommand
    {
        public Guid OrderId { get; set; }
    }
}