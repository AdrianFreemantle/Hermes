namespace Hermes.Domain
{
    public interface IDomainEvent
    {
        int Version { get; }
    }
}