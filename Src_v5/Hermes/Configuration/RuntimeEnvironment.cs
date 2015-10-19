using System;
using System.Web;

namespace Hermes.Configuration
{
    public static class RuntimeEnvironment
    {
        public static Func<string> GetCurrentUserName { get; set; }
        public static Func<string> GetMachineName { get; set; }

        static RuntimeEnvironment()
        {
            GetMachineName = () => Environment.MachineName;

            GetCurrentUserName = () =>
            {
                if (HttpContext.Current == null || HttpContext.Current.User == null || String.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                    return Environment.UserName;

                return HttpContext.Current.User.Identity.Name;
            };
        }
    }
}