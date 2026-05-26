using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;

namespace EmployeeManagement.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeController(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        // 一覧（部門情報含む）
        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _repository.GetAllWithDepartmentAsync());

        // 1件取得
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _repository.GetByIdAsync(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        // 部門情報JOIN付き1件取得
        [HttpGet("{id}/with-department")]
        public async Task<IActionResult> GetWithDepartment(int id)
        {
            var emp = await _repository.GetWithDepartmentAsync(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        // 登録
        [HttpPost]
        public async Task<IActionResult> Post(Employee employee)
        {
            await _repository.AddAsync(employee);
            return Ok(employee);
        }

        // 更新
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee employee)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.EmployeeNo = employee.EmployeeNo;
            existing.LastName   = employee.LastName;
            existing.FirstName  = employee.FirstName;
            existing.BirthDate  = employee.BirthDate;
            existing.Phone      = employee.Phone;
            existing.Email      = employee.Email;
            existing.HireDate   = employee.HireDate;
            existing.DeptId     = employee.DeptId;
            existing.Status     = employee.Status;
            existing.IsDeleted  = employee.IsDeleted;
            existing.UpdatedAt  = DateTime.Now;
            existing.UpdatedId  = employee.UpdatedId;

            await _repository.UpdateAsync(existing);
            return Ok(existing);
        }

        // 削除（論理削除）
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _repository.GetByIdAsync(id);
            if (emp == null) return NotFound();
            await _repository.DeleteAsync(id);
            return Ok();
        }
    }
}