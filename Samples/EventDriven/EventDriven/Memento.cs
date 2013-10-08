namespace EventDriven
{
    public abstract class Memento : IMemento
    {
        IHaveIdentity IMemento.Identity { get; set; }
    }
}