using System.ServiceModel;
using System.Threading.Tasks;

using Hermes.Messaging.Wcf;

using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
{
    public class BaristaService : CommandService, ICommandService<OrderCoffee>
    {
        public async Task<int> Execute(OrderCoffee command)
        {
            return await base.Execute(command);
        }
    }
}
