using System.ServiceModel;
using System.Threading.Tasks;

namespace Hermes.Messaging.Wcf
{
    [ServiceContract(Namespace = "Hermes.Messaging.Wcf", Name = "CommandService")]
    public interface ICommandService<in TCommand>
    {
        [OperationContract]
        Task<int> Execute(TCommand command);
    }
}