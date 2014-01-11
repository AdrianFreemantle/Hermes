using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Reflection
{
    public static class TypeExtensions
    {
        private static readonly IDictionary<Type, string> TypeToNameLookup = new Dictionary<Type, string>();

        public static bool IsSimpleType(this Type type)
        {
            return (type == typeof(string) ||
                type.IsPrimitive ||
                type == typeof(decimal) ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan) ||
                type == typeof(DateTimeOffset) ||
                type.IsEnum);
        }

        public static string SerializationFriendlyName(this Type t)
        {
            lock (TypeToNameLookup)
                if (TypeToNameLookup.ContainsKey(t))
                    return TypeToNameLookup[t];

            var index = t.Name.IndexOf('`');

            if (index >= 0)
            {
                var result = t.Name.Substring(0, index) + "Of";
                var args = t.GetGenericArguments();
                for (var i = 0; i < args.Length; i++)
                {
                    result += args[i].SerializationFriendlyName();
                    if (i != args.Length - 1)
                        result += "And";
                }

                if (args.Length == 2)
                    if (typeof(KeyValuePair<,>).MakeGenericType(args) == t)
                        result = "Hermes." + result;

                lock (TypeToNameLookup)
                    TypeToNameLookup[t] = result;

                return result;
            }

            lock (TypeToNameLookup)
                TypeToNameLookup[t] = t.Name;

            return t.Name;
        }
    }
}
