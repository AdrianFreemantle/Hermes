using System;
using System.Data.Entity;

namespace Clientele.Infrastructure
{
    public class ContextFactory<TContext> : IContextFactory
        where TContext : DbContext, new()
    {
        private readonly string connectionStringName;

        public ContextFactory(string connectionStringName)
        {
            this.connectionStringName = connectionStringName;
        }

        public DbContext GetContext()
        {
            return String.IsNullOrWhiteSpace(connectionStringName)
                ? new TContext()
                : Activator.CreateInstance(typeof(TContext), connectionStringName) as TContext;
        }
    }
}