using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Hermes;

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