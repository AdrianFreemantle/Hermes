using System;
using CoderBurger.Messages;
using CoderBurger.Messages.Waiter;
using Hermes;
using Hermes.Logging;
using Stateless;

namespace CoderBurger.Waiter
{
    public enum OrderState
    {
        NoOrder,
        Unpaid,
        Canceled,
        Preparing,
        Ready,
        Collected,
        Abandoned
    }

    enum OrderTrigger
    {
        OrderPlaced,
        OrderPaid,
        OrderCanceled,
        FoodItemReady,
        OrderCollected,
        OrderAbandoned
    }   

    public class OrderWorkflow
    {  
        public Guid OrderId { get; set; }
        public OrderState CurrentStatus { get; private set; }

        private Order order = new Order();

        private readonly StateMachine<OrderState, OrderTrigger> orderState;
        private readonly IMessageBus bus;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OrderWorkflow));

        public OrderWorkflow(IMessageBus bus, Guid id)
        {
            this.OrderId = id;
            this.bus = bus;
            CurrentStatus = OrderState.NoOrder;
            orderState = new StateMachine<OrderState, OrderTrigger>(GetCurrentState, s => CurrentStatus = s);

            orderState.Configure(OrderState.NoOrder)
                      .Permit(OrderTrigger.OrderPlaced, OrderState.Unpaid);

            orderState.Configure(OrderState.Unpaid)
                      .OnEntry(StartPaymentTimer)
                      .Permit(OrderTrigger.OrderPaid, OrderState.Preparing)
                      .Permit(OrderTrigger.OrderCanceled, OrderState.Canceled);

            orderState.Configure(OrderState.Preparing)
                      .OnEntry(SubmitOrderToKitchen)
                      .Ignore(OrderTrigger.OrderCanceled)
                      .IgnoreIf(OrderTrigger.FoodItemReady, () => !IsReadyForCollection())
                      .PermitIf(OrderTrigger.FoodItemReady, OrderState.Ready, IsReadyForCollection);

            orderState.Configure(OrderState.Ready)
                      .OnEntry(OrderReady)
                      .Ignore(OrderTrigger.OrderCanceled)
                      .Permit(OrderTrigger.OrderCollected, OrderState.Collected)
                      .Permit(OrderTrigger.OrderAbandoned, OrderState.Abandoned);

            orderState.Configure(OrderState.Collected)
                      .OnEntry(Collected)
                      .Ignore(OrderTrigger.OrderCanceled)
                      .Ignore(OrderTrigger.OrderAbandoned);

            orderState.Configure(OrderState.Abandoned)
                      .Ignore(OrderTrigger.OrderCanceled)
                      .OnEntry(RefundCustomer);

            orderState.OnUnhandledTrigger((state, trigger) => Console.WriteLine("Invalid state transition {0} : {1}", trigger, state));
        }

        public OrderState GetCurrentState()
        {
            return CurrentStatus;
        }

        public void PlaceOrder()
        {
            orderState.Fire(OrderTrigger.OrderPlaced);
        }

        public void Pay()
        {
            orderState.Fire(OrderTrigger.OrderPaid);
        }

        public void CancelOrder()
        {
            orderState.Fire(OrderTrigger.OrderCanceled);
        }

        public void FriesPrepared()
        {
            order.FriesReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        public void DrinkPrepared()
        {
            order.DrinkReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        public void BurgerPrepared()
        {
            order.BurgerReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        public void Collect()
        {
            orderState.Fire(OrderTrigger.OrderCollected);
        }

        public void Abandon()
        {
            orderState.Fire(OrderTrigger.OrderAbandoned);
        }

        void SubmitOrderToKitchen()
        {
            Logger.Info("Submitting order {0} to kitchen", OrderId);
            bus.Publish(new OrderPlaced { OrderId = OrderId });
        }

        void OrderReady()
        {
            Logger.Info("Starting order abandonment timer for order {0}", OrderId);
            bus.Defer(TimeSpan.FromSeconds(15), new AbandonOrder { OrderId = OrderId });
            bus.Publish(new OrderReady { OrderId = OrderId });
        }

        void StartPaymentTimer()
        {
            Logger.Info("Starting payment timer for order {0}", OrderId);
            bus.Defer(TimeSpan.FromSeconds(15), new CancelOrder { OrderId = OrderId });
        }

        void RefundCustomer()
        {
            Logger.Info("Refunding Customer {0}", OrderId);
            bus.Send(new RefundCustomer { OrderId = OrderId });
        }

        bool IsReadyForCollection()
        {
            return order.DrinkReady && order.FriesReady && order.BurgerReady;
        }

        void Collected()
        {
            bus.Publish(new OrderCollected { OrderId = OrderId });            
        }
    }
}
