using System;

namespace Hermes.Domain
{
    [Serializable]
    public class DomainRuleException : Exception
    {
        public DomainRuleException(string message)
            :base(message)
        {      
        }
    }
}
