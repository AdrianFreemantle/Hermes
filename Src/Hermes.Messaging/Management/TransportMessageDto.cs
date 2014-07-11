using System;
using System.Collections.Generic;
using System.Text;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Management
{
    public class TransportMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public HeaderValue[] Headers { get; set; }
        public string Body { get; set; }
        public string ReplyToAddress { get; set; }

        public TransportMessageDto(Guid messageId, Guid correlationId, string endpoint, string body, HeaderValue[] headers)
        {
            Mandate.ParameterNotDefaut(messageId, "messageId");
            Mandate.ParameterNotDefaut(correlationId, "correlationId");
            Mandate.ParameterNotNullOrEmpty(endpoint, "endpoint");
            Mandate.ParameterNotNullOrEmpty(body, "body");
            Mandate.ParameterNotNullOrEmpty(headers, "headers");

            MessageId = messageId;
            CorrelationId = correlationId;
            ReplyToAddress = endpoint;
            Body = body;
            Headers = headers;
        }
    }

    public static class TransportMessageDtoExtensions
    {
        public static TransportMessage ToTransportMessage(this TransportMessageDto dto, Dictionary<string, string> headers)
        {
            Address replyToAddress = Address.Parse(dto.ReplyToAddress);
            byte[] body = Encoding.UTF8.GetBytes(dto.Body);

            var message = new TransportMessage(dto.MessageId, dto.CorrelationId, replyToAddress, TimeSpan.MaxValue, headers, body);

            message.Headers.Remove(HeaderKeys.FirstLevelRetryCount);
            message.Headers.Remove(HeaderKeys.SecondLevelRetryCount);
            message.Headers.Remove(HeaderKeys.FailureDetails);

            return message;
        }
    }
}