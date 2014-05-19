using System;
using Hermes.Logging;

namespace Hermes
{
    public interface IHermesSystemClock
    {
        DateTimeOffset UtcNow { get; }
    }

    public class HermesSystemClock : IHermesSystemClock
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (HermesSystemClock));
        protected Func<DateTimeOffset> ResolveUtcNow;

        public DateTimeOffset UtcNow { get; protected set; }

        public HermesSystemClock()
        {
            ResolveUtcNow = () => new DateTimeOffset();
        }

        public void SetUtcResolver(Func<DateTimeOffset> resolveUtcNow)
        {
            Mandate.ParameterNotNull(resolveUtcNow, "resolveUtcNow");

            ResolveUtcNow = resolveUtcNow;
        }

        protected virtual DateTimeOffset ResolveCurrentUtcTime()
        {
            try
            {
                return ResolveUtcNow();
            }
            catch(Exception ex)
            {
                Logger.Error("Error resolving current UTC time : {0}", ex.GetFullExceptionMessage());
                return new DateTimeOffset();
            }
        }
    }
}