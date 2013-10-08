namespace Hermes.EntityFramework
{
    public interface ICurrentUser
    {
        string UserName { get; }
        bool IsAuthenticated { get; }
    }
}