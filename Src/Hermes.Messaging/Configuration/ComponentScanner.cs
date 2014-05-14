using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Hermes.Ioc;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Queries;
using Hermes.Reflection;

namespace Hermes.Messaging.Configuration
{
    internal static class ComponentScanner
    {
        public static void Scan(IContainerBuilder containerBuilder)
        {
            using (var scanner = new AssemblyScanner())
            {
                IEnumerable<Type> messageTypes = GetMesageTypes(scanner);
                ICollection<Type> messageHandlerTypes = GetMessageHandlerTypes(scanner);
                ICollection<Type> queryHandlerTypes = GetQueryHandlerTypes(scanner);


                foreach (var messageHandlerType in messageHandlerTypes)
                {
                    containerBuilder.RegisterType(messageHandlerType, DependencyLifecycle.InstancePerUnitOfWork);
                }

                foreach (var queryHandlerType in queryHandlerTypes)
                {
                    containerBuilder.RegisterType(queryHandlerType, DependencyLifecycle.InstancePerUnitOfWork);
                }
                
                foreach (var messageType in messageTypes)
                {
                    CacheHandlersForMessageContract(messageType, messageHandlerTypes);
                }

                foreach (var intitializer in scanner.Types.Where(t => typeof(INeedToInitializeSomething).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    containerBuilder.RegisterType(intitializer, DependencyLifecycle.SingleInstance);
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

        private static void CacheHandlersForMessageContract(Type messageContract, IEnumerable<Type> messageHandlerTypes)
        {
            foreach (Type handlerType in messageHandlerTypes)
            {
                if (HandlerCache.Contains(handlerType, messageContract))
                    continue;

                Action<object, object> handlerAction = GetHandlerAction(handlerType, messageContract);

                HandlerCache.SaveHandlerDetails(handlerType, messageContract, handlerAction);
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

        private static ICollection<Type> GetQueryHandlerTypes(AssemblyScanner scanner)
        {
            return
                scanner.Types.Where(
                    t => t.GetInterfaces()
                          .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAnswerQuery<,>)))
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