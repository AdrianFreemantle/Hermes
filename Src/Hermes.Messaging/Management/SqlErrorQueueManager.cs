using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.Queries;
using Hermes.Serialization;

namespace Hermes.Messaging.Management
{   
    public class SqlErrorQueueManager : IManageErrorQueues
    {
        private readonly string connectionString;
        private readonly ISerializeObjects serializer;
        private readonly ISendMessages messageSender;
        private static readonly string DeleteErrorMessage = String.Format(SqlCommands.DeleteMessage, Settings.ErrorEndpoint);
        private static readonly string QueryErrorQueue = String.Format(SqlCommands.QeuryQueue, Settings.ErrorEndpoint);
        private static readonly string QeuryErrorQueueCount = String.Format(SqlCommands.QeuryQueueCount, Settings.ErrorEndpoint);

        public SqlErrorQueueManager(ISerializeObjects serializer, ISendMessages messageSender)
        {
            this.serializer = serializer;
            this.messageSender = messageSender;
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

        public void Delete(Guid id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(String.Format(DeleteErrorMessage), connection))
                {
                    command.Parameters.Add(new SqlParameter("@id", id));
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Resend(TransportMessageDto dto)
        {
            Dictionary<string, string> headers = dto.Headers.ToDictionary();
            TransportMessage message = dto.ToTransportMessage(headers);
            Address destinationAddress = Address.Parse(headers[HeaderKeys.ProcessingEndpoint]);

            using (var scope = TransactionScopeUtils.Begin(IsolationLevel.ReadCommitted))
            {
                Delete(dto.MessageId);
                messageSender.Send(message, destinationAddress);
                scope.Complete();
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

            return (pageNumber - 1) * resultsPerPage;
        }
    }
}
