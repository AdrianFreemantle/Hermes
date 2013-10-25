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
        private readonly IServiceLocator serviceLocator;

        public Dispatcher(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void DispatchToHandlers(object message)
        {
            object[] handlers = GetHandlers(message).ToArray();
            ValidateCommandMessageHandlers(message, handlers);

            if (handlers.Any())
            {
                //logger.Verbose("Dispatching message {0} to {1} handlers", message.GetType(), handlers.Length);
                //InvokeHandlers(handlers, message);
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

        private IEnumerable<object> GetHandlers(object message)
        {
            var messageType = message.GetType();

            Type[] contracts = Settings.IsEventType(messageType)
                                   ? message.GetType().GetInterfaces()
                                   : new[] {message.GetType()};

            var handlerTypes = contracts.Select(GetHandlerType).Where(type => type != null).Distinct();

            var handlers = new Dictionary<string, object>();

            foreach (var type in handlerTypes)
            {
                try
                {
                    foreach (var handler in serviceLocator.GetAllInstances(type).Distinct())
                    {
                        handlers[handler.GetType().FullName] = handler;
                    }
                }
                catch (Exception)
                {
                    //we dont support this event type
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

        //private static Action CreateHandlerAction(object message, object handler)
        //{
        //    return () => ((dynamic)handler).Handle((dynamic)message);
        //}


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

    public static class HandlerInvocationCache
    {
        /// <summary>
        /// Invokes the handle method of the given handler passing the message
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="message">The message instance.</param>
        public static void InvokeHandle(object handler, object message)
        {
            Invoke(handler, message, HandlerCache);
        }

        /// <summary>
        /// Invokes the timeout method of the given handler passing the message
        /// </summary>
        /// <param name="handler">The handler instance.</param>
        /// <param name="state">The message instance.</param>
        public static void InvokeTimeout(object handler, object state)
        {
            Invoke(handler, state, TimeoutCache);
        }

        /// <summary>
        /// Registers the method in the cache
        /// </summary>
        /// <param name="handler">The object type.</param>
        /// <param name="messageType">the message type.</param>
        public static void CacheMethodForHandler(Type handler, Type messageType)
        {
            CacheMethod(handler, messageType, typeof(IHandleMessage<>), HandlerCache);
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public static void Clear()
        {
            HandlerCache.Clear();
            TimeoutCache.Clear();
        }

        static void Invoke(object handler, object message, Dictionary<RuntimeTypeHandle, List<DelegateHolder>> dictionary)
        {
            List<DelegateHolder> methodList;

            if (!dictionary.TryGetValue(handler.GetType().TypeHandle, out methodList))
            {
                return;
            }

            foreach (var delegateHolder in methodList.Where(x => x.MessageType.IsInstanceOfType(message)))
            {
                delegateHolder.MethodDelegate(handler, message);
            }
        }

        static void CacheMethod(Type handler, Type messageType, Type interfaceGenericType, Dictionary<RuntimeTypeHandle, List<DelegateHolder>> cache)
        {
            var handleMethod = GetMethod(handler, messageType, interfaceGenericType);

            if (handleMethod == null)
            {
                return;
            }
            var delegateHolder = new DelegateHolder
            {
                MessageType = messageType,
                MethodDelegate = handleMethod
            };
            List<DelegateHolder> methodList;
            if (cache.TryGetValue(handler.TypeHandle, out methodList))
            {
                if (methodList.Any(x => x.MessageType == messageType))
                {
                    return;
                }
                methodList.Add(delegateHolder);
            }
            else
            {
                cache[handler.TypeHandle] = new List<DelegateHolder>
				    {
					    delegateHolder
				    };
            }
        }

        static Action<object, object> GetMethod(Type targetType, Type messageType, Type interfaceGenericType)
        {
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

        class DelegateHolder
        {
            public Type MessageType;
            public Action<object, object> MethodDelegate;
        }

        static readonly Dictionary<RuntimeTypeHandle, List<DelegateHolder>> HandlerCache = new Dictionary<RuntimeTypeHandle, List<DelegateHolder>>();
        static readonly Dictionary<RuntimeTypeHandle, List<DelegateHolder>> TimeoutCache = new Dictionary<RuntimeTypeHandle, List<DelegateHolder>>();
    }
}
