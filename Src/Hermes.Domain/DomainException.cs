using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainRuleException : Exception
    {
        public string Name { get; protected set; }

        public DomainRuleException(string name, string message)
            : base( message)
        {
            Mandate.ParameterNotNullOrEmpty(name, "name");
            Name = name;
        }
    }
}
