using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Messaging.Wcf;

using Starbucks.Barrista.Wcf.Queries;

namespace Starbucks.Barrista.Wcf
{
    public class OrderQueryService : QueryService, IOrderQueryService
    {
        public async Task<OrderStatusQueryResult> Query(OrderStatusQuery query)
        {
            return await base.Query<OrderStatusQuery, OrderStatusQueryResult>(query);
        }
    }
}
