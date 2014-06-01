using Hermes.Queries;

namespace EntityFrameworkTest.Queries.EmployeeDtoQueries
{
    public class FetchAllEmployeesForCompany : IReturn<EmployeeDto[]>
    {
        public string CompanyName { get; set; }
    }
}