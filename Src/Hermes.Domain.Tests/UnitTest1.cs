using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Hermes.Domain.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var aggregateId = Guid.NewGuid();
            var aggregate = new TestAggregate(new TestAggregateId(aggregateId));
            aggregate.TriggerEntityEvent();
            aggregate.TriggerAggregateEvent();

            var changes = ((IAggregate)aggregate).GetUncommittedEvents().ToArray();

            var entityEvent = (EntityEvent)changes[0];
            var aggregateEvent = (AggregateEvent)changes[1];

            var reconstructed = new TestAggregate(new TestAggregateId(aggregateId));

            ((IAggregate)reconstructed).LoadFromHistory(changes);
        }

    }
}
