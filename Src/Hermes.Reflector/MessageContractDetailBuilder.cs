using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hermes.Equality;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.Configuration.MessageHandlerCache;
using Hermes.Reflection;

namespace Hermes.Reflector
{
    public class MessageContractDetailBuilder
    {
        private readonly List<MessageContractDetail> messageDetails = new List<MessageContractDetail>();

        public MessageContractDetailBuilder()
        {
            foreach (Type handledType in HandlerCache.GetAllHandledMessageContracts())
            {
                HandlerCacheItem[] handlers = HandlerCache.GetHandlers(new[] { handledType });

                foreach (var handler in handlers)
                {
                    MessageContractDetail[] matchingMessasgeContractDetail = messageDetails
                        .Where(d => d.TypeMatchesMessageContract(handledType))
                        .ToArray();

                    if (matchingMessasgeContractDetail.Any())
                    {
                        foreach (var messageContractDetail in matchingMessasgeContractDetail)
                        {
                            messageContractDetail.AddHandledType(handledType, handler);
                        }
                    }
                    else
                    {
                        messageDetails.Add(new MessageContractDetail(handledType, handler));
                    }
                }
            }

            var typesUsingBus = GetTypesUsingBus();

            foreach (Type busClassType in typesUsingBus)
            {
                MethodInfo[] methods = busClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Console.WriteLine(busClassType.FullName);

                foreach (var methodInfo in methods)
                {
                    foreach (ILInstruction instruciton in ILInstructionLoader.GetInstructions(methodInfo).Where(i => i.OpCode.Name == "call" || i.OpCode.Name == "callvirt"))
                    {
                        if (instruciton.IsMethodCall)
                        {
                            var mc = (MethodInfo)instruciton.Data;

                            if (mc.DeclaringType.FullName == typeof(IMessageBus).FullName || mc.DeclaringType.FullName == typeof(IInMemoryBus).FullName)
                            {
                                WriteMessageLog(messageDetails, busClassType, mc, methodInfo);
                            }
                            else if (mc.DeclaringType.FullName.StartsWith("Hermes.Messaging.ProcessManagement.ProcessManager"))
                            {
                                if (mc.Name == "Publish" || mc.Name == "Send" || mc.Name == "ReplyToOriginator" || mc.Name == "Timeout")
                                {
                                    WriteMessageLog(messageDetails, busClassType, mc, methodInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteMessageLog(List<MessageContractDetail> messageDetails, Type busClassType, MethodInfo busMethodInfo, MethodInfo methodInfo)
        {
            ParameterInfo mParam = GetMessageParameter(busMethodInfo);

            if (mParam != null)
            {
                var matchingMessasgeContractDetail = messageDetails
                    .Where(d => d.TypeMatchesMessageContract(mParam.ParameterType))
                    .ToArray();

                if (matchingMessasgeContractDetail.Any())
                {
                    foreach (var messageDetail in matchingMessasgeContractDetail)
                    {
                        messageDetail.AddOriginator(new MessageOriginator(busClassType, methodInfo, busMethodInfo, mParam.ParameterType));
                    }
                }
                else
                {
                    MessageContractDetail md = new MessageContractDetail(new MessageOriginator(busClassType, methodInfo, busMethodInfo, mParam.ParameterType));
                    messageDetails.Add(md);
                }
            }
        }

        private ParameterInfo GetMessageParameter(MethodInfo mc)
        {
            return mc.GetParameters().FirstOrDefault(p => Settings.IsCommandType(p.ParameterType) || Settings.IsEventType(p.ParameterType));
        }

        private static ICollection<Type> GetTypesUsingBus()
        {
            var a = AssemblyScanner.Types.Where(t => !t.AssemblyQualifiedName.StartsWith("Hermes.Messaging") && t.IsClass && !t.IsAbstract && t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Any(p => p.PropertyType == typeof(IMessageBus) || p.PropertyType == typeof(IInMemoryBus))).ToArray();
            var b = AssemblyScanner.Types.Where(t => !t.AssemblyQualifiedName.StartsWith("Hermes.Messaging") && t.IsClass && !t.IsAbstract && t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Any(p => p.FieldType == typeof(IMessageBus) || p.FieldType == typeof(IInMemoryBus))).ToArray();

            return a.Union(b).Distinct(new TypeEqualityComparer()).ToArray();
        }

        public void WriteDetails(IContractWriter writer)
        {
            foreach (var messageContractDetail in messageDetails)
            {
                messageContractDetail.WriteDetails(writer);
            }
        }
    }
}