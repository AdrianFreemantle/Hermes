using System.Data.Entity;
using Hermes;

namespace EntityFrameworkTest.Model
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<EntityFrameworkTestContext>
    {
        protected override void Seed(EntityFrameworkTestContext context)
        {
            var acme = context.Companies.Add(new Company
            {
                Id = SequentialGuid.New(),
                Name = "Google"
            });

            var amazon = context.Companies.Add(new Company
            {
                Id = SequentialGuid.New(),
                Name = "Amazon"
            });

            context.Employees.Add(new Employee
            {
                Id = SequentialGuid.New(),
                Name = "Joe Smith",
                CompanyId = acme.Id,
            });

            context.Employees.Add(new Employee
            {
                Id = SequentialGuid.New(),
                Name = "Sally Smith",
                CompanyId = acme.Id,
            });

            context.Employees.Add(new Employee
            {
                Id = SequentialGuid.New(),
                Name = "Billy Bob",
                CompanyId = acme.Id,
            });

            context.Employees.Add(new Employee
            {
                Id = SequentialGuid.New(),
                Name = "Sandra Jones",
                CompanyId = amazon.Id,
            });
        }
    }
}