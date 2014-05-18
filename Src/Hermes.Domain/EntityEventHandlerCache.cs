using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hermes.Domain
{
    internal static class EntityEventHandlerCache
    {
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<Type, IReadOnlyCollection<EventHandlerProperties>> EventHandlers = new Dictionary<Type, IReadOnlyCollection<EventHandlerProperties>>();

        public static IReadOnlyCollection<EventHandlerProperties> ScanEntity(EntityBase entityBase)
        {
            var entityType = entityBase.GetType();

            try
            {
                ReaderWriterLock.EnterReadLock();
                
                if (EventHandlers.ContainsKey(entityType))
                {
                    return EventHandlers[entityType];
                }
            }
            finally 
            {
                ReaderWriterLock.ExitReadLock();
            }

            ScanTypeForHandlers(entityType);
            return EventHandlers[entityType];
        }

        private static void ScanTypeForHandlers(Type entityType)
        {
            ReaderWriterLock.EnterWriteLock();

            try
            {
                if (EventHandlers.ContainsKey(entityType))
                {
                    return;
                }

                GetEventHandlers(entityType);
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }

        private static void GetEventHandlers(Type entityType)
        {            
            Type eventBaseType = typeof(IDomainEvent);

            var methodsToMatch = entityType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var matchedMethods = from method in methodsToMatch
                                 let parameters = method.GetParameters()
                                 where
                                     method.Name.Equals("When", StringComparison.InvariantCulture) &&
                                         parameters.Length == 1 &&
                                         eventBaseType.IsAssignableFrom(parameters[0].ParameterType)
                                 select
                                     new { MethodInfo = method, FirstParameter = method.GetParameters()[0] };

            EventHandlers[entityType] = matchedMethods.Select(method => EventHandlerProperties.CreateFromMethodInfo(method.MethodInfo, entityType)).ToArray();
        }
    }
}