using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Login> Logins => Set<Login>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("employee");
            modelBuilder.Entity<Department>().ToTable("department");
            modelBuilder.Entity<Login>().ToTable("login");
        }
    }
}
