using System;
using System.Data.Entity;

namespace Hermes.EntityFramework
{
    public class LookupTableFactory : ILookupTableFactory
    {
        readonly DbContext context;
        private bool disposed;

        public LookupTableFactory(IContextFactory contextFactory)
        {
            context = contextFactory.GetContext();
        }        

        public void Add<TWrapper, TEnum>()
            where TWrapper : EnumWrapper<TEnum>
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            context.RegisterLookupTable<TWrapper, TEnum>();
        }

        public void Add<TLookup>(TLookup lookup) where TLookup : class, ILookupTable
        {
            context.RegisterLookupTable(lookup);
        }

        ~LookupTableFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && context != null)
            {
                context.SaveChanges();
                context.Dispose();
            }

            disposed = true;
        }
    }
}