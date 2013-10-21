using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hermes.Messaging;
using Starbucks.Messages;

namespace Starbucks.Controllers
{
    public class BaristaController : Controller
    {
        private readonly IMessageBus messageBus;

        public BaristaController(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        [HandleError(ExceptionType = typeof(TimeoutException), View = "Error")]
        public async Task<ActionResult> BuyCoffee()
        {
            var myOrder = new OrderCoffee
            {
                Coffee = Coffee.Espresso,
                OrderNumber = Guid.NewGuid()
            };

            Task<OrderReady> myOrderCallback = messageBus.Send(Guid.NewGuid(), myOrder).Register(this.GetResult<OrderReady>, TimeSpan.FromSeconds(4));
            await myOrderCallback;

            return View("BuyCoffee", myOrderCallback.Result);
        }
    }

    public static class CompletionResultExtension
    {
        public static T GetResult<T>(this Controller controller, CompletionResult e)
        {
            if ((ErrorCodes)e.ErrorCode != ErrorCodes.Success)
            {
                throw new RequestFailedException((ErrorCodes)e.ErrorCode);
            }

            return (T)e.Messages.FirstOrDefault();
        }
    }
}
