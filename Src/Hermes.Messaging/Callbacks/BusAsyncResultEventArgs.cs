using System;

namespace Hermes.Messaging.Callbacks
{
    /// <summary>
    /// Argument passed in the Registered event of the Callback object.
    /// </summary>
    public class BusAsyncResultEventArgs : EventArgs
    {
        /// <summary>
        /// Gets/sets the IAsyncResult.
        /// </summary>
        public BusAsyncResult Result { get; set; }

        /// <summary>
        /// Gets/sets the message id.
        /// </summary>
        public Guid MessageId { get; set; }
    }
}