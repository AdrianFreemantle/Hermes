namespace Hermes.Domain.Tests
{
    public class TestEntity : Entity
    {
        public TestEntity(TestAggregate parent, TestEntityId identity)
            : base(parent, identity)
        {
        }

        public void TriggerEntityEvent()
        {
            RaiseEvent(new EntityEvent(GetType().FullName));
        }

        protected void When(EntityEvent e)
        {

        }
    }
}