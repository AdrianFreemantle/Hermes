using System;

namespace Hermes.Messaging.Configuration.MessageHandlerCache
{
    public class ActionCacheItem
    {
        public Action<object, object> Action { get; private set; }
        public Type MessageContract { get; private set; }

        public ActionCacheItem(Type messageContract, Action<object, object> handlerAction)
        {
            MessageContract = messageContract;
            Action = handlerAction;
        }
    }
}