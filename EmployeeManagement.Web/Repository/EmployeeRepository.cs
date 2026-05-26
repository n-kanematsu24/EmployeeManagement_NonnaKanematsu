using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Web.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
            => await _context.Employees
                .Where(e => e.IsDeleted != true)
                .ToListAsync();

        public async Task<IEnumerable<Employee>> GetAllWithDepartmentAsync()
            => await _context.Employees
                .Where(e => e.IsDeleted != true)
                .Include(e => e.Department)
                .ToListAsync();

        public async Task<Employee?> GetByIdAsync(int id)
            => await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted != true);

        public async Task<Employee?> GetWithDepartmentAsync(int id)
            => await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted != true);

        public async Task AddAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.Now;
            employee.UpdatedAt = DateTime.Now;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            employee.UpdatedAt = DateTime.Now;
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await GetByIdAsync(id);
            if (emp == null) return;
            emp.IsDeleted = true;
            emp.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}