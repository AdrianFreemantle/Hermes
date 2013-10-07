using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Starbucks.Messages;

namespace Starbucks.Controllers
{
    public class BaristaController : Controller
    {
        [HandleError(ExceptionType = typeof(TimeoutException), View = "Error")]
        public async Task<ActionResult> BuyCoffeeAsync()
        {
            Task<ErrorCodes> b = MvcApplication.Bus.Send(new Guid(), new BuyCoffee()).Register<ErrorCodes>();
            ErrorCodes result = await b;
            return View("BuyCoffee", result);
        }
    }
}
