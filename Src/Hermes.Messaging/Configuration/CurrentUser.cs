using System;
using Hermes.Logging;

namespace Hermes.Messaging.Configuration
{
    public static class CurrentUser
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (CurrentUser));

        public static string GetCurrentUserName()
        {
            try
            {
                if (Settings.UserNameResolver == null)
                {
                    return string.Empty;
                }

                return Settings.UserNameResolver();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetFullExceptionMessage());
            }

            return String.Empty;
        }
    }
}