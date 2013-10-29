namespace Hermes.Domain.Tests
{
    public class TestAggregate : Aggregate
    {
        private readonly TestEntity testEntity;
 
        public TestAggregate(TestAggregateId identity) 
            : base(identity)
        {
            testEntity = new TestEntity(this, new TestEntityId(4));
        }

        public void TriggerEntityEvent()
        {
            testEntity.TriggerEntityEvent();
        }

        public void TriggerAggregateEvent()
        {
            RaiseEvent(new AggregateEvent(GetType().FullName));
        }

        protected void When(AggregateEvent e)
        {
            
        }
    }
}