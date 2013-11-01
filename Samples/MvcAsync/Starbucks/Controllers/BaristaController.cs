using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hermes.Messaging;
using Starbucks.BaristaServiceProxy;
using Starbucks.Messages;
using Starbucks.Models;
using Starbucks.OrderQueryServiceProxy;

namespace Starbucks.Controllers
{
    [HandleError(ExceptionType = typeof(TimeoutException), View = "Timeout")]
    [HandleError(ExceptionType = typeof(RequestFailedException), View = "Error")]
    public class BaristaController : Controller
    {
        private readonly IMessageBus messageBus;

        public BaristaController(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }
        
        public async Task<ActionResult> BuyCoffee()
        {
            var myOrder = new PlaceOrder
            {
                Coffee = Coffee.Espresso,
                OrderNumber = Guid.NewGuid()
            };

            ErrorCodes result = await messageBus.Send(myOrder).Register<ErrorCodes>(TimeSpan.FromSeconds(50000));

            if (result != ErrorCodes.Success)
            {
                throw new RequestFailedException(result);
            }

            return View("BuyCoffee", new OrderReadyViewModel(myOrder));
        }

        public async Task<ActionResult> BuyCoffeeWcf()
        {
            var myOrder = new PlaceOrder
            {
                Coffee = Coffee.Espresso,
                OrderNumber = Guid.NewGuid()
            };

            var proxy = new BaristaServiceClient();
            ErrorCodes result = (ErrorCodes)await proxy.PutAsync(myOrder);

            if (result != ErrorCodes.Success)
            {
                throw new RequestFailedException(result);
            }

            return View("BuyCoffee", new OrderReadyViewModel(myOrder));
        }

        public async Task<string> Query()
        {
            var proxy = new OrderQueryServiceClient();
            OrderStatusQueryResult result = await proxy.GetAsync(new OrderStatusQuery { OrderNumber = Guid.NewGuid() });
            return result.Status;
        }
       
    }
}
