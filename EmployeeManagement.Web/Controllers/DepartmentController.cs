using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.Data;
using EmployeeManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        // 診断用
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            var conn = _context.Database.GetDbConnection();
            return Ok(new {
                database = conn.Database,
                dataSource = conn.DataSource,
                state = conn.State.ToString()
            });
        }

        // 一覧
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Departments.ToList());
        }

        // 1件取得
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        // 登録
        [HttpPost]
        public IActionResult Create(Department department)
        {
            _context.Departments.Add(department);
            _context.SaveChanges();
            return Ok(department);
        }

        // 更新
        [HttpPut("{id}")]
        public IActionResult Update(int id, Department department)
        {
            var existing = _context.Departments.Find(id);
            if (existing == null) return NotFound();

            existing.DeptName = department.DeptName;
            _context.SaveChanges();
            return Ok(existing);
        }

        // 削除
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            _context.Departments.Remove(dept);
            _context.SaveChanges();
            return Ok();
        }
    }
}