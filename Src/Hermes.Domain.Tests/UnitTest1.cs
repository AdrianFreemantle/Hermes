using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hermes.Domain.Tests
{
    [DebuggerStepThrough]
    public class TestAggregateId : Identity<Guid>
    {
        public TestAggregateId(Guid id)
            : base(id)
        {
            
        }
    }

    [DebuggerStepThrough]
    public class TestEntityId : Identity<int>
    {
        public TestEntityId(int id)
            : base(id)
        {

        }
    }

    public class TestEntity : Entity
    {
        public TestEntity(TestAggregate parent, TestEntityId identity) 
            : base(parent, identity)
        {
        }

        public void TriggerEvent()
        {
            RaiseEvent(new TestEvent());
        }

        protected void When(TestEvent e)
        {

        }
    }

    public class TestEvent : IDomainEvent
    {
        [AggregateId]
        public Guid AggregateId { get; protected set; }
        [EntityId]
        public int EntityId { get; protected set; }
        public int Version { get; protected set; }
    }


    public class TestAggregate : Aggregate
    {
        private readonly TestEntity testEntity;
 
        public TestAggregate(TestAggregateId identity) 
            : base(identity)
        {
            testEntity = new TestEntity(this, new TestEntityId(4));
        }

        public void TriggerEvent()
        {
            testEntity.TriggerEvent();
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = new TestAggregate(new TestAggregateId(aggregateId));
            aggregate.TriggerEvent();

            var changes = ((IAggregate)aggregate).GetUncommittedEvents();

            var domainEvent = changes.First() as TestEvent;

            Console.WriteLine(domainEvent.AggregateId);
        }

    }
}
