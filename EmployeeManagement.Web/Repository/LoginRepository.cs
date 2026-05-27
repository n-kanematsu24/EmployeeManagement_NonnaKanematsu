using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Web.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly AppDbContext _context;

        public LoginRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Login?> FindByEmployeeNoAsync(string employeeNo)
            => await _context.Logins
                .FirstOrDefaultAsync(l => l.EmployeeNo == employeeNo && l.IsDeleted != true);
    }
}
