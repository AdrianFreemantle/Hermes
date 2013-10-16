using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    /// <summary>
    /// Indicates the ability to transport inbound and outbound messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface must be designed to be multi-thread safe.
    /// </remarks>
    public interface ITransportMessages : IDisposable
    {        
        TransportMessage CurrentTransportMessage { get; }

        /// <summary>
        /// Starts the transport listening for new messages to receive.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the transport listening for new messages.
        /// </summary>
        void Stop();

        ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages);
        ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers);
        void SendControlMessage(Address recipient, Guid correlationId, params HeaderValue[] headerValues);

        void OnMessageReceived(TransportMessage transportMessage);
    }
}
