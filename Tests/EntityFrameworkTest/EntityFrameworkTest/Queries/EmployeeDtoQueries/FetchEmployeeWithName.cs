using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class FetchEmployeeWithName: IReturn<EmployeeDto>
    {
        public string Name { get; set; }
    }
}