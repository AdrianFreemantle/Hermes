using System.Collections.Generic;
using System.Linq;

namespace Hermes.Messaging.Transports
{
    public static class HeaderValueExtensions
    {
        public static IEnumerable<HeaderValue> ToHeaderValues(this Dictionary<string, string> dictionary)
        {
            return dictionary.Select(keyValuePair => new HeaderValue(keyValuePair.Key, keyValuePair.Value));
        }

        public static Dictionary<string, string> ToDictionary(this IEnumerable<HeaderValue> headerValues)
        {
            var headers = new Dictionary<string, string>();

            foreach (var messageHeader in headerValues)
            {
                headers[messageHeader.Key] = messageHeader.Value;
            }

            return headers;
        }
    }
}