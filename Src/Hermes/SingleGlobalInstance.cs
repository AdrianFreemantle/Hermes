using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Hermes
{
    public class SingleGlobalInstance : IDisposable
    {
        private bool hasHandle = false;
        Mutex mutex;

        public bool HasHandle
        {
            get { return hasHandle; }
        }

        private void InitMutex(string mutexId)
        {
            mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            mutex.SetAccessControl(securitySettings);
        }

        private static string GetDefaultMutexId()
        {
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            return mutexId;
        }

        public SingleGlobalInstance(int timeout)
            : this(timeout, GetDefaultMutexId())
        {
        }

        public SingleGlobalInstance(int timeout, string mutexId)
        {
            Mandate.ParameterNotNullOrEmpty(mutexId, "mutexId");

            InitMutex(mutexId);

            GetHandle(timeout);
        }

        private void GetHandle(int timeout)
        {
            try
            {
                if (timeout <= 0)
                {
                    hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                }
                else
                {
                    hasHandle = mutex.WaitOne(timeout, false);
                }

                if (hasHandle == false)
                {
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
                }
            }
            catch (AbandonedMutexException)
            {
                hasHandle = true;
            }
        }

        public void Dispose()
        {
            if (mutex != null)
            {
                if (hasHandle)
                    mutex.ReleaseMutex();

                mutex.Dispose();
            }
        }
    }
}
