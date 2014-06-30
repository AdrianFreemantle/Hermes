using System;
using System.Collections.Generic;
using Hermes.Messaging.Configuration;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class LocalMessageStoreModule : IModule<IncomingMessageContext>
    {
        private readonly IStoreLocalMessages messageStorage;

        public LocalMessageStoreModule(IStoreLocalMessages messageStorage)
        {
            this.messageStorage = messageStorage;
        }

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            LocalSession session = StartNewSession(input);

            try
            {
                return next();
            }
            catch (Exception ex)
            {
                AddErrorDetails(session, ex);
                return false;
            }
            finally
            {
                SaveSession(session);
            }
        }

        private void AddErrorDetails(LocalSession session, Exception ex)
        {
            if (session == null)
                return;

            session.Headers[HeaderKeys.FailureDetails] = ex.GetFullExceptionMessage();
        }

        private void SaveSession(LocalSession session)
        {
            if(session == null)
                return;

            messageStorage.SaveSession(session);
        }

        public LocalSession StartNewSession(IncomingMessageContext input)
        {
            if (!Settings.UseLocalMessageStore)
                return null;

            var session = new LocalSession
            {
                Headers = new Dictionary<string, string>(),
                Message = input.Message,
                MessageId = input.MessageId
            };

            session.Headers[HeaderKeys.MessageType] = input.Message.GetType().FullName;
            session.Headers[HeaderKeys.UserName] = input.GetUserName();
            session.Headers[HeaderKeys.ReceivedTime] = DateTime.Now.ToWireFormattedString();

            return session;
        }
    }
}