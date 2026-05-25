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

        // 1件取得
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound();
            return Ok(emp);
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

        // 更新
        [HttpPut("{id}")]
        public IActionResult Update(int id, Employee employee)
        {
            var existing = _context.Employees.Find(id);
            if (existing == null) return NotFound();

            existing.EmployeeNo = employee.EmployeeNo;
            existing.LastName = employee.LastName;
            existing.FirstName = employee.FirstName;
            existing.BirthDate = employee.BirthDate;
            existing.Phone = employee.Phone;
            existing.Email = employee.Email;
            existing.HireDate = employee.HireDate;
            existing.DeptId = employee.DeptId;
            existing.Status = employee.Status;
            existing.IsDeleted = employee.IsDeleted;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedId = employee.UpdatedId;
            _context.SaveChanges();
            return Ok(existing);
        }

        // 削除
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp == null) return NotFound();

            _context.Employees.Remove(emp);
            _context.SaveChanges();
            return Ok();
        }
    }
}