using System;
using System.Threading;
using Hermes.Ioc;
using Hermes.Logging;

namespace Hermes
{
    public static class HermesSystemClock 
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (HermesSystemClock));

        private static readonly WebSafeThreadLocal<Func<DateTimeOffset>> CurrentResolver = new WebSafeThreadLocal<Func<DateTimeOffset>>();

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
                var resolver = CurrentResolver.Value ?? (() => DateTimeOffset.UtcNow);
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