using System;
using System.Data.Entity;
using Hermes.Logging;

namespace Hermes.EntityFramework
{
    public class ContextFactory<TContext> : IContextFactory
        where TContext : DbContext, new()
    {
        protected readonly ILog Logger;
        internal static string ConnectionStringName { get; set; }
        public static bool DebugTrace { get; set; }

        public ContextFactory()
        {
            Logger = LogFactory.BuildLogger(typeof(ContextFactory<TContext>));
        }

        public ContextFactory(string connectionStringName)
        {
            Logger = LogFactory.BuildLogger(typeof(ContextFactory<TContext>));
            ConnectionStringName = connectionStringName;
        }

        public DbContext GetContext(ContextConfiguration configuration)
        {
            TContext context = String.IsNullOrWhiteSpace(ConnectionStringName)
                ? new TContext()
                : Activator.CreateInstance(typeof (TContext), ConnectionStringName) as TContext;

            ConfigureContext(context, configuration);

            return context;
        }

        private void ConfigureContext(TContext context, ContextConfiguration contextConfiguration)
        {
            if (DebugTrace)
                context.Database.Log = s => Logger.Info(s);

            if (contextConfiguration == ContextConfiguration.Queryable)
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.AutoDetectChangesEnabled = false;
            }
        }
    }
}