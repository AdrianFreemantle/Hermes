using System;

namespace Hermes.Messaging
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UnitOfWorkCommitOrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}