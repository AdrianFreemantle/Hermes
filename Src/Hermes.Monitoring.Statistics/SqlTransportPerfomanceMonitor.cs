using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using Hermes.Serialization;
using Hermes.Sql;

namespace Hermes.Monitoring.Statistics
{
    public class PerformanceMetricEventArgs : EventArgs
    {
        public PerformanceMetricCollection PerformanceMetric { get; private set; }
        public TimeSpan MonitorPeriod { get; private set; }

        public PerformanceMetricEventArgs(PerformanceMetricCollection performanceMetric, TimeSpan monitorPeriod)
        {
            PerformanceMetric = performanceMetric;
            MonitorPeriod = monitorPeriod;
        }
    }

    public delegate void PerformanceMetricEventHandler(object sender, PerformanceMetricEventArgs e);

    public class SqlTransportPerfomanceMonitor
    {
        private readonly ISerializeObjects serializer;
        private readonly string connectionString;
        private readonly Timer timer;
        private bool disposed;
        private long previousAudit;
        private long previousError;
        private readonly TimeSpan monitoringPeriod = TimeSpan.FromSeconds(10);

        public event PerformanceMetricEventHandler OnPerformancePeriodCompleted;

        public SqlTransportPerfomanceMonitor(ISerializeObjects serializer)
        {
            this.serializer = serializer;
            connectionString = ConfigurationManager.ConnectionStrings["SqlTransport"].ConnectionString;

            timer = new Timer
            {
                Interval = monitoringPeriod.TotalMilliseconds,
                AutoReset = true,                
            };

            timer.Elapsed += Elapsed;
            timer.Start();

            previousAudit = GetCurrentAuditMessageCount();
            previousError = GetCurrentErrorMessageCount();
        }

        void Elapsed(object sender, ElapsedEventArgs e)
        {
            PerformanceMetricCollection performanceMetricCollection = GetNextAuditMessageDetails();

            if (OnPerformancePeriodCompleted != null)
            {
                OnPerformancePeriodCompleted(this, new PerformanceMetricEventArgs(performanceMetricCollection, monitoringPeriod));
            }           
        }

        private long GetCurrentAuditMessageCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT TOP 1 [RowVersion] " +
                                                    "FROM [queue].[Audit] " +
                                                    "ORDER BY [RowVersion] desc", connection))
                {
                    return Convert.ToInt64(command.ExecuteScalar());
                }
            }
        }

        private long GetCurrentErrorMessageCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT TOP 1 [RowVersion] " +
                                                    "FROM [queue].[Error] " +
                                                    "ORDER BY [RowVersion] desc", connection))
                {
                    return Convert.ToInt64(command.ExecuteScalar());
                }
            }
        }

        private PerformanceMetricCollection GetNextAuditMessageDetails()
        {
            var performanceCounter = new PerformanceMetricCollection();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT [RowVersion], [Headers] " +
                                                    "FROM [queue].[Audit] " +
                                                    "WHERE [RowVersion] > @previous " +
                                                    "ORDER BY [RowVersion]", connection))
                {
                    command.Parameters.Add(new SqlParameter("previous", previousAudit));

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
            }

            return performanceCounter;
        }

        ~SqlTransportPerfomanceMonitor()
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
