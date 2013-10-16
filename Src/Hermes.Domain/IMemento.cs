namespace Hermes.Domain
{
    public interface IMemento
    {
        IIdentity Identity { get; set; }
    }
}