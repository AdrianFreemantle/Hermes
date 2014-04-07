using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainRuleException : Exception
    {
        private readonly string name = string.Empty;

        public string Name { get; protected set; }

        public DomainRuleException(string name, string message)
            : base( message)
        {
            Mandate.ParameterNotNullOrEmpty(name, "name");
            this.name = name;
        }
    }
}
