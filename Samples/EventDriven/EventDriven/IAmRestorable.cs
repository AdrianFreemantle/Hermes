namespace EventDriven
{
    public interface IAmRestorable
    {
        IMemento GetSnapshot();
        void RestoreSnapshot(IMemento memento);
    }
}