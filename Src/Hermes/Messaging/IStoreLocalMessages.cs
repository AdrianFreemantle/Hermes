using System;
using System.Collections.Generic;

namespace Hermes.Messaging
{
    public interface IStoreLocalMessages
    {
        void SaveSession(object message, Guid messageId, Dictionary<string, string> headers);
    }
}