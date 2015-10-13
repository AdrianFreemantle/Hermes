using System;

namespace Hermes.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InitializationOrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}