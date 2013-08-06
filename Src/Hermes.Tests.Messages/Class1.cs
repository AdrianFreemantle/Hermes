using System;
using Hermes.Messages;

namespace Hermes.Tests.Messages
{
    public class SellShoes : ICommand
    {
        public int ShoeTypeId { get; set; }
        public int Size { get; set; }
    }

    public class ShoesSold : IEvent
    {
        public int ShoeTypeId { get; set; }
        public int Size { get; set; }
        public string OrderNumber { get; set; }
    }

    public class OrderShipped : IEvent
    {
        public string OrderNumber { get; set; }
    }

    public class TestEvent : IEvent
    {
        public string Message { get; set; }
    }
}
