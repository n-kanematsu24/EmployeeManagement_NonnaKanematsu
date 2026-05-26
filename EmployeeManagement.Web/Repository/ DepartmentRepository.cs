using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Web.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
            => await _context.Departments
                .Where(d => d.IsDeleted != true)
                .ToListAsync();

        public async Task<Department?> GetByIdAsync(int id)
            => await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.IsDeleted != true);

        public async Task AddAsync(Department department)
        {
            department.CreatedAt = DateTime.Now;
            department.UpdatedAt = DateTime.Now;
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            department.UpdatedAt = DateTime.Now;
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dept = await GetByIdAsync(id);
            if (dept == null) return;
            dept.IsDeleted = true;
            dept.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}