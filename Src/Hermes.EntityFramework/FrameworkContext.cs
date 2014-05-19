using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Hermes.Messaging.Configuration;
using Hermes.Persistence;

namespace Hermes.EntityFramework
{   
    public abstract class FrameworkContext : DbContext
    {
        protected FrameworkContext()
        {
        }

        protected FrameworkContext(string databaseName)
            : base(databaseName)
        {
        }

        public virtual int SaveLookupTableChanges(params Type[] lookupTypes)
        {
            SaveLookups(lookupTypes);
            UpdateEntityAuditData();
            return base.SaveChanges();
        }

        public override int SaveChanges()
        {
            ValidateIds();
            SaveLookups();
            UpdateEntityAuditData();

            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex);
            }
            catch (DbEntityValidationException ex)
            {
                throw new EntityValidationException(ex);
            }
        }

        protected virtual void SaveLookups(params Type[] lookupTypesToSave)
        {
            var currentLookupItems = ChangeTracker.Entries<ILookupTable>().ToList();

            foreach (var entry in currentLookupItems)
            {
                if (lookupTypesToSave != null && lookupTypesToSave.Any(type => type.IsInstanceOfType(entry.Entity)))
                {
                    continue;
                }

                entry.State = EntityState.Unchanged;
            }
        }

        protected virtual void UpdateEntityAuditData()
        {
            foreach (var entry in ChangeTracker.Entries<ITimestampPersistenceAudit>())
            {
                AdjustTimestamps(entry);
            }

            foreach (var entry in ChangeTracker.Entries<IUserNamePersistenceAudit>())
            {
                AdjustUsers(entry);
            }
        }

        protected virtual void AdjustUsers(DbEntityEntry<IUserNamePersistenceAudit> entity)
        {
            string userName;

            if (!CurrentUser.GetCurrentUserName(out userName))
                userName = "Error resolving user name";

            if (entity.State == EntityState.Added)
                entity.Entity.CreatedBy = entity.Entity.ModifiedBy = userName;

            if (entity.State == EntityState.Modified)
                entity.Entity.ModifiedBy = userName;
        }

        protected virtual void AdjustTimestamps(DbEntityEntry<ITimestampPersistenceAudit> entity)
        {
            var localTime = HermesSystemClock.UtcNow.LocalDateTime;

            if (entity.State == EntityState.Added)
                entity.Entity.CreatedTimestamp = entity.Entity.ModifiedTimestamp = localTime;

            if (entity.State == EntityState.Modified)
                entity.Entity.ModifiedTimestamp = localTime;
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
            if(entity.Entity is ISequentialGuidId && (entity.State == EntityState.Added || entity.State == EntityState.Modified))
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

            if (objectStateEntry.EntityKey.EntityKeyValues != null && objectStateEntry.EntityKey.EntityKeyValues.Any())
            {
                return objectStateEntry.EntityKey.EntityKeyValues[0].Value;
            }

            return null;
        }
    }
}