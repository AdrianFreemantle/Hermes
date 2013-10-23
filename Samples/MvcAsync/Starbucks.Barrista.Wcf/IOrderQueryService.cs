using System.ServiceModel;
using System.Threading.Tasks;

using Starbucks.Barrista.Wcf.Queries;

namespace Starbucks.Barrista.Wcf
{
    [ServiceContract(Namespace = "Hermes.Messaging.Wcf", Name = "OrderQueryService")]
    public interface IOrderQueryService
    {
        [OperationContract]
        Task<OrderStatusQueryResult> Get(OrderStatusQuery query);
    }
}