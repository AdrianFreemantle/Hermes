using System;

namespace Hermes.Messaging.Configuration
{
    internal struct MessageHandlerDetail
    {
        public Type HandlerType { get; private set; }
        public Action<object, object> HandlerAction { get; private set; }

        public MessageHandlerDetail(Type handlerType, Action<object, object> handlerAction)
            : this()
        {
            HandlerType = handlerType;
            HandlerAction = handlerAction;
        }
    }
}