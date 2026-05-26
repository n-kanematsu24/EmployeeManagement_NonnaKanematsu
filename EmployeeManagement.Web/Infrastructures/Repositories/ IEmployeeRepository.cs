using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Infrastructures.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<IEnumerable<Employee>> GetAllWithDepartmentAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetWithDepartmentAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
    }
}