using System;
using System.Data.Entity;
using Hermes.Logging;

namespace Hermes.EntityFramework
{
    public class ContextFactory<TContext> : IContextFactory
        where TContext : DbContext, new()
    {
        protected readonly ILog Logger;
        protected DbContext Context;

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

        public DbContext GetContext()
        {
            if (Context == null)
                BuildContext();

            return Context;
        }

        private void BuildContext()
        {
            TContext context = String.IsNullOrWhiteSpace(ConnectionStringName)
                ? new TContext()
                : Activator.CreateInstance(typeof (TContext), ConnectionStringName) as TContext;

            if (context != null && DebugTrace)
            {
                context.Database.Log = s => Logger.Debug(s);
            }

            Context = context;
        }
    }
}