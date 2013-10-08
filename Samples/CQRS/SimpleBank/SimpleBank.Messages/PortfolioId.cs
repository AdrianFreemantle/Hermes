using System;
using System.Globalization;
using System.Runtime.Serialization;

using Hermes.Domain;

namespace SimpleBank.Messages
{
    [Serializable]
    public class PortfolioId : Identity<string>
    {
        public PortfolioId(string id) : 
            base(id)
        {
        }

        public static PortfolioId GenerateId()
        {
            var ticks = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var id = ticks.Substring(ticks.Length - 7, 7);

            return new PortfolioId(id);
        }
    }
}