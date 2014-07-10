using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.Queries;
using Hermes.Serialization;

namespace Hermes.Messaging.Management
{
    public class SqlErrorQueueQuery
    {
        private readonly string connectionString;
        private readonly ISerializeObjects serializer;
        private static readonly string QueryErrorQueue = String.Format(SqlCommands.QeuryQueue, Settings.AuditEndpoint);
        private static readonly string QeuryErrorQueueCount = String.Format(SqlCommands.QeuryQueueCount, Settings.AuditEndpoint);

        public SqlErrorQueueQuery(ISerializeObjects serializer)
        {
            this.serializer = serializer;
            connectionString = Settings.GetSetting<string>(SqlTransportConfiguration.MessagingConnectionStringKey);
        }

        public int GetErrorCount()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return FetchErrorCount(connection);
            }
        }

        public PagedResult<TransportMessageDto> GetErrorMessages(int pageNumber, int resultsPerPage)
        {
            Mandate.ParameterCondition(pageNumber > 0, "pageNumber");
            Mandate.ParameterCondition(resultsPerPage > 0, "resultsPerPage");

            int start = StartRecord(pageNumber, resultsPerPage);
            int end = GetEndRecord(start, resultsPerPage);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                TransportMessageDto[] messages = FetchErrorMessages(connection, start, end);
                var count = FetchErrorCount(connection);

                return new PagedResult<TransportMessageDto>(messages, pageNumber, resultsPerPage, count);
            }
        }

        private TransportMessageDto[] FetchErrorMessages(SqlConnection connection, int start, int end)
        {
            var messages = new List<TransportMessage>();            

            using (var query = new SqlCommand(String.Format(QueryErrorQueue), connection))
            {
                query.Parameters.Add(new SqlParameter("@start", start));
                query.Parameters.Add(new SqlParameter("@end", end));

                using (var dataReader = query.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        TransportMessage message = dataReader.BuildTransportMessage(serializer);
                        messages.Add(message);
                    }
                }
            }

            return messages.Select(m => m.ToDto()).ToArray();
        }

        private int FetchErrorCount(SqlConnection connection)
        {
            using (var query = new SqlCommand(String.Format(QeuryErrorQueueCount), connection))
            {
                dynamic result = query.ExecuteScalar();
                return (int)result;
            }
        }

        private static int GetEndRecord(int start, int resultsPerPage)
        {
            return start + resultsPerPage - 1;
        }

        private int StartRecord(int pageNumber, int resultsPerPage)
        {
            if (pageNumber == 1)
            {
                return 1;
            }

            return pageNumber * resultsPerPage - 1;
        }
    }
}
