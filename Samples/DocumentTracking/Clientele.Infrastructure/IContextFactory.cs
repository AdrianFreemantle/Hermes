using System.Data.Entity;

namespace Clientele.Infrastructure
{
    public interface IContextFactory
    {
        DbContext GetContext();
    }
}
