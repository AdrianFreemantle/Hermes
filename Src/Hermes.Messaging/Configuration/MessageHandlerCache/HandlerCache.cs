using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Configuration.MessageHandlerCache
{
    internal static class HandlerCache
    {
        private static readonly List<HandlerCacheItem> handlerDetails = new List<HandlerCacheItem>();

        public static void SaveHandlerDetails(Type handlerType, Type messageContract, Action<object, object> handlerAction)
        {
            if(handlerAction == null)
                return;

            HandlerCacheItem details = handlerDetails.FirstOrDefault(detail => detail.HandlerType == handlerType);

            if (details == null)
            {
                details = new HandlerCacheItem(handlerType);
                handlerDetails.Add(details);              
            }

            details.AddHandlerAction(messageContract, handlerAction);
        }

        public static HandlerCacheItem[] GetHandlerDetails(ICollection<Type> messageTypes)
        {
            var result = handlerDetails.Where(detail => messageTypes.Any(detail.ContainsHandlerFor)).Distinct().ToArray();

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

        public static bool Contains(Type handlerType, Type messageContract)
        {
            return handlerDetails.Any(detail => detail.ContainsHandlerFor(handlerType));
        }

        public static IEnumerable<Type> GetAllHandlerTypes()
        {
            return handlerDetails.Select(detail => detail.HandlerType);
        }

        public static IEnumerable<Type> GetAllHandledMessageContracts()
        {
            return handlerDetails.SelectMany(handler => handler.GetHandledMessageContracts());
        }
    }
}