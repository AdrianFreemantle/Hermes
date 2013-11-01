using System.ServiceModel;
using System.Threading.Tasks;

using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
{
    [ServiceContract(Namespace = "Hermes.Messaging.Wcf", Name = "BaristaService")]
    public interface IBaristaService
    {
        [OperationContract]
        Task<int> Put(PlaceOrder command);
    }
}