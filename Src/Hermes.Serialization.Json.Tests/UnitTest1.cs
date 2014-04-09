using System;
using System.Linq;
using System.Runtime.Serialization;
using Hermes.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Hermes.Messaging.Serialization;

using Shouldly;

namespace Hermes.Serialization.Json.Tests
{
    public interface IEvent
    {
    }

    public interface IVersion1 : IEvent
    {
        Guid MessageId { get; }
    }

    public interface IVersion2 : IVersion1
    {
        string Text { get; }
    }

    public interface IVersion3 : IVersion2
    {
        DateTime Sent { get; }
    }

    [DataContract]
    public class MyEventMessage : IVersion3
    {
        [DataMember]
        public DateTime Sent { get; private set; }

        [DataMember]
        public Guid MessageId { get; private set; }

        [DataMember]
        public string Text { get; private set; }

        protected MyEventMessage()
        {
        }

        public MyEventMessage(DateTime sent, Guid messageId, string text)
        {
            Sent = sent;
            MessageId = messageId;
            Text = text;
        }

        public override bool Equals(object obj)
        {
            var @event = obj as IEvent;
            var version1 = obj as IVersion1;
            var version2 = obj as IVersion2;
            var version3 = obj as IVersion3;

            if (@event == null || version1 == null || version2 == null || version3 == null)
                return false;

            return version1.MessageId == MessageId
                && version2.Text == Text
                && version3.Sent == Sent;
        }
    }

    [TestClass]
    public class UnitTest1
    {
        static JsonMessageSerializer messageSerializer;
        static ISerializeObjects objectSerializer;
        static MyEventMessage testMessage;
        static TypeMapper messageMapper;

        [ClassInitialize]
        public static void Startup(TestContext context)
        {
            messageMapper = new TypeMapper();
            messageSerializer = new JsonMessageSerializer(messageMapper);
            objectSerializer = new JsonObjectSerializer();

            testMessage = new MyEventMessage(DateTime.Now, Guid.NewGuid(), "Hello there");

            messageMapper.Initialize(testMessage.GetType().GetInterfaces());
        }

        [TestMethod]
        public void TestMethod4()
        {
            byte[] serialized = messageSerializer.Serialize(testMessage);
            object restored = messageSerializer.Deserialize(serialized, testMessage.GetType());
            testMessage.Equals(restored).ShouldBe(true);
        }

        [TestMethod]
        public void TestMethod5()
        {
            byte[] serialized = messageSerializer.Serialize(testMessage);
            object restored = messageSerializer.Deserialize(serialized, testMessage.GetType().GetInterfaces().First());
            testMessage.Equals(restored).ShouldBe(true);
        }

        [TestMethod]
        public void TestMethod6()
        {
            byte[] serialized = messageSerializer.Serialize(testMessage);
            object restored = messageSerializer.Deserialize(serialized, testMessage.GetType().GetInterfaces().First());
            testMessage.Equals(restored).ShouldBe(true);
        }
    }
}
