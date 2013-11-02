﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Hermes.Ioc;

namespace Hermes.Messaging.Configuration.MessageHandlers
{
    internal static class MessageHandlerScanner
    {
        public static void Scan(IContainerBuilder containerBuilder)
        {
            using (var scanner = new AssemblyScanner())
            {
                IEnumerable<Type> messageTypes = GetMesageTypes(scanner);
                ICollection<Type> messageHandlerTypes = GetMessageHandlerTypes(scanner);

                foreach (var messageHandlerType in messageHandlerTypes)
                {
                    containerBuilder.RegisterType(messageHandlerType, DependencyLifecycle.InstancePerUnitOfWork);
                }
                
                foreach (var messageType in messageTypes)
                {
                    blah(messageType, messageHandlerTypes);
                }
            }
        }

        private static IEnumerable<Type> GetMesageTypes(AssemblyScanner scanner)
        {
            return scanner.Types
                          .Where(Settings.IsCommandType)
                          .Union(scanner.Types.Where(Settings.IsEventType))
                          .Union(scanner.Types.Where(Settings.IsMessageType))
                          .Distinct();
        }

        private static void blah(Type messageContract, IEnumerable<Type> messageHandlerTypes)
        {
            foreach (Type handlerType in messageHandlerTypes)
            {
                if (MessageHandlerCache.Contains(handlerType, messageContract))
                    continue;

                Action<object, object> handlerAction = GetHandlerAction(handlerType, messageContract);

                MessageHandlerCache.SaveHandlerDetails(handlerType, messageContract, handlerAction);
            }
        }

        private static ICollection<Type> GetMessageHandlerTypes(AssemblyScanner scanner)
        {
            return
                scanner.Types.Where(
                    t => t.GetInterfaces()
                          .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleMessage<>)))
                       .Distinct(new TypeComparer()).ToArray();
        }

        private static Action<object, object> GetHandlerAction(Type typeThatImplementsHandler, Type messageType)
        {
            Type interfaceGenericType = typeof (IHandleMessage<>);
            var interfaceType = interfaceGenericType.MakeGenericType(messageType);

            if (interfaceType.IsAssignableFrom(typeThatImplementsHandler))
            {
                var methodInfo = typeThatImplementsHandler.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();                                

                if (methodInfo != null)
                {
                    ParameterInfo firstParameter = methodInfo.GetParameters().First();

                    if (firstParameter.ParameterType != messageType)
                        return null;

                    var handler = Expression.Parameter(typeof (object));
                    var message = Expression.Parameter(typeof (object));

                    var castTarget = Expression.Convert(handler, typeThatImplementsHandler);
                    var castParam = Expression.Convert(message, methodInfo.GetParameters().First().ParameterType);
                    var execute = Expression.Call(castTarget, methodInfo, castParam);
                    return Expression.Lambda<Action<object, object>>(execute, handler, message).Compile();
                }
            }

            return null;
        }
    }
}