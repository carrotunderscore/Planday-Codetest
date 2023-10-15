using System.Threading.Tasks;

namespace Planday.Schedule.Clients
{
	public interface IEmployeeApiClient
	{
		Task<Employee> GetEmployeeById(long id);
	}
}
