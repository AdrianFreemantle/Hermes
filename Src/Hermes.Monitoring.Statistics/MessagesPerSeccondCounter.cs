using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Timers;

using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Serialization.Json;
using Hermes.Sql;

namespace Hermes.Monitoring.Statistics
{
    public class MessagesPerSeccondCounter
    {
        private static readonly ILog logger = LogFactory.BuildLogger(typeof (MessagesPerSeccondCounter));
        private readonly string connectionString;
        private readonly Timer timer;
        private bool disposed;
        private long previous;

        public MessagesPerSeccondCounter()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SqlTransport"].ConnectionString;

            timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(10).TotalMilliseconds,
                AutoReset = true,                
            };

            timer.Elapsed += Elapsed;
            timer.Start();
            previous = GetCurrentMessageCount();            
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            var metrics = GetNextAuditMessageDetails();

            if (metrics.Any())
            {
                TimeSpan averageTtd = TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToDeliver.Ticks));
                TimeSpan averageTtp = TimeSpan.FromTicks((long)metrics.Average(metric => metric.TimeToProcess.Ticks));
                TimeSpan maxTtd = metrics.Max(metric => metric.TimeToDeliver);
                TimeSpan maxTtp = metrics.Max(metric => metric.TimeToProcess);

                logger.Info("[MSG/S {0}] [ATTD: {1} ] [ATTP: {2}] [MxTTD: {3}] [MxTTP: {4}]", (int)(metrics.Count/10), averageTtd, averageTtp, maxTtd, maxTtp);
            }
        }

        private long GetCurrentMessageCount()
        {
            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT TOP 1 [RowVersion] " +
                    "FROM [queue].[Audit] " +
                    "ORDER BY [RowVersion] desc");

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private List<MessagePerformanceMetric> GetNextAuditMessageDetails()
        {
            var performanceMetrics = new List<MessagePerformanceMetric>();

            using (var connection = TransactionalSqlConnection.Begin(connectionString, IsolationLevel.ReadCommitted))
            {
                var command = connection.BuildCommand("SELECT [RowVersion], [Headers] " +
                    "FROM [queue].[Audit] " +
                    "WHERE [RowVersion] > @previous " +
                    "ORDER BY [RowVersion]", new SqlParameter("previous", previous));

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        previous = reader.GetInt64(0);
                        string header = reader.GetString(1);
                        performanceMetrics.Add(new MessagePerformanceMetric(header));
                    }
                }
            }

            return performanceMetrics;
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

    

    public class MessagePerformanceMetric
    {
        private static readonly JsonObjectSerializer serializer = new JsonObjectSerializer();
        private readonly Dictionary<string, string> headers;

        public TimeSpan TimeToProcess { get; private set; }
        public TimeSpan TimeToDeliver { get; private set; }
        public Address Endpoint { get; private set; }

        public MessagePerformanceMetric(string header)
        {
            Mandate.ParameterNotNullOrEmpty(header, "header", "A valid header string is required in order to determine message performance metrics");
            headers = serializer.DeserializeObject<Dictionary<string, string>>(header);

            DateTime completedTime = headers[HeaderKeys.CompletedTime].ToUtcDateTime();
            DateTime sentTime = headers[HeaderKeys.SentTime].ToUtcDateTime();
            DateTime receivedTime = headers[HeaderKeys.ReceivedTime].ToUtcDateTime();
            Endpoint = Address.Parse(headers[HeaderKeys.ProcessingEndpoint]);

            TimeToProcess = completedTime - receivedTime;
            TimeToDeliver = receivedTime - sentTime;
        }
    }
}
