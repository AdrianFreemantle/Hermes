using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Equality;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Configuration.MessageHandlerCache;

namespace Hermes.Reflector
{
    public class MessageContractDetail
    {
        static readonly TypeEqualityComparer TypeComparer = new TypeEqualityComparer();

        private readonly HashSet<Type> contractTypes;
        private MessageType messageType;

        private readonly List<HandlerDetail> handlerDetails;
        private readonly Dictionary<string, List<MessageOriginator>> originators;

        public MessageContractDetail(MessageOriginator originator)
            : this()
        {
            SetMessageType(originator.MessageType);

            AddOriginator(originator);
        }

        public void AddOriginator(MessageOriginator originator)
        {
            if (!originators.ContainsKey(originator.OriginatorType.FullName))
            {
                originators[originator.OriginatorType.FullName] = new List<MessageOriginator>();
            }

            originators[originator.OriginatorType.FullName].Add(originator);

            Type[] contracts = originator.MessageType.GetContracts();
            AddContracts(contracts);
        }

        public MessageContractDetail(Type handledType, HandlerCacheItem handlerCacheItem)
            : this()
        {
            AddHandledType(handledType, handlerCacheItem);
        }

        public void AddHandledType(Type handledType, HandlerCacheItem handlerCacheItem)
        {
            HandlerDetail handlerDetail = handlerDetails.FirstOrDefault(h => h.HandlerType == handlerCacheItem.HandlerType);

            if (handlerDetail == null)
            {
                handlerDetail = new HandlerDetail(handlerCacheItem.HandlerType, handlerCacheItem.GetHandledMessageContracts());
                handlerDetails.Add(handlerDetail);
            }

            SetMessageType(handledType);
            AddContracts(handledType.GetContracts());
        }

        public bool TypeMatchesMessageContract(Type type)
        {
            return contractTypes.Any(p => p.IsAssignableFrom(type) || type.IsAssignableFrom(p));
        }

        private void AddContracts(ICollection<Type> handledContractTypes)
        {
            foreach (var handledType in handledContractTypes)
            {
                if (contractTypes.Contains(handledType, TypeComparer) || handledType.Name == "IEvent" || handledType.Name == "IDomainEvent")
                {
                    continue;
                }

                contractTypes.Add(handledType);
            }
        }

        private MessageContractDetail()
        {
            contractTypes = new HashSet<Type>();
            handlerDetails = new List<HandlerDetail>();
            originators = new Dictionary<string, List<MessageOriginator>>();
        }

        private void SetMessageType(Type contractType)
        {
            if (messageType == MessageType.Unknown)
            {
                if (Settings.IsEventType(contractType))
                {
                    messageType = MessageType.Event;
                }
                else if (Settings.IsCommandType(contractType))
                {
                    messageType = MessageType.Command;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unable to determine message type for {0}", contractType.FullName));
                }
            }
        }

        public void WriteDetails(IContractWriter writer)
        {
            MessageOriginator[] o = originators.Values.SelectMany(x => x).ToArray();

            writer.WriteDetails(messageType, contractTypes, handlerDetails, o);
        }
    }
}