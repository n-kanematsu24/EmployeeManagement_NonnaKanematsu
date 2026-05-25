using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Employees.ToList());
        }

        [HttpPost]
        public IActionResult Post(Employee employee)
        {
            _context.Employees.Add(employee);

            _context.SaveChanges();

            return Ok();
        }
    }
}