using System;

namespace Hermes.Messaging.Configuration.MessageHandlers
{
    internal class CachedMessageHandlerAction
    {
        public Action<object, object> Action { get; private set; }
        public Type MessageContract { get; private set; }

        public CachedMessageHandlerAction(Type messageContract, Action<object, object> handlerAction)
        {
            MessageContract = messageContract;
            Action = handlerAction;
        }
    }
}