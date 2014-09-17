using System;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging.Transports
{
    public static class MessageRuleValidation
    {
        private const string CommandRuleNotDefined = "No rules have been configured for command message types. Use the DefineCommandAs function during endpoint configuration to configure this rule.";
        private const string CommandRuleFailed = "Send is reserved for messages that have been defined as commands using the DefineCommandAs function during endpoing configuration. Message {0} does not comply with the current rule.";

        private const string EventRuleNotDefined = "No rules have been configured for event message types. Use the DefineEventAs function during endpoint configuration to configure this rule.";
        private const string EventRuleFailed = "Publish is reserved for messages that have been defined as events using the DefineEventAs function during endpoing configuration. Message {0} does not comply with the current rule.";

        private const string MessageRuleNotDefined = "No rules have been configured for reply message types. Use the DefineMessageAs function during endpoint configuration to configure this rule.";
        private const string MessageRuleFailed = "Reply is reserved for messages that have been defined as normal messages using the DefineMessageAs function during endpoing configuration. Message {0} does not comply with the current rule.";

        public static void ValidateCommand(object message)
        {
            ValidateMessage(message, Settings.IsCommandType, CommandRuleNotDefined, CommandRuleFailed);
        }        

        public static void ValidateEvent(object message)
        {
            ValidateMessage(message, Settings.IsEventType, EventRuleNotDefined, EventRuleFailed);
        }

        public static void ValidateMessage(object message)
        {
            ValidateMessage(message, Settings.IsMessageType, MessageRuleNotDefined, MessageRuleFailed);
        }

        private static void ValidateMessage(object message, Func<Type, bool> predicate, string notDefinedMessage, string ruleFailedMessage)
        {
            if (message == null)
                throw new InvalidOperationException("Cannot send a null message.");

            if (predicate == null)
                throw new InvalidOperationException(notDefinedMessage);

            if (predicate(message.GetType()))
                return;
            
            var error = String.Format(ruleFailedMessage, message.GetType().FullName);
            throw new InvalidOperationException(error);
        }
    }
}