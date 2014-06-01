using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkTest.Queries.ComanyDtoQueries
{
    public class CompanyDto
    {
        public string Name { get; set; }
        public int EmployeeCount  { get; set; }
        public EmployedPersonDto[] Employees { get; set; }
    }
}
