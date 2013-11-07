using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Hermes.Reflection;

namespace Starbucks.Messages
{
    public class PlaceOrder : ICommand
    {
        public Guid OrderNumber { get; set; }
        public Coffee Coffee { get; set; }
    }

    [DataContract]
    public class OrderReady
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
