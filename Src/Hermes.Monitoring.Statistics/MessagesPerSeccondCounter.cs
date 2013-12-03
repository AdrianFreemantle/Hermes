using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Timers;

using Hermes.Logging;
using Hermes.Serialization;
using Hermes.Sql;

namespace Hermes.Monitoring.Statistics
{
    public class MessagesPerSeccondCounter
    {
        private readonly ISerializeObjects serializer;
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessagesPerSeccondCounter));
        private readonly string connectionString;
        private readonly Timer timer;
        private bool disposed;
        private long previousAudit;
        private long previousError;

        public MessagesPerSeccondCounter(ISerializeObjects serializer)
        {
            this.serializer = serializer;
            connectionString = ConfigurationManager.ConnectionStrings["SqlTransport"].ConnectionString;

            timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(10).TotalMilliseconds,
                AutoReset = true,                
            };

            timer.Elapsed += Elapsed;
            timer.Start();
            previousAudit = GetCurrentAuditMessageCount();
            previousError = GetCurrentErrorMessageCount();
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            PerformanceCounter performanceCounter = GetNextAuditMessageDetails();

            foreach (var metric in performanceCounter.GetEndpointPerformance())
            {
                logger.Info("[Endpoint: {0}] [MSG: {1}] [MSG/S: {2}] [ATTD: {3} ] [ATTP: {4}]", metric.Endpoint, metric.TotalMessagesProcessed, (int)(metric.TotalMessagesProcessed / 10), performanceCounter.AverageTimeToDelivery, performanceCounter.AverageTimeToProcess);
            }
        }

        private long GetCurrentAuditMessageCount()
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT TOP 1 [RowVersion] " +
                    "FROM [queue].[Audit] " +
                    "ORDER BY [RowVersion] desc");

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private long GetCurrentErrorMessageCount()
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT TOP 1 [RowVersion] " +
                    "FROM [queue].[Error] " +
                    "ORDER BY [RowVersion] desc");

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private PerformanceCounter GetNextAuditMessageDetails()
        {
            var performanceCounter = new PerformanceCounter();

            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT [RowVersion], [Headers] " +
                    "FROM [queue].[Audit] " +
                    "WHERE [RowVersion] > @previous " +
                    "ORDER BY [RowVersion]", new SqlParameter("previous", previousAudit));

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        previousAudit = reader.GetInt64(0);
                        string header = reader.GetString(1);

                        var messageHeader = serializer.DeserializeObject<Dictionary<string, string>>(header);

                        performanceCounter.Add(new MessagePerformanceMetric(messageHeader));
                    }
                }
            }

            return performanceCounter;
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
