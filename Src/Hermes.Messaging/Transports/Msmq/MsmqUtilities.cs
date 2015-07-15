using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hermes.Logging;

namespace Hermes.Messaging.Transports.Msmq
{
    public class MsmqUtilities
    {
        /// <summary>
        ///     Turns a '@' separated value into a full path.
        ///     Format is 'queue@machine', or 'queue@ipaddress'
        /// </summary>
        public static string GetFullPath(Address value)
        {
            IPAddress ipAddress;

            if (IPAddress.TryParse(value.Machine, out ipAddress))
            {
                return PrefixTcp + MsmqQueueCreator.GetFullPathWithoutPrefix(value);
            }

            return Prefix + MsmqQueueCreator.GetFullPathWithoutPrefix(value);
        }

        /// <summary>
        ///     Gets the name of the return address from the provided value.
        ///     If the target includes a machine name, uses the local machine name in the returned value
        ///     otherwise uses the local IP address in the returned value.
        /// </summary>
        public static string GetReturnAddress(string value, string target)
        {
            return GetReturnAddress(Address.Parse(value), Address.Parse(target));
        }

        /// <summary>
        ///     Gets the name of the return address from the provided value.
        ///     If the target includes a machine name, uses the local machine name in the returned value
        ///     otherwise uses the local IP address in the returned value.
        /// </summary>
        public static string GetReturnAddress(Address value, Address target)
        {
            var machine = target.Machine;

            IPAddress targetIpAddress;

            //see if the target is an IP address, if so, get our own local ip address
            if (IPAddress.TryParse(machine, out targetIpAddress))
            {
                if (string.IsNullOrEmpty(localIp))
                {
                    localIp = LocalIpAddress(targetIpAddress);
                }

                return PrefixTcp + localIp + Private + value.Queue;
            }

            return Prefix + MsmqQueueCreator.GetFullPathWithoutPrefix(value);
        }

        static string LocalIpAddress(IPAddress targetIpAddress)
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            var availableAddresses =
                networkInterfaces.Where(
                    ni =>
                        ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses).ToList();

            var firstWithMatchingFamily =
                availableAddresses.FirstOrDefault(a => a.Address.AddressFamily == targetIpAddress.AddressFamily);

            if (firstWithMatchingFamily != null)
            {
                return firstWithMatchingFamily.Address.ToString();
            }

            var fallbackToDifferentFamily = availableAddresses.FirstOrDefault();

            if (fallbackToDifferentFamily != null)
            {
                return fallbackToDifferentFamily.Address.ToString();
            }

            return "127.0.0.1";
        }


        static Address GetIndependentAddressForQueue(MessageQueue q)
        {
            if (q == null)
            {
                return null;
            }

            var arr = q.FormatName.Split('\\');
            var queueName = arr[arr.Length - 1];

            var directPrefixIndex = arr[0].IndexOf(Directprefix);
            if (directPrefixIndex >= 0)
            {
                return new Address(queueName, arr[0].Substring(directPrefixIndex + Directprefix.Length));
            }

            var tcpPrefixIndex = arr[0].IndexOf(DirectprefixTcp);
            if (tcpPrefixIndex >= 0)
            {
                return new Address(queueName, arr[0].Substring(tcpPrefixIndex + DirectprefixTcp.Length));
            }

            try
            {
                // the pessimistic approach failed, try the optimistic approach
                arr = q.QueueName.Split('\\');
                queueName = arr[arr.Length - 1];
                return new Address(queueName, q.MachineName);
            }
            catch
            {
                throw new Exception("Could not translate format name to independent name: " + q.FormatName);
            }
        }

        /// <summary>
        ///     Converts an MSMQ message to a TransportMessage.
        /// </summary>
        public static TransportMessage Convert(Message m)
        {
            Guid messageId = GetGuidId(m.Id);
            Guid correlationId = GetGuidId(m.CorrelationId);

            Dictionary<string, string> headers = DeserializeMessageHeaders(m);
            m.BodyStream.Position = 0;
            byte[] body = new byte[m.BodyStream.Length];
            m.BodyStream.Read(body, 0, body.Length);

            return new TransportMessage(messageId, correlationId, Address.Local, m.TimeToBeReceived, headers, body);
        }

        static Guid GetGuidId(string id)
        {
            if (String.IsNullOrWhiteSpace(id) || id == "00000000-0000-0000-0000-000000000000\\0")
            {
                return Guid.Empty;
            }

            string truncated = id.Substring(0, GuidStringLength);

            return Guid.Parse(truncated);
        }

        static Dictionary<string, string> DeserializeMessageHeaders(Message m)
        {
            var result = new Dictionary<string, string>();

            if (m.Extension.Length == 0)
            {
                return result;
            }

            //This is to make us compatible with v3 messages that are affected by this bug:
            //http://stackoverflow.com/questions/3779690/xml-serialization-appending-the-0-backslash-0-or-null-character
            var extension = Encoding.UTF8.GetString(m.Extension).TrimEnd('\0');
            object o;
            using (var stream = new StringReader(extension))
            {
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings
                {
                    CheckCharacters = false
                }))
                {
                    o = HeaderSerializer.Deserialize(reader);
                }
            }

            foreach (var pair in (List<HeaderValue>)o)
            {
                if (pair.Key != null)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        /// <summary>
        ///     Converts a TransportMessage to an Msmq message.
        ///     Doesn't set the ResponseQueue of the result.
        /// </summary>
        public static Message Convert(TransportMessage message)
        {
            var result = new Message();

            if (message.Body != null)
            {
                result.BodyStream = new MemoryStream(message.Body);
            }

            AssignMsmqNativeCorrelationId(message, result);

            result.Recoverable = true;

            using (var stream = new MemoryStream())
            {
                HeaderSerializer.Serialize(stream, message.Headers.Select(pair => new HeaderValue(pair.Key, pair.Value)).ToList());
                result.Extension = stream.ToArray();
            }

            return result;
        }

        private static void AssignMsmqNativeCorrelationId(TransportMessage message, Message result)
        {
            if (Guid.Empty == message.CorrelationId)
            {
                return;
            }

            result.CorrelationId = message.CorrelationId + "\\0";
        }

        const string Directprefix = "DIRECT=OS:";
        const string DirectprefixTcp = "DIRECT=TCP:";
        const string PrefixTcp = "FormatName:" + DirectprefixTcp;
        const string Prefix = "FormatName:" + Directprefix;
        internal const string Private = "\\private$\\";
        static string localIp;
        static readonly XmlSerializer HeaderSerializer = new XmlSerializer(typeof(List<HeaderValue>));
        static readonly ILog Logger = LogFactory.Build<MsmqUtilities>();
        private static readonly int GuidStringLength = Guid.Empty.ToString().Length;
    }
}