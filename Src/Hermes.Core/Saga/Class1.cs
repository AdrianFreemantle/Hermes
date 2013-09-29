using System;

using Hermes.Messaging;
using Hermes.Saga;

namespace Hermes.Core.Saga
{
    public interface ISaga<T> where T : class, IContainSagaData
    {
        T State { get; set; }
    }

    public abstract class Saga<T> : ISaga<T> where T : class, IContainSagaData, new()
    {
        private readonly IPersistSagas sagaPersistence;
        protected readonly IMessageBus Bus;

        protected Saga(IPersistSagas sagaPersistence, IMessageBus bus)
        {
            this.sagaPersistence = sagaPersistence;
            Bus = bus;
        }

        public T State { get; set; }

        protected void BeginSaga(Guid id)
        {
            var state = sagaPersistence.Get<T>(id);

            if (state == null)
            {
                State = new T
                {
                    Id = Bus.CurrentMessageContext.CorrelationId,
                    OriginalMessageId = Bus.CurrentMessageContext.MessageId,
                    Status = 0,
                    Originator = Bus.CurrentMessageContext.ReplyToAddress.ToString()
                };

                sagaPersistence.Create(State);
            }
            else
            {
                State = state;
            }
        }

        protected void CompleteSaga()
        {
            
        }
    }
}
