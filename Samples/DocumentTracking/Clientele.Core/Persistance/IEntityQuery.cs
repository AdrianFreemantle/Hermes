using System.Linq;

namespace Clientele.Core.Persistance
{
    public interface IEntityQuery
    {
        IQueryable<TEntity> Query<TEntity>() where TEntity : class;
        void EnableLazyLoading();
        void DisableLazyLoading();
    }
}
