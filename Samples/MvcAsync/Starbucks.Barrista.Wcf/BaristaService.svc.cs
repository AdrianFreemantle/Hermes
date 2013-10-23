using System.Threading.Tasks;

using Hermes.Messaging.Wcf;

using Starbucks.Messages;

namespace Starbucks.Barrista.Wcf
{
    public class BaristaService : CommandService, IBaristaService
    {
        public async Task<int> Put(OrderCoffee command)
        {
            return await base.Put(command);
        }
    }
}
