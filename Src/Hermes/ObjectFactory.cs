using System;
using System.Linq;
using System.Reflection;

namespace Hermes
{
    public static class ObjectFactory
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static object CreateInstance(Type type, params object[] parameters)
        {
            Type[] types = parameters.Length == 0
                ? new Type[0]
                : parameters.ToList().ConvertAll(input => input.GetType()).ToArray();

            var constructor = type.GetConstructor(Flags, null, types, null);
            return constructor.Invoke(parameters);
        }

        public static T CreateInstance<T>(params object[] parameters) where T : class
        {
            return CreateInstance(typeof (T), parameters) as T;
        }

        public static bool HasDefaultConstructor<T>()
        {
            return HasDefaultConstructor(typeof (T));
        }

        public static bool HasDefaultConstructor(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}