using System;
using System.Runtime.Serialization;

namespace Hermes.Domain
{
    [DataContract]
    public abstract class Memento : IMemento
    {
        [DataMember(Name = "Identity")]
        public dynamic Identity { get; set; }

        [DataMember(Name = "Version")]
        public int Version { get; set; }
    }
}