using System;
using System.Linq;
using System.Reflection;

namespace MyDomain
{
    public static class ActivatorHelper
    {
        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static T CreateInstance<T>(params object[] parameters) where T : class
        {
            return CreateInstance(typeof(T), parameters) as T;
        }

        public static object CreateInstance(Type type, params object[] parameters)
        {
            Type[] types = parameters.ToList().ConvertAll(input => input.GetType()).ToArray();
            var constructor = type.GetConstructor(Flags, null, types, null);
            return constructor.Invoke(parameters);
        }
    }
}