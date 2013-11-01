using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.ProcessManagement;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging
{
    public class Dispatcher : IDispatchMessagesToHandlers
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof(Dispatcher)); 

        public void DispatchToHandlers(object message, IServiceLocator serviceLocator)
        {
            object[] handlers = GetHandlers(message, serviceLocator).ToArray();
            ValidateCommandMessageHandlers(message, handlers);

            if (handlers.Any())
            {
                TrySaveSaga(handlers);
            }
            else if (!Settings.IsClientEndpoint)
            {
                throw new InvalidOperationException(String.Format("No handlers could be found for message {0}", message.GetType()));
            }
        }

        private static void ValidateCommandMessageHandlers(object message, IEnumerable<object> handlers)
        {
            if (Settings.IsCommandType != null && Settings.IsCommandType(message.GetType()) && handlers.Count() != 1)
            {
                throw new InvalidOperationException("A command must have one and only one handler allocated to it.");
            }
        }

        private static void TrySaveSaga(IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {
                var saga = handler as ProcessManager;

                if (saga != null)
                {
                    saga.Save();
                }
            }
        }

        private IEnumerable<object> GetHandlers(object message, IServiceLocator serviceLocator)
        {
            var messageType = message.GetType();

            Type[] contracts = Settings.IsEventType(messageType)
                                   ? message.GetType().GetInterfaces().Union(new[] { messageType }).ToArray()
                                   : new[] { messageType };

            var handlerTypes = contracts.Select(GetHandlerType).Where(type => type != null).Distinct();

            var handlers = new Dictionary<string, object>();

            foreach (var type in handlerTypes)
            {
                foreach (var handler in serviceLocator.GetAllInstances(type).Distinct())
                {
                    handlers[handler.GetType().FullName] = handler;
                }
            }

            foreach (Type contract in contracts)
            {
                foreach (var handler in handlers.Values)
                {
                    var method = GetMethod(handler.GetType(), contract);
                     
                    if (method != null)
                    {
                        method.Invoke(handler, message);
                    }
                }
            }

            return handlers.Values;
        }

        private static Type GetHandlerType(Type contractType)
        {
            Type handlerGenericType = typeof (IHandleMessage<>);
            return handlerGenericType.MakeGenericType(new[] { contractType });
        }

        private static Action<object, object> GetMethod(Type targetType, Type messageType)
        {
            Type interfaceGenericType = typeof(IHandleMessage<>);
            var interfaceType = interfaceGenericType.MakeGenericType(messageType);

            if (interfaceType.IsAssignableFrom(targetType))
            {
                var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
                if (methodInfo != null)
                {
                    var target = Expression.Parameter(typeof(object));
                    var param = Expression.Parameter(typeof(object));

                    var castTarget = Expression.Convert(target, targetType);
                    var castParam = Expression.Convert(param, methodInfo.GetParameters().First().ParameterType);
                    var execute = Expression.Call(castTarget, methodInfo, castParam);
                    return Expression.Lambda<Action<object, object>>(execute, target, param).Compile();
                }
            }

            return null;
        }
    }
}
