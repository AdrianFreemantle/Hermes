using System;
using System.IO;
using Hermes.Serialization.Json;
using Hermes.Serialization.Tests.TestMessages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Hermes.Serialization.Tests
{
    [TestClass]
    public class JsonMessageSerializerTests
    {
        private static MySimpleMessage simpleMessage;
        private static MyDataContractMessage dataContractMessage;
        private static JsonMessageSerializer messageSerializer;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            messageSerializer = new JsonMessageSerializer();
            dataContractMessage = new MyDataContractMessage(Guid.NewGuid(), DateTime.Now, "Hello World");

            simpleMessage = new MySimpleMessage
            {
                Id = Guid.NewGuid(),
                Sent = DateTime.Now,
                Text = "Hello World"
            };
        }

        [TestMethod]
        public void Can_serialize_and_deserialize_array_of_messages()
        {
            var serialized = new byte[0];
            var deserialized = new object[0];
            var messages = new object[] {simpleMessage, dataContractMessage};

            using (var stream = new MemoryStream())
            {
                messageSerializer.Serialize(messages, stream);
                serialized = stream.ToArray();
            }

            using (var stream = new MemoryStream(serialized))
            {
                deserialized = messageSerializer.Deserialize(stream);
            }

            deserialized[0].ShouldBe(simpleMessage);
            deserialized[1].ShouldBe(dataContractMessage);
        }     
    }
}
