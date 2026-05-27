using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Infrastructures.Repositories
{
    public interface ILoginRepository
    {
        Task<Login?> FindByEmployeeNoAsync(string employeeNo);
    }
}
