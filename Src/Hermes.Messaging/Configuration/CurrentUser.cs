using System;
using Hermes.Logging;

namespace Hermes.Messaging.Configuration
{
    public static class CurrentUser
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (CurrentUser));

        public static bool GetCurrentUserName(out string userName)
        {
            userName = String.Empty;

            try
            {
                if (Settings.UserNameResolver == null)
                {
                    Logger.Warn("Settings.UserNameResolver has not been configured.");
                    return false;
                }

                userName = Settings.UserNameResolver();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetFullExceptionMessage());
                return false;
            }
        }

        public static string GetCurrentUserName()
        {
            string userName;

            GetCurrentUserName(out userName);

            return userName;
        }
    }
}