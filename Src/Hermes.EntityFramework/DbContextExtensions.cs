using System;
using System.Data.Entity;
using System.Linq;

using Hermes.Persistence;
using Hermes.Reflection;

namespace Hermes.EntityFramework
{
    public static class DbContextExtensions
    {
        public static void RegisterLookupTable<TWrapper, TEnum>(this DbContext context)
            where TWrapper : EnumWrapper<TEnum>
            where TEnum : struct, IComparable, IFormattable, IConvertible 
        {
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            foreach (var enumValue in enumValues)
            {
                var wrapper = ObjectFactory.CreateInstance<TWrapper>(enumValue);
                var dbSet = context.Set<TWrapper>();
                AddOrUpdate(wrapper, dbSet);
            }
        }

        public static void RegisterLookupTable<TLookup>(this DbContext context, TLookup lookup) 
            where TLookup : class, ILookupTable
        {
            var dbSet = context.Set<TLookup>();
            AddOrUpdate(lookup, dbSet);
        }

        private static void AddOrUpdate<TLookup>(TLookup lookup, DbSet<TLookup> dbSet) 
            where TLookup : class, ILookupTable
        {
            var savedItem = dbSet.SingleOrDefault(t => t.Id == lookup.Id);

            if (savedItem != null)
            {
                savedItem.Description = "this does not actually change anything, it makes ef think this value has been updated.";
            }
            else
            {
                dbSet.Add(lookup);
            }
        }

        /// <summary>
        /// This method ensures that there is only ever one instance of
        /// an enum wrapper object in use for a given context. We do this because entity 
        /// framework does not allow us add two copies of the same entity type
        /// with identical keys to the context. This means that for a given lookup table
        /// entity, only one instance with a specific ID may ever be in circulation 
        /// for a given context. 
        /// </summary>
        /// <typeparam name="TLookup">The lookup table type</typeparam>
        /// <returns>A shared lookup table entity</returns>
        public static TLookup Fetch<TLookup>(this DbContext context, int id)
            where TLookup : ILookupTable
        {
            return (TLookup)context.Set(typeof(TLookup)).Find(id);
        }
    }
}