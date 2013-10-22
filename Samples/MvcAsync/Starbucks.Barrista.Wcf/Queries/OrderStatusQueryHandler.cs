using Hermes;

namespace Starbucks.Barrista.Wcf.Queries
{
    public class OrderStatusQueryHandler : IAnswerQuery<OrderStatusQuery, OrderStatusQueryResult>
    {
        public OrderStatusQueryResult Answer(OrderStatusQuery query)
        {
            return new OrderStatusQueryResult {Status = "Completed"};
        }
    }
}