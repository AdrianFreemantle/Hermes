using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Hermes.Enums;
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
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                .Select(t => ObjectFactory.CreateInstance<TWrapper>(t))
                .ToArray();

            AddOrUpdate(context.Set<TWrapper>(), enumValues);
        }

        public static void RegisterLookupTable<TLookup>(this DbContext context, TLookup lookup) 
            where TLookup : class, ILookupTable
        {
            AddOrUpdate(context.Set<TLookup>(), lookup);
        }

        public static void RegisterLookupTable<TLookup>(this DbContext context, params TLookup[] lookup)
            where TLookup : class, ILookupTable
        {
            AddOrUpdate(context.Set<TLookup>(), lookup);
        }

        private static void AddOrUpdate<TLookup>(DbSet<TLookup> dbSet, params TLookup[] lookups) 
            where TLookup : class, ILookupTable
        {
            dbSet.AddOrUpdate(t => t.Id, lookups);
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