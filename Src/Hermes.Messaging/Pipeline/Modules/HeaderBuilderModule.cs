using System;
using System.Collections.Generic;
using System.Linq;

using Hermes.Logging;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class HeaderBuilderModule : IModule<OutgoingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(HeaderBuilderModule));

        public bool Invoke(OutgoingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Building headers for message {0}.", input);
            input.BuildHeaderFunction(BuildMessageHeaders);
            return next();
        }

        private Dictionary<string, string> BuildMessageHeaders(OutgoingMessageContext context)
        {
            var headers = new Dictionary<string, string>();

            ConvertHeaderValues(context.Headers, headers);
            AddMessageTypeToHeader(context.OutgoingMessage, headers);
            headers.Add(HeaderKeys.SentTime, DateTime.UtcNow.ToWireFormattedString());
            
            return headers;
        }

        private void ConvertHeaderValues(IEnumerable<HeaderValue> headerValues, Dictionary<string, string> headers)
        {
            foreach (var messageHeader in headerValues)
            {
                headers[messageHeader.Key] = messageHeader.Value;
            }
        }

        private void AddMessageTypeToHeader(object message, Dictionary<string, string> headers)
        {
            if (message != null)
            {
                string messageType = String.Join(";", message.GetContracts().Select(type => type.FullName));
                headers.Add(HeaderKeys.MessageType, messageType);
            }
        }
    }
}