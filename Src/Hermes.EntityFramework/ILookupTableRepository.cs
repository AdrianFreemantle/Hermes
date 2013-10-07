using System;

namespace Hermes.EntityFramework
{
    public interface ILookupTableRepository
    {
        /// <summary>
        /// Retrieves a shared lookup table entity
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns>A shared lookup table entity</returns>
        TLookup Get<TLookup>(Enum id) where TLookup : ILookupTable;
        TLookup Get<TLookup>(int id) where TLookup : ILookupTable;
    }
}