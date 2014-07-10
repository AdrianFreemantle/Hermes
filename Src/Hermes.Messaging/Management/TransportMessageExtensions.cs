using System.Linq;
using System.Text;
using Hermes.Messaging.Transports;

namespace Hermes.Messaging.Management
{
    public static class TransportMessageExtensions
    {
        public static TransportMessageDto ToDto(this TransportMessage m)
        {
            var body = Encoding.UTF8.GetString(m.Body);
            var endpoint = m.ReplyToAddress.ToString();
            var headers = m.Headers.Select(pair => new HeaderValue(pair.Key, pair.Value)).ToArray();

            return new TransportMessageDto(m.MessageId, m.CorrelationId, endpoint, body, headers);
        }
    }
}