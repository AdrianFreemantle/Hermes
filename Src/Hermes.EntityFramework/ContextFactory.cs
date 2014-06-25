using System;
using System.Data.Entity;
using Hermes.Logging;

namespace Hermes.EntityFramework
{
    public class ContextFactory<TContext> : IContextFactory
        where TContext : DbContext, new()
    {
        protected readonly ILog Logger;
        private readonly string connectionStringName;

        public static bool DebugTrace { get; set; }

        public ContextFactory(string connectionStringName)
        {
            Logger = LogFactory.BuildLogger(typeof(DbContext));
            this.connectionStringName = connectionStringName;
        }

        public DbContext GetContext()
        {
            TContext context = String.IsNullOrWhiteSpace(connectionStringName)
                ? new TContext()
                : Activator.CreateInstance(typeof(TContext), connectionStringName) as TContext;

            if (context != null && DebugTrace)
                context.Database.Log = s => Logger.Debug(s);

            return context;
        }
    }
}