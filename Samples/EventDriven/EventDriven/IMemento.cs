namespace EventDriven
{
    public interface IMemento
    {
        IHaveIdentity Identity { get; set; }
    }
}