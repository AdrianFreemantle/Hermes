using System;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shouldly;

namespace Hermes.Serialization.Json.Tests
{
    public interface IEventContract
    {
        Guid Id { get; set; }
    }

    public class EventMessage : IEventContract
    {
        public Guid Id { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        readonly JsonMessageSerializer serializer = new JsonMessageSerializer();

        [TestMethod]
        public void TestMethod1()
        {
            var message1 = new EventMessage { Id = Guid.NewGuid() };
            var message2 = new EventMessage { Id = Guid.NewGuid() };

            byte[] serialized = serializer.Serialize(new object[] {message1, message2});

            var text = Encoding.UTF8.GetString(serialized);

            object[] deserialized = serializer
                .Deserialize(serialized)
                .ToArray();

            //deserialized[0].Id.ShouldBe(message1.Id);
            //deserialized[1].Id.ShouldBe(message2.Id);
        }
    }
}
