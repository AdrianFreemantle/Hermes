using System;

namespace Hermes.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UnitOfWorkCommitOrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}