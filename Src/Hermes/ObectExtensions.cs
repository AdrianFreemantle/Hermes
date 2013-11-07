using System;

namespace Hermes
{
    public static class ObectExtensions
    {
        public static bool HasAttribute<TAttribute>(this object o) where TAttribute : Attribute
        {
            return Attribute.IsDefined(o.GetType(), typeof(TAttribute));
        }
    }
}