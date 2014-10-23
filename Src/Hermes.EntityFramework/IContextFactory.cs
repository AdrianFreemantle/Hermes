using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public interface IContextFactory
    {
        DbContext GetContext(ContextConfiguration configuration);
    }
}