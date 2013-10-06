namespace Hermes.Domain
{
    public interface IHaveIdentity
    {
        dynamic GetId();
        bool IsEmpty();
        string GetTag();
    }
}