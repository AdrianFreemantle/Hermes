using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Hermes;

namespace Starbucks.Messages
{
    public class OrderCoffee : ICommand
    {
        public Guid OrderNumber { get; set; }
        public Coffee Coffee { get; set; }
    }

    public interface IOrderReady : IEvent
    {
        Guid OrderNumber { get; }
    }

    public interface IDrinkPrepared : IEvent
    {
        string Drink { get; }
    }

    [DataContract]
    public class CoffeeReady : IOrderReady, IDrinkPrepared
    {
        [DataMember]
        public Guid OrderNumber { get; protected set; }
        [DataMember]
        public string Drink { get; protected set; }

        protected CoffeeReady()
        {
        }

        public CoffeeReady(Guid orderNumber, Coffee coffee)
        {
            OrderNumber = orderNumber;
            Drink = coffee.GetDescription();
        }
    }

    public enum Coffee
    {
        Filter,
        Espresso
    }

    public enum ErrorCodes
    {
        Timeout = -1,
        Success = 0,
        [Description("Out of coffee")]
        OutOfCoffee = 1
    }

    public interface ICommand {}

    public interface IEvent {}
}
