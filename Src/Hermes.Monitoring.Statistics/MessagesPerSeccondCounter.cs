using System;
using System.Configuration;
using System.Data;
using System.Timers;

using Hermes.Logging;
using Hermes.Sql;

namespace Hermes.Monitoring.Statistics
{
    public class MessagesPerSeccondCounter
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessagesPerSeccondCounter));
        private readonly string connectionString;
        private readonly Timer timer;
        private bool disposed;
        private long current;
        private long previous;

        public MessagesPerSeccondCounter()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SqlTransport"].ConnectionString;

            timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(1).TotalMilliseconds,
                AutoReset = true,                
            };

            timer.Elapsed += Elapsed;
            timer.Start();
            previous = current = GetCurrentMessageCount();            
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            current = GetCurrentMessageCount();     
            logger.Info("Completed Messages {0}", current - previous);
            previous = current;
        }

        private long GetCurrentMessageCount()
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT TOP 1 [RowVersion]" +
                    "FROM [MessageBroker].[queue].[Audit]" +
                    "ORDER BY [RowVersion] desc");

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        ~MessagesPerSeccondCounter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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

            if (disposing)
            {
                timer.Dispose();
            }

            disposed = true;
        }
    }
}
