using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Messaging.Configuration.MessageHandlers
{
    internal class CachedMessageHandler : IEquatable<CachedMessageHandler>
    {
        private readonly List<CachedMessageHandlerAction> actionDetails;

        public Type HandlerType { get; private set; } 
    
        public CachedMessageHandler(Type handlerType)
        {
            HandlerType = handlerType;
            actionDetails = new List<CachedMessageHandlerAction>();
        }

        public void AddHandlerAction (Type messageContract, Action<object, object> handlerAction)
        {
            if (actionDetails.Any(action => action.MessageContract == messageContract))
            {
                return;
            }

            actionDetails.Add(new CachedMessageHandlerAction(messageContract, handlerAction));
        }

        public bool ContainsHandlerFor(Type messageContract)
        {
            return actionDetails.Any(action => action.MessageContract == messageContract);
        }

        private Action<object, object> GetHandlerFor(Type messageContract)
        {
            var singleOrDefault = actionDetails.SingleOrDefault(action => action.MessageContract == messageContract);
            
            if (singleOrDefault != null)
            {
                return singleOrDefault.Action;
            }

            return null;
        }

        public IEnumerable<Type> GetHandledMessageContracts()
        {
            return actionDetails.Select(action => action.MessageContract).Distinct(new TypeComparer());
        }

        public bool Equals(CachedMessageHandler other)
        {
            if (other == null)
                return false;

            return other.HandlerType == HandlerType;
        }

        public bool TryHandleMessage(object handler, object message, IEnumerable<Type> contracts)
        {
            bool foundHandler = false;

            foreach (var contract in contracts)
            {
                var action = GetHandlerFor(contract);

                if (action != null)
                {
                    action.Invoke(handler, message);
                    foundHandler = true;
                }
            }

            return foundHandler;
        }
    }
}