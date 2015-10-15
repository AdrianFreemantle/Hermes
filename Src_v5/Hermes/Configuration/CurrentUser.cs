using System;
using System.Web;
using Hermes.Logging;

namespace Hermes.Configuration
{
    public static class CurrentUser
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(CurrentUser));
        public static Func<string> UserNameResolver { get; set; }

        static CurrentUser()
        {
            UserNameResolver = () =>
            {
                if (HttpContext.Current == null || HttpContext.Current.User == null || String.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                    return Environment.UserName;

                return HttpContext.Current.User.Identity.Name;
            };
        }

        public static bool GetCurrentUserName(out string userName)
        {
            userName = String.Empty;

            try
            {
                if (UserNameResolver == null)
                {
                    Logger.Warn("Settings.UserNameResolver has not been configured.");
                    return false;
                }

                userName = UserNameResolver();

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
