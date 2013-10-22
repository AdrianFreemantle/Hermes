using System.ServiceModel;
using System.Threading.Tasks;

namespace Hermes.Messaging.Wcf
{
    [ServiceContract(Namespace = "Hermes.Messaging.Wcf", Name = "QueryService")]
    public interface IQueryService<in TQuery, TResult> where TQuery : IReturn<TResult>
    {
        [OperationContract]
        Task<TResult> Query(TQuery command);
    }
}