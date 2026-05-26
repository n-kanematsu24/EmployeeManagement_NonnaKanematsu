using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentController(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        // 一覧
        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _repository.GetAllAsync());

        // 1件取得
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await _repository.GetByIdAsync(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        // 登録
        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            await _repository.AddAsync(department);
            return Ok(department);
        }

        // 更新
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Department department)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            existing.DeptName = department.DeptName;
            await _repository.UpdateAsync(existing);
            return Ok(existing);
        }

        // 削除（論理削除）
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _repository.GetByIdAsync(id);
            if (dept == null) return NotFound();
            await _repository.DeleteAsync(id);
            return Ok();
        }
    }
}