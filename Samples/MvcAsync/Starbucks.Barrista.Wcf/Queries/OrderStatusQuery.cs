using System;

using Hermes;

namespace Starbucks.Barrista.Wcf.Queries
{
    public class OrderStatusQuery : IReturn<OrderStatusQueryResult>
    {
        public Guid OrderNumber { get; set; }
    }
}