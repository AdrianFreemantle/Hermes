using System;

namespace CoderBurger.Waiter
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public bool FriesReady { get; set; }
        public bool DrinkReady { get; set; }
        public bool BurgerReady { get; set; }
        public OrderState CurrentStatus { get; set; }
    }
}