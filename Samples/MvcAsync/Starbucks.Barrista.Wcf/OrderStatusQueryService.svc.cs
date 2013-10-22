using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Messaging.Wcf;

using Starbucks.Barrista.Wcf.Queries;
using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
{
    [ServiceBehavior(Name = "OrderQueryService")]
    public class OrderStatusQueryService : QueryService<OrderStatusQuery, OrderStatusQueryResult>
    {
        public override System.Threading.Tasks.Task<OrderStatusQueryResult> Query(OrderStatusQuery query)
        {
            return base.Query(query);
        }
    }
}
