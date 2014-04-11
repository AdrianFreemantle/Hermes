using System;
using Hermes.Logging;

namespace Hermes.Messaging.Configuration
{
    public static class CurrentUser
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (CurrentUser));

        public static Guid GetCurrentUserId()
        {
            try
            {
                if (Settings.UserIdResolver == null)
                {
                    return Guid.Empty;
                }

                return Settings.UserIdResolver();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetFullExceptionMessage());
            }

            return Guid.Empty;
        }
    }
}