using System;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public static class MessageRuleValidation
    {
        public static void ValidateIsCommandType(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            if (Settings.IsCommandType == null)
            {
                throw new InvalidOperationException("No rules have been configured for command message types. Use the DefineCommandAs function during endpoint configuration to configure this rule.");
            }

            foreach (var message in messages)
            {
                if (!Settings.IsCommandType(message.GetType()))
                {
                    var error = String.Format("Send is reserved for messages that have been defined as commands using the DefineCommandAs" +
                        " function during endpoing configuration. Message {0} does not comply with the current rule.", message.GetType().FullName);
                    throw new InvalidOperationException(error);
                }
            }
        }

        public static void ValidateIsEventType(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            if (Settings.IsEventType == null)
            {
                throw new InvalidOperationException("No rules have been configured for event message types. Use the DefineEventAs function during endpoint configuration to configure this rule.");
            }

            foreach (var message in messages)
            {
                if (!Settings.IsEventType(message.GetType()))
                {
                    var error = String.Format("Publish is reserved for messages that have been defined as events using the DefineEventAs" +
                        " function during endpoing configuration. Message {0} does not comply with the current rule.", message.GetType().FullName);
                    throw new InvalidOperationException(error);
                }
            }
        }

        public static void ValidateIsMessageType(params object[] messages)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            if (Settings.IsMessageType == null)
            {
                throw new InvalidOperationException("No rules have been configured for reply message types. Use the DefineMessageAs function during endpoint configuration to configure this rule.");
            }

            foreach (var message in messages)
            {
                if (!Settings.IsMessageType(message.GetType()))
                {
                    var error = String.Format("Reply is reserved for messages that have been defined as normal messages using the DefineMessageAs" +
                        " function during endpoing configuration. Message {0} does not comply with the current rule.", message.GetType().FullName);
                    throw new InvalidOperationException(error);
                }
            }
        }
    }
}