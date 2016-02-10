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

        public static string ToPrettyString(this Type type)
        {
            return ToPrettyString(type, true);
        }

        public static string ToPrettyString(this Type type, bool useShortNotation)
        {
            if (type.IsGenericType == false)
            {
                if (useShortNotation == true)
                {
                    if (type == typeof(int)) return "int";
                    else if (type == typeof(void)) return "void";
                    else if (type == typeof(object)) return "object";
                    else if (type == typeof(string)) return "string";
                }

                return type.FullName;
            }
            else
            {
                Type genericType = type.GetGenericTypeDefinition();

                StringBuilder sb = new StringBuilder();

                sb.Append(genericType.FullName);
                sb.Append("[");
                bool isFirst = true;
                foreach (Type parameterType in type.GetGenericArguments())
                {
                    if (isFirst == true) isFirst = false;
                    else sb.Append(", ");

                    sb.Append(parameterType.ToPrettyString());
                }
                sb.Append("]");

                return sb.ToString();
            }
        }
    }
}
