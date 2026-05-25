using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // 一覧
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Employees.ToList());
        }

        // JOIN付き一覧
        [HttpGet("with-department")]
        public async Task<IActionResult> GetWithDepartment()
        {
            var result = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            return Ok(result);
        }

        // 登録
        [HttpPost]
        public IActionResult Post(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return Ok();
        }
    }
}