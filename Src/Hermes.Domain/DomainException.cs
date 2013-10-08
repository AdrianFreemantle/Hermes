using System;

namespace Hermes.Domain
{
    public class DomainRuleException : Exception
    {
        public DomainRuleException(string message)
            :base(message)
        {      
        }
    }
}
