using Hermes.Messaging;

namespace Hermes.Gateway.TestServer
{
    public class TestCommand : IDomainCommand
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }
}