using System;

namespace Hermes.Domain
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EntityIdAttribute : Attribute
    {
    }
}