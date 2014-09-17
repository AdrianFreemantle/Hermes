using System;
using System.Collections.Generic;

namespace Hermes.Reflection
{
    public static class AssemblyScannerDefaultIgnoreRules
    {
        public static List<Func<string, bool>> Rules { get; private set; }

        static AssemblyScannerDefaultIgnoreRules()
        {
            Rules.AddRange(new Func<string, bool>[]
            {
                s => s.StartsWith("Microsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("ServiceStack.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("Newtonsoft.", StringComparison.CurrentCultureIgnoreCase),
                s => s.StartsWith("System.", StringComparison.CurrentCultureIgnoreCase), 
                s => s.StartsWith("Autofac.", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("Antlr3.Runtime.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("EntityFramework.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("WebGrease.dll", StringComparison.CurrentCultureIgnoreCase),
                s => s.Equals("gsdll32.dll", StringComparison.CurrentCultureIgnoreCase)
            });
        }
    }
}