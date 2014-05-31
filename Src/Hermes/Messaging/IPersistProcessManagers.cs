using System;
using System.Linq.Expressions;

namespace Hermes.Messaging
{
    public interface IPersistProcessManagers
    {
        /// <summary>
        /// Saves the saga entity to the persistence store.
        /// </summary>
        /// <param name="state">The saga entity to save.</param>
        void Create<T>(T state) where T : class, IContainProcessManagerData, new();

        void Update<T>(T state) where T : class, IContainProcessManagerData, new();

        /// <summary>
        /// Gets a saga entity from the persistence store by its Id.
        /// </summary>
        /// <param name="processId">The Id of the saga entity to get.</param>
        /// <returns></returns>
        T Get<T>(Guid processId) where T : class, IContainProcessManagerData, new();

        /// <summary>
        /// Sets a saga as completed and removes it from the active saga list
        /// in the persistence store.
        /// </summary>
        void Complete<T>(Guid processId) where T : class, IContainProcessManagerData, new();

        T Find<T>(Expression<Func<T, bool>> expression) where T : class, IContainProcessManagerData, new();
    }
}