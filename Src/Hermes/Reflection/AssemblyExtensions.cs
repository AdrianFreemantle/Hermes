using System;
using System.Reflection;

namespace Hermes.Reflection
{
    public static class AssemblyExtensions
    {
        public static string GetVersionFormattedName(this Assembly assembly)
        {
            var version = assembly.GetName().Version;
            return String.Format("{0}-{1}.{2}", assembly.GetName().Name, version.Major, version.Minor);
        }
    }
}