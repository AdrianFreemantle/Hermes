using System;
using System.Threading;
using Hermes.Logging;

namespace Hermes
{
    public static class HermesSystemClock 
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (HermesSystemClock));

        private static readonly ThreadLocal<Func<DateTimeOffset>> CurrentResolver = new ThreadLocal<Func<DateTimeOffset>>();

        public static DateTimeOffset UtcNow { get { return ResolveCurrentUtcTime(); } }

        public static void SetUtcResolver(Func<DateTimeOffset> resolveUtcNow)
        {
            Mandate.ParameterNotNull(resolveUtcNow, "resolveUtcNow");

            CurrentResolver.Value = resolveUtcNow;
        }

        private static DateTimeOffset ResolveCurrentUtcTime()
        {
            try
            {
                var resolver = CurrentResolver.Value ?? (() => new DateTimeOffset());
                return resolver();
            }
            catch(Exception ex)
            {
                Logger.Error("Error resolving current UTC time : {0}", ex.GetFullExceptionMessage());
                return new DateTimeOffset();
            }
        }
    }
}