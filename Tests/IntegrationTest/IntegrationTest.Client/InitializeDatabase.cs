using System;
using System.Data.Entity;
using System.Linq;
using Hermes.EntityFramework;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using IntegrationTests.PersistenceModel;

namespace IntegrationTest.Client
{
    public class InitializeDatabase : INeedToInitializeSomething
    {
        public void Initialize()
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                Database.SetInitializer(new DatabaseInitializer());
                var repositoryFactory = scope.GetInstance<IRepositoryFactory>();
                bool result = repositoryFactory.GetRepository<Record>().Any(); //database gets created with first opperation against it

                if (result)
                {
                    throw new Exception("Expected an empty database");
                }
            }
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<IntegrationTestContext>
    {
    }

}