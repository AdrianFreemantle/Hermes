using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Hermes.Failover;
using Hermes.Logging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class HeaderBuilderModule : IModule<OutgoingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(HeaderBuilderModule));

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Building headers for message {0}.", input);
            input.BuildHeaderFunction(BuildMessageHeaders);
            return next();
        }

        private Dictionary<string, string> BuildMessageHeaders(OutgoingMessageContext context)
        {
            var headers = context.Headers.ToDictionary();

            AddMessageTypeToHeader(context.OutgoingMessage, headers);
            AddUserName(headers);
            SetSentTime(headers);

            return headers;
        }

        private static void SetSentTime(Dictionary<string, string> headers)
        {
            headers.Add(HeaderKeys.SentTime, DateTime.UtcNow.ToWireFormattedString());
        }

        private void AddUserName(Dictionary<string, string> headers)
        {
            string userName;

            if (CurrentUser.GetCurrentUserName(out userName) && !String.IsNullOrWhiteSpace(userName)) 
            {
                headers.Add(HeaderKeys.UserName, userName);
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