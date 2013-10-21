using System;
using System.ComponentModel;

namespace Starbucks.Messages
{
    public class OrderCoffee : ICommand
    {
        public Guid OrderNumber { get; set; }
        public Coffee Coffee { get; set; }
    }

    public class OrderReady : IMessage
    {
        public Guid OrderNumber { get; set; }
        public string Coffee { get; set; }
    }

    public enum Coffee
    {
        Filter,
        Espresso
    }

    public enum ErrorCodes
    {
        Timeout = -1,
        Success = 0,
        [Description("Out of coffee")]
        OutOfCoffee = 1
    }

    public interface ICommand
    {
    }

    public interface IMessage
    {
    }

}
