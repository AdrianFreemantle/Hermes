using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Transactions;

using Hermes.Serialization;

namespace Hermes.Core
{
    public class MessageProcessor : IProcessMessages
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly IObjectBuilder objectBuilder;

        public MessageProcessor(ISerializeMessages messageSerializer, IObjectBuilder objectBuilder)
        {
            this.messageSerializer = messageSerializer;
            this.objectBuilder = objectBuilder;
        }

        public void Process(MessageEnvelope envelope)
        {
            var messages = ExtractMessages(envelope);

            try
            {
                using(var childBuilder = objectBuilder.BeginLifetimeScope())
                using(var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Suppress))
                {
                    var messageDispatcher = childBuilder.GetInstance<IDispatchMessagesToHandlers>();
                    
                    foreach (var message in messages)
                    {
                        messageDispatcher.DispatchToHandlers(message);
                    }
                    //commit all units of work
                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                //logg exception 
                //rollback all units of work
                throw new MessageProcessingFailedException(envelope, ex);
            }
        }

        private IEnumerable<object> ExtractMessages(MessageEnvelope envelope)
        {
            if (envelope.Body == null || envelope.Body.Length == 0)
            {
                return new object[0];
            }

            try
            {
                using (var stream = new MemoryStream(envelope.Body))
                {
                    return messageSerializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                throw new SerializationException("Could not deserialize message.", e);
            }
        }
    }
}