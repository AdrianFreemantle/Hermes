using System;
using System.Runtime.Serialization;

namespace Hermes.Domain
{
    [DataContract]
    public abstract class Memento : IMemento
    {
        [DataMember(Name = "Identity")]
        public IIdentity Identity { get; set; }
    }
}