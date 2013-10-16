using System;

namespace Hermes.Domain
{
    public class NullIdentity : IIdentity
    {
        private const string NullId = "Null ID";

        public dynamic GetId()
        {
            return string.Empty;
        }

        public bool IsEmpty()
        {
            return true;
        }

        public string GetTag()
        {
            return String.Empty;
        }

        public override string ToString()
        {
            return NullId;
        }
    }
}