using System;

namespace Hermes.Domain
{
    [Serializable]
    public abstract class Memento : IMemento
    {
        IHaveIdentity IMemento.Identity { get; set; }
    }
}