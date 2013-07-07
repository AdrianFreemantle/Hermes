using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Hermes.Serialization;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageBus : IMessageBus, IStartableMessageBus, IDisposable
    {
        private readonly ISerializeMessages messageSerializer;
        private readonly ITransportMessages messageTransport;
        private readonly IRouteMessageToEndpoint messageRouter;

        public MessageBus(ISerializeMessages messageSerializer, ITransportMessages messageTransport, IRouteMessageToEndpoint messageRouter)
        {
            this.messageSerializer = messageSerializer;
            this.messageTransport = messageTransport;
            this.messageRouter = messageRouter;
        }

        public void Start(Address localAddress)
        {
            messageTransport.Start(localAddress);
        }

        public void Stop()
        {
            messageTransport.Stop();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Send(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return;
            }

            Send(messageRouter.GetDestinationFor(messages.First().GetType()), messages);
        }

        public void Send(Address address, params object[] messages)
        {
            byte[] messageBody;

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                stream.Flush();
                messageBody = stream.ToArray();
            }

            var message = new MessageEnvelope(Guid.NewGuid(), Guid.Empty, Address.Self, TimeSpan.MaxValue, true, new Dictionary<string, string>(), messageBody);
            messageTransport.Send(message, address);
        }        
    }
}
