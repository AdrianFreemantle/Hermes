using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hermes.Messaging;
using Starbucks.Messages;

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
            var myOrder = new OrderCoffee
            {
                Coffee = Coffee.Espresso,
                OrderNumber = Guid.NewGuid()
            };

            Task<OrderReady> myOrderCallback = messageBus.Send(Guid.NewGuid(), myOrder).Register(GetResult, TimeSpan.FromSeconds(4));
            await myOrderCallback;

            return View("BuyCoffee", myOrderCallback.Result);
        }

        private OrderReady GetResult(CompletionResult completionResult)
        {
            if (completionResult.ErrorCode != (int)ErrorCodes.Success)
            {
                throw new RequestFailedException((ErrorCodes)completionResult.ErrorCode);
            }

            return (OrderReady)completionResult.Messages.FirstOrDefault();
        }
    }
}
