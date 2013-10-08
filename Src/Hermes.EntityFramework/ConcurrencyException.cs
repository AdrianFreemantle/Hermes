using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Hermes.EntityFramework
{
    [Serializable]
    public class ConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ConcurrencyException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="concurrencyException"></param>
        public ConcurrencyException(string message, DbUpdateConcurrencyException concurrencyException)
            : base(message, concurrencyException)
        {
        }

        public ConcurrencyException(DbUpdateConcurrencyException concurrencyException)
            : base(GetMessage(concurrencyException), concurrencyException)
        {
        }

        static string GetMessage(DbUpdateConcurrencyException concurrencyException)
        {
            List<DbEntityEntry> entries = concurrencyException.Entries.ToList();
            string entities = String.Join(", ", entries.ConvertAll(GetEntityName));
            return "A concurrency exception occured in the following entities : " + entities;
        }

        private static string GetEntityName(DbEntityEntry input)
        {
            var type = input.Entity.GetType();

            if (type.FullName.StartsWith("System.Data.Entity.DynamicProxies") && type.BaseType != null)
                return type.BaseType.Name;

            return type.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the ConcurrencyException class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The message that is the cause of the current exception.</param>
        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
