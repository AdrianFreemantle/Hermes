using System;

namespace Hermes.Saga
{
    public interface IPersistProcessManagers
    {
        /// <summary>
        /// Saves the saga entity to the persistence store.
        /// </summary>
        /// <param name="state">The saga entity to save.</param>
        void Create<T>(T state) where T : class, IContainProcessManagerData;


        void Update<T>(T state) where T : class, IContainProcessManagerData;

        /// <summary>
        /// Gets a saga entity from the persistence store by its Id.
        /// </summary>
        /// <param name="sagaId">The Id of the saga entity to get.</param>
        /// <returns></returns>
        T Get<T>(Guid sagaId) where T : class, IContainProcessManagerData;

        /// <summary>
        /// Sets a saga as completed and removes it from the active saga list
        /// in the persistence store.
        /// </summary>
        void Complete(Guid sagaId);
    }
}