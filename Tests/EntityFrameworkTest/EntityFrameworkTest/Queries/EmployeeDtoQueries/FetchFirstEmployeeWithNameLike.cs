using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class FetchFirstEmployeeWithNameLike : IReturn<EmployeeDto>
    {
        public string Name { get; set; }
    }
}