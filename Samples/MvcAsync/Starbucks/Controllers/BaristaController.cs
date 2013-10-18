using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Starbucks.Messages;
using Hermes.Messaging;
using Hermes;

namespace Starbucks.Controllers
{
    public class BaristaController : Controller
    {
        [HandleError(ExceptionType = typeof(TimeoutException), View = "Error")]
        public async Task<ActionResult> BuyCoffeeAsync()
        {
            var myOrder = new OrderCoffee
            {
                Coffee = Coffee.Espresso,
                OrderNumber = Guid.NewGuid()
            };

            using (var myOrderCallback = MvcApplication.Bus.Send(Guid.NewGuid(), myOrder).Register<OrderReady>(GetResult<OrderReady>))
            {
                if (await Task.WhenAny(myOrderCallback, Task.Delay(TimeSpan.FromSeconds(2))) == myOrderCallback)
                {
                    return View("BuyCoffee", myOrderCallback.Result);
                }
            }

            throw new TimeoutException("blah");
        }

        private static T GetResult<T>(CompletionResult e)
        {
            if ((ErrorCodes)e.ErrorCode != ErrorCodes.Success)
            {
                throw new Exception(String.Format("Message failed with error: {0}", ((ErrorCodes)e.ErrorCode).GetDescription()));
            }

            return (T)e.Messages.FirstOrDefault();
        }
    }
}
