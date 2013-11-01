using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hermes.Messaging.Configuration
{
    internal static class HandlerCache
    {
        private static readonly Dictionary<string, List<MessageHandlerDetail>> handlerDetails = new Dictionary<string, List<MessageHandlerDetail>>();

        public static void Scan(IEnumerable<Type> messageTypes, AssemblyScanner scanner)
        {
            foreach (var messageType in messageTypes.Distinct())
            {
                foreach (Type handlerType in scanner.Types.Distinct())
                {
                    if(HandlerAlreadyAdded(handlerType, messageType))
                        continue;
                    
                    Action<object, object> handlerAction = GetMethod(handlerType, messageType);

                    if (handlerAction != null)
                    {
                        if (!handlerDetails.ContainsKey(messageType.FullName))
                        {
                            handlerDetails[messageType.FullName] = new List<MessageHandlerDetail>();
                        }

                        handlerDetails[messageType.FullName].Add(new MessageHandlerDetail(handlerType, handlerAction));
                    }
                }
            }
        }

        private static bool HandlerAlreadyAdded(Type handlerType, Type messageType)
        {
            return handlerDetails.ContainsKey(messageType.FullName) 
                && handlerDetails[messageType.FullName].Any(detail => detail.HandlerType.FullName == handlerType.FullName);
        }

        private static Action<object, object> GetMethod(Type typeThatImplementsHandler, Type messageType)
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

                    var target = Expression.Parameter(typeof (object));
                    var param = Expression.Parameter(typeof (object));

                    var castTarget = Expression.Convert(target, typeThatImplementsHandler);
                    var castParam = Expression.Convert(param, methodInfo.GetParameters().First().ParameterType);
                    var execute = Expression.Call(castTarget, methodInfo, castParam);
                    return Expression.Lambda<Action<object, object>>(execute, target, param).Compile();
                }
            }

            return null;
        }

        public static IReadOnlyCollection<MessageHandlerDetail> GetHandlerDetails(object message)
        {
            Type messageType = message.GetType();

            if(handlerDetails.ContainsKey(messageType.FullName))
            {
                return handlerDetails[messageType.FullName];
            }

            return new MessageHandlerDetail[0];
        }
    }
}