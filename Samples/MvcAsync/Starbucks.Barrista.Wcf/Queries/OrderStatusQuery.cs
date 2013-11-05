using System;

using Hermes;
using Hermes.Messaging;

namespace Starbucks.Barrista.Wcf.Queries
{
    public class OrderStatusQuery : IReturn<OrderStatusQueryResult>
    {
        public Guid OrderNumber { get; set; }
    }
}