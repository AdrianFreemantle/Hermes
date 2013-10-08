namespace EventDriven
{
    public interface IHaveIdentity
    {
        dynamic GetId();
        bool IsEmpty();
        string GetTag();
    }
}