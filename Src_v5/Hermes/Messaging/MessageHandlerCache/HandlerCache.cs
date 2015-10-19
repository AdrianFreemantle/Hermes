using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Equality;
using Hermes.Ioc;

namespace Hermes.Messaging.MessageHandlerCache
{
    public static class HandlerCache
    {
        private static readonly List<HandlerCacheItem> HandlerDetails = new List<HandlerCacheItem>();

        public static void InitializeCache(IEnumerable<Type> messageTypes, ICollection<Type> messageHandlerTypes)
        {
            foreach (var messageType in messageTypes)
            {
                CacheHandlersForMessageContract(messageType, messageHandlerTypes);
            }
        }

        private static void CacheHandlersForMessageContract(Type messageContract, IEnumerable<Type> messageHandlerTypes)
        {
            foreach (Type handlerType in messageHandlerTypes)
            {
                if (HandlerIsCached(handlerType, messageContract))
                    continue;

                var handlerAction = HandlerFactory.BuildHandlerAction(handlerType, messageContract);
                SaveHandlerAction(handlerType, messageContract, handlerAction);
            }
        }

        private static void SaveHandlerAction(Type handlerType, Type messageContract, Action<object, object> handlerAction)
        {
            if (handlerAction == null)
                return;

            if (MessageContractFactory.IsCommandType != null && MessageContractFactory.IsCommandType(messageContract))
            {
                if (HandlerDetails.Any(d => d.ContainsHandlerFor(messageContract)))
                {
                    throw new HermesComponentRegistrationException(String.Format("A command may only be handled by one class. A duplicate command handler for command {0} was found on class {1}.", messageContract.Name, handlerType.FullName));
                }
            }

            HandlerCacheItem details = HandlerDetails.FirstOrDefault(detail => detail.HandlerType == handlerType);

            if (details == null)
            {
                details = new HandlerCacheItem(handlerType);
                HandlerDetails.Add(details);
            }

            details.AddHandlerAction(messageContract, handlerAction);
        }

        public static HandlerCacheItem[] GetHandlers(ICollection<Type> messageTypes)
        {
            var result = HandlerDetails.Where(detail => messageTypes.Any(detail.ContainsHandlerFor)).Distinct().ToArray();

            if (!result.Any())
            {
                throw new InvalidOperationException(String.Format("No handlers could be found for message contract {0}", GetContractNames(messageTypes)));
            }

            return result;
        }

        private static string GetContractNames(IEnumerable<Type> messageTypes)
        {
            return String.Join(", ", messageTypes.Select(type => type.FullName));
        }

        public static bool HandlerIsCached(Type handlerType, Type messageContract)
        {
            return HandlerDetails.Any(detail => detail.HandlerType == handlerType && detail.ContainsHandlerFor(handlerType));
        }

        public static IEnumerable<Type> GetAllHandlerTypes()
        {
            return HandlerDetails.Select(detail => detail.HandlerType);
        }

        public static IEnumerable<Type> GetAllHandledMessageContracts()
        {
            return HandlerDetails
                .SelectMany(handler => handler.GetHandledMessageContracts())
                .Distinct(new TypeEqualityComparer());
        }
    }
}
