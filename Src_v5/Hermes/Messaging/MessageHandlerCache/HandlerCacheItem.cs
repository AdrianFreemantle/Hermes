using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Equality;
using Hermes.Logging;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.MessageHandlerCache
{
    public class HandlerCacheItem : IEquatable<HandlerCacheItem>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(HandlerCacheItem));

        private readonly List<ActionCacheItem> actionDetails;

        public Type HandlerType { get; private set; }

        public HandlerCacheItem(Type handlerType)
        {
            HandlerType = handlerType;
            actionDetails = new List<ActionCacheItem>();
        }

        public void AddHandlerAction(Type messageContract, Action<object, object> handlerAction)
        {
            if (actionDetails.Any(action => action.MessageContract == messageContract))
            {
                return;
            }

            actionDetails.Add(new ActionCacheItem(messageContract, handlerAction));
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
            return actionDetails.Select(action => action.MessageContract).Distinct(new TypeEqualityComparer());
        }

        public bool Equals(HandlerCacheItem other)
        {
            if (other == null)
                return false;

            return other.HandlerType == HandlerType;
        }

        public object TryHandleMessage(object message, IEnumerable<Type> contracts)
        {
            object messageHandler = ServiceLocator.Current.GetInstance(HandlerType);

            foreach (var contract in contracts)
            {
                var action = GetHandlerFor(contract);

                if (action != null)
                {
                    Logger.Debug("Invoking handler {0} for message {1}", HandlerType.FullName, message.ToString());
                    action.Invoke(messageHandler, message);
                }
            }

            return messageHandler;
        }
    }
}