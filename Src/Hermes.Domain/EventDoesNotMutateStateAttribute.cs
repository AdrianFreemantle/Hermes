using System;

namespace Hermes.Domain
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EventDoesNotMutateStateAttribute : Attribute
    {
    }
}