using System;
using System.Collections.Generic;
using CoderBurger.Messages;
using Hermes;
using Hermes.Logging;
using Hermes.Messages;
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

    public static class OrderRepository
    {
        public static Dictionary<Guid, Order> Store { get; set; }

        static OrderRepository()
        {
            Store = new Dictionary<Guid, Order>();
        }
    }

    public class OrderWorkflow 
        : IHandleMessage<AbandonOrder>
        , IHandleMessage<PayOrder>
        , IHandleMessage<CancelOrder>
        , IHandleMessage<CollectOrder>
        , IHandleMessage<PlaceOrder>
        , IHandleMessage<RefundCustomer>
        , IHandleMessage<FriesPrepared>
        , IHandleMessage<BurgerPrepared>
        , IHandleMessage<DrinkPrepared>
    {
        enum OrderTrigger
        {
            OrderPlaced,
            OrderPaid,
            OrderCanceled,
            FoodItemReady,
            OrderCollected,
            OrderAbandoned
        }

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(OrderWorkflow));
        private readonly StateMachine<OrderState, OrderTrigger> orderState;
        private readonly IMessageBus messageBus;

        private Order order = new Order();

        public OrderWorkflow(IMessageBus messageBus)
        {
            this.messageBus = messageBus;

            orderState = new StateMachine<OrderState, OrderTrigger>(() => order.CurrentStatus, s => order.CurrentStatus = s);

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

            orderState.OnUnhandledTrigger((state, trigger) => Logger.Warn("Invalid state transition {0} : {1}", trigger, state));
        }

        private void GetOrder(Guid orderId)
        {
            if (!OrderRepository.Store.ContainsKey(orderId))
            {
                OrderRepository.Store[orderId] = new Order{ OrderId = orderId };
            }

            order = OrderRepository.Store[orderId];
        }

        public void Handle(PayOrder command)
        {
            Logger.Info("Accepting payment for order");
            GetOrder(command.OrderId);
           
            orderState.Fire(OrderTrigger.OrderPaid);
        }

        public void Handle(AbandonOrder command)
        {
            Logger.Info("Abandoning order");
            GetOrder(command.OrderId);

            orderState.Fire(OrderTrigger.OrderAbandoned);
        }

        public void Handle(CancelOrder command)
        {
            Logger.Info("Cancelling order");
            GetOrder(command.OrderId);
           
            orderState.Fire(OrderTrigger.OrderCanceled);
        }

        public void Handle(CollectOrder command)
        {
            Logger.Info("Order is being collected");
            GetOrder(command.OrderId);
           
            orderState.Fire(OrderTrigger.OrderCollected);
        }

        public void Handle(PlaceOrder command)
        {
            Logger.Info("Accpeting new order");
            GetOrder(command.OrderId);
         
            orderState.Fire(OrderTrigger.OrderPlaced);
        }

        public void Handle(RefundCustomer command)
        {
            Logger.Info("Refunding customer for order");
            messageBus.Publish(new CustomerRefunded { OrderId = command.OrderId });
        }

        public void Handle(FriesPrepared command)
        {
            Logger.Info("Received fries");
            GetOrder(command.OrderId);
           
            order.FriesReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        public void Handle(BurgerPrepared command)
        {
            Logger.Info("Received burger ");
            GetOrder(command.OrderId);

            order.BurgerReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        public void Handle(DrinkPrepared command)
        {
            Logger.Info("Received drink");
            GetOrder(command.OrderId);

            order.DrinkReady = true;
            orderState.Fire(OrderTrigger.FoodItemReady);
        }

        private void SubmitOrderToKitchen()
        {
            Logger.Info("Submitting order to kitchen");
            messageBus.Publish(new OrderPlaced { OrderId = order.OrderId });
        }

        private void OrderReady()
        {
            Logger.Info("Starting order abandonment timer for order");
            messageBus.Defer(TimeSpan.FromSeconds(15), new AbandonOrder { OrderId = order.OrderId });
            messageBus.Publish(new OrderReady { OrderId = order.OrderId });
        }

        private void StartPaymentTimer()
        {
            Logger.Info("Starting payment timer for order");
            messageBus.Defer(TimeSpan.FromSeconds(15), new CancelOrder { OrderId = order.OrderId });
        }

        private void RefundCustomer()
        {
            Logger.Info("Refunding Customer");
            messageBus.Send(new RefundCustomer { OrderId = order.OrderId });
        }

        private bool IsReadyForCollection()
        {
            return order.DrinkReady && order.FriesReady && order.BurgerReady;
        }

        private void Collected()
        {
            messageBus.Publish(new OrderCollected { OrderId = order.OrderId });            
        }
    }
}
