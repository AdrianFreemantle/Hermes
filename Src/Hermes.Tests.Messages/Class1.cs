using System;

namespace Hermes.Tests.Messages
{
    public class SellShoes 
    {
        public int ShoeTypeId { get; set; }
        public int Size { get; set; }
    }

    public class ShoesSold 
    {
        public int ShoeTypeId { get; set; }
        public int Size { get; set; }
        public string OrderNumber { get; set; }
    }

    public class OrderShipped 
    {
        public string OrderNumber { get; set; }
    }

    public class TestEvent 
    {
        public string Message { get; set; }
    }
}
