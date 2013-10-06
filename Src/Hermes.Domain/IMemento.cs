namespace Hermes.Domain
{
    public interface IMemento
    {
        IHaveIdentity Identity { get; set; }
    }
}