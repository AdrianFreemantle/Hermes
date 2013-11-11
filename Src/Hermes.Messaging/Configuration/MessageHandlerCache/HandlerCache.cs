﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<HandlerCacheItem> GetHandlerDetails(ICollection<Type> messageTypes)
        {
            return handlerDetails.Where(detail => messageTypes.Any(detail.ContainsHandlerFor)).Distinct().ToList();
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