using Hermes;
using Hermes.Reflection;

using Starbucks.Messages;

namespace Starbucks.Models
{
    public class OrderReadyViewModel
    {
        public string Drink { get; private set; }

        public OrderReadyViewModel(PlaceOrder order)
        {
            Drink = order.Coffee.GetDescription();
        }
    }
}