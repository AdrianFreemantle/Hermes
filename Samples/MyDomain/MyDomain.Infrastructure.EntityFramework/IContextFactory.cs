using System.Data.Entity;

namespace MyDomain.Infrastructure.EntityFramework
{
    public interface IContextFactory
    {
        DbContext GetContext();
    }
}