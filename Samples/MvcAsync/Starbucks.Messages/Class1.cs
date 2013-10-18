using System;

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
        Success = 0,
        OutOfCoffee = 1
    }

    public interface ICommand
    {
    }

    public interface IMessage
    {
    }

}
