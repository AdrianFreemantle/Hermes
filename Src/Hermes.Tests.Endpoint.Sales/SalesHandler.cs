﻿using System;
using System.Threading;
using Hermes.Messages;
using Hermes.Tests.Messages;

namespace Hermes.Tests.Endpoint.Sales
{
    public class SalesHandler : IHandleMessage<SellShoes>, IHandleMessage<TestEvent>
    {
        private readonly IMessageBus bus;

        public SalesHandler(IMessageBus bus)
        {
            this.bus = bus;
        }

        public void Handle(SellShoes command)
        {
            Console.WriteLine("Publishing shoes sold event");

            Console.WriteLine("SellShoes message handled by {0} on thread {1}", GetHashCode(), Thread.CurrentThread.ManagedThreadId);
            bus.InMemory.Raise(new TestEvent { Message =  "Raise" });

            //bus.Defer(TimeSpan.FromSeconds(10), new ShoesSold
            //{
            //    OrderNumber = DateTime.Now.Ticks.ToString(),
            //    ShoeTypeId = command.ShoeTypeId,
            //    Size = command.Size
            //});
        }

        public void Handle(TestEvent command)
        {
            Console.WriteLine("TestEvent message handled by {0} on thread {1}", GetHashCode(), Thread.CurrentThread.ManagedThreadId);
        }
    }
}