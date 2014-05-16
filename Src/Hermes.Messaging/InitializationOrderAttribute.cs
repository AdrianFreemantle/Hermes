using System;

namespace Hermes.Messaging
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InitializationOrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}