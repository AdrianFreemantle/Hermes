using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class FetchEmployeesWithNameLike : IReturn<PagedResult<EmployeeDto>>
    {
        public string Name { get; set; }
    }
}