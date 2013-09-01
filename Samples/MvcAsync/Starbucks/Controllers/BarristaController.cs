using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Hermes;

using Starbucks.Messages;

namespace Starbucks.Controllers
{
    public class BarristaController : Controller
    {
        public async Task<ActionResult> BuyCoffeeAsync()
        {
            Task<ErrorCodes> b = MvcApplication.Bus.Send(new Guid(), new BuyCoffee()).Register<ErrorCodes>();
            return View("BuyCoffee", await b);
        }
    }
}
