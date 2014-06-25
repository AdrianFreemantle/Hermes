using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes;
using Hermes.EntityFramework;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using ProcessManagement.ProcessManager;

namespace ProcessManagement.Persistence
{
    public class ProcessManagerContext : FrameworkContext, INeedToInitializeSomething, IDatabaseInitializer<ProcessManagerContext>
    {
        private static readonly string MutexKey;

        public IDbSet<SeatReservationProcessManagerState> SeatReservationProcessManagerState { get; set; }

        static ProcessManagerContext()
        {
            MutexKey = DeterministicGuid.Create(typeof(ProcessManagerContext).FullName).ToString();
        }

        public ProcessManagerContext()
        {
        }

        public ProcessManagerContext(string connectionStringName)
            : base(connectionStringName)
        {
        }

        public void Initialize()
        {
            using (new SingleGlobalInstance(30000, MutexKey))
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                Database.SetInitializer(this);
                var uow = scope.GetInstance<EntityFrameworkUnitOfWork>();
                Seed((ProcessManagerContext)uow.GetDbContext());
                uow.Commit();
            }
        }

        public void InitializeDatabase(ProcessManagerContext context)
        {
            if (context.Database.Exists() && !context.Database.CompatibleWithModel(false))
            {
                context.Database.Delete();
            }
            context.Database.CreateIfNotExists();
        }

        protected void Seed(ProcessManagerContext context)
        {
   
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
