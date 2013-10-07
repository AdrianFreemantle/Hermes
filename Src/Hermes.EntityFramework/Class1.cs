using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Hermes.EntityFramework
{
    public interface ICurrentUser
    {
        string UserName { get; }
        bool IsAuthenticated { get; }
    }

    internal class NullUser : ICurrentUser
    {
        public string UserName
        {
            get { return string.Empty; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }
    }

    public abstract class FrameworkContext : DbContext
    {
        public ICurrentUser CurrentUser { get; set; }

        protected FrameworkContext()
        {
            CurrentUser = new NullUser();
        }

        protected FrameworkContext(string databaseName)
            : base(databaseName)
        {
            CurrentUser = new NullUser();
        }

        protected FrameworkContext(string databaseName, ICurrentUser currentUser)
            : base(databaseName)
        {
            CurrentUser = currentUser;
        }

        public virtual int SaveLookupTableChanges(params Type[] lookupTypes)
        {
            OnlySaveLookupsOfType(lookupTypes);
            UpdateEntityAuditData();
            return base.SaveChanges();
        }

        public override int SaveChanges()
        {
            ValidateIds();
            OnlySaveLookupsOfType();
            UpdateEntityAuditData();

            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex);
            }
        }

        protected virtual void OnlySaveLookupsOfType(params Type[] lookupTypesToSave)
        {
            var currentLookupItems = ChangeTracker.Entries<ILookupTable>().ToList();

            foreach (var entry in currentLookupItems)
            {
                if (lookupTypesToSave.Any(type => type.IsInstanceOfType(entry.Entity)))
                {
                    continue;
                }

                entry.State = EntityState.Unchanged;
            }
        }

        protected virtual void UpdateEntityAuditData()
        {
            var changedEntities = ChangeTracker.Entries<IPersistanceAudit>().ToList();

            foreach (var entry in changedEntities)
            {
                AdjustTimestamps(entry);
                AdjustUsers(entry);
            }
        }

        protected virtual void AdjustUsers(DbEntityEntry<IPersistanceAudit> entity)
        {
            if (entity.State == EntityState.Added)
                entity.Entity.CreatedBy = entity.Entity.ModifiedBy = CurrentUser.UserName;

            if (entity.State == EntityState.Modified)
                entity.Entity.ModifiedBy = CurrentUser.UserName;
        }

        protected virtual void AdjustTimestamps(DbEntityEntry<IPersistanceAudit> entity)
        {
            if (entity.State == EntityState.Added)
                entity.Entity.CreatedTimestamp = entity.Entity.ModifiedTimestamp = DateTime.Now;

            if (entity.State == EntityState.Modified)
                entity.Entity.ModifiedTimestamp = DateTime.Now;
        }

        protected virtual void ValidateIds()
        {
            foreach (DbEntityEntry entity in ChangeTracker.Entries())
            {
                ValidateId(entity);
            }
        }

        protected virtual void ValidateId(DbEntityEntry entity)
        {
            if (entity.State == EntityState.Added || entity.State == EntityState.Modified)
            {
                object entityKey = GetPrimaryKeyValue(entity);

                if (entityKey is Guid && !SequentialGuid.IsSequentialGuid((Guid)entityKey))
                {
                    throw new DbEntityValidationException(string.Format("An entity of type {0} has a non sequential Guid ID", entity.Entity.GetType()));
                }
            }
        }

        private object GetPrimaryKeyValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            return objectStateEntry.EntityKey.EntityKeyValues[0].Value;
        }
    }

    public interface IPersistanceAudit
    {
        string ModifiedBy { get; set; }
        string CreatedBy { get; set; }
        DateTime ModifiedTimestamp { get; set; }
        DateTime CreatedTimestamp { get; set; }
    }

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
