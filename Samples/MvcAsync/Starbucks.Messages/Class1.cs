using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Hermes;

namespace Starbucks.Messages
{
    public class PlaceOrder : ICommand
    {
        public Guid OrderNumber { get; set; }
        public Coffee Coffee { get; set; }
        public Sandwich Sandwich { get; set; }
    }

    public interface IOrderPlaced : IEvent
    {
        Guid OrderNumber { get; }
        Coffee Coffee { get; }
        Sandwich Sandwich { get; }
    }

    public interface IOrderReady : IEvent
    {
        Guid OrderNumber { get; }
    }

    public interface IDrinkPrepared : IEvent
    {
        Guid OrderNumber { get; }
        string Drink { get; }
    }

    public interface ISandwichPrepared : IEvent
    {
        Guid OrderNumber { get; }
    }

    [DataContract]
    public class OrderReady : IOrderReady, IDrinkPrepared
    {
        [DataMember]
        public Guid OrderNumber { get; protected set; }
        [DataMember]
        public string Drink { get; protected set; }

        protected OrderReady()
        {
        }

        public OrderReady(Guid orderNumber, Coffee coffee)
        {
            OrderNumber = orderNumber;
            Drink = coffee.GetDescription();
        }
    }

    public enum Coffee
    {
        None,
        FilterCoffee,
        Espresso,
        DoubleEspresso
    }

    public enum Sandwich
    {
        None,
        HamAndCheese,
        BaconLettuceAndTomato
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
