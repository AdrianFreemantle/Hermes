using System;
using System.Collections.Generic;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Pipeline;

namespace Hermes.Messaging
{
    public class LocalSession
    {
        private readonly Guid messageId;
        private readonly object message;
        private readonly Dictionary<string, string> headers;
        private readonly IStoreLocalMessages messageStore;

        private LocalSession()
        {
        }

        private LocalSession(IncomingMessageContext messageContext)
        {
            messageStore = messageContext.ServiceLocator.GetInstance<IStoreLocalMessages>();
           
            message = messageContext.Message;
            messageId = messageContext.MessageId;

            headers = new Dictionary<string, string>();
            headers[HeaderKeys.MessageType] = messageContext.Message.GetType().FullName;   
            headers[HeaderKeys.ReceivedTime] = DateTime.Now.ToWireFormattedString();

            if (!String.IsNullOrWhiteSpace(messageContext.GetUserName()))
                headers[HeaderKeys.UserName] = messageContext.GetUserName();
        }

        public static LocalSession Begin(IncomingMessageContext messageContext)
        {
            if (!Settings.UseLocalMessageStore)
                return new LocalSession();

            return new LocalSession(messageContext);
        }

        public void AddErrorDetails(Exception ex)
        {
            if (!Settings.UseLocalMessageStore)
                return;

            headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
        }

        public void Commit()
        {
            if (!Settings.UseLocalMessageStore)
                return;

            messageStore.SaveSession(message, messageId, headers);
        }
    }
}