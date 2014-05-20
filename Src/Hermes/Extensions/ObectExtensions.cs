using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Hermes
// ReSharper restore CheckNamespace
{
    public static class ObectExtensions
    {
        public static bool HasAttribute<TAttribute>(this object o) where TAttribute : Attribute
        {
            return Attribute.IsDefined(o.GetType(), typeof(TAttribute));
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this object o) where TAttribute : Attribute
        {
            return o.GetType().GetCustomAttributes(typeof (TAttribute), true).Cast<TAttribute>();
        }
    }
}