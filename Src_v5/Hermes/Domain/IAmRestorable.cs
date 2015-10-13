namespace Hermes.Domain
{
    public interface IAmRestorable
    {
        IMemento GetSnapshot();
        void RestoreSnapshot(IMemento memento);
    }
}