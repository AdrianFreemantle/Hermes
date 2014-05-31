using System.Data.Entity;
using System.Linq;
using Hermes;
using Hermes.EntityFramework;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace EntityFrameworkTest.Model
{
    public class EntityFrameworkTestContext : FrameworkContext, INeedToInitializeSomething
    {
        private static readonly string MutexKey;

        public IDbSet<Employee> Employees { get; set; }
        public IDbSet<Company> Companies { get; set; }

        static EntityFrameworkTestContext()
        {
            MutexKey = DeterministicGuid.Create(typeof(EntityFrameworkTestContext).FullName).ToString();
        }

        public EntityFrameworkTestContext()
        {
        }

        public EntityFrameworkTestContext(string connectionStringName)
            : base(connectionStringName)
        {
        }

        public void Initialize()
        {
            using (new SingleGlobalInstance(30000, MutexKey))
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                Database.SetInitializer(new DatabaseInitializer());
                var uow = scope.GetInstance<EntityFrameworkUnitOfWork>();
                uow.GetRepository<Employee>().Any();
            }
        }
    }
}
