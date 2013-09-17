namespace Clientele.Core
{
    public interface IAmRestorable
    {
        IMemento GetSnapshot();
        void RestoreSnapshot(IMemento memento);
    }
}