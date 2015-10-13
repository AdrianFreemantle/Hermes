namespace Hermes.Domain
{
    public interface IIdentity
    {
        dynamic GetId();
        bool IsEmpty();
        string GetTag();
    }
}