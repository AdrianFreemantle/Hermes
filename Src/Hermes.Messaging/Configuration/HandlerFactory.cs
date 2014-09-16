using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hermes.Messaging.Configuration
{
    internal static class HandlerFactory
    {
        public static Action<object, object> BuildHandlerAction(Type handlerType, Type messageContract)
        {
            Type interfaceGenericType = typeof(IHandleMessage<>);
            var interfaceType = interfaceGenericType.MakeGenericType(messageContract);

            if (!interfaceType.IsAssignableFrom(handlerType))
                return null;

            var methodInfo = handlerType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();

            if (methodInfo == null)
                return null;
            
            ParameterInfo firstParameter = methodInfo.GetParameters().First();

            if (firstParameter.ParameterType != messageContract)
                return null;

            Expression<Action<object, object>> handlerAction = BuildHandlerExpression(handlerType, methodInfo);
            return handlerAction.Compile();
        }

        private static Expression<Action<object, object>> BuildHandlerExpression(Type handlerType, MethodInfo methodInfo)
        {
            var handler = Expression.Parameter(typeof (object));
            var message = Expression.Parameter(typeof (object));

            var castTarget = Expression.Convert(handler, handlerType);
            var castParam = Expression.Convert(message, methodInfo.GetParameters().First().ParameterType);
            var execute = Expression.Call(castTarget, methodInfo, castParam);
            return Expression.Lambda<Action<object, object>>(execute, handler, message);
        }
    }
}