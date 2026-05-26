using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.ViewModels;

namespace EmployeeManagement.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _empRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(IEmployeeRepository empRepo, IDepartmentRepository deptRepo, IWebHostEnvironment env)
        {
            _empRepo = empRepo;
            _deptRepo = deptRepo;
            _env = env;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("logged_in") == "true";

        private IActionResult RequireLogin() =>
            RedirectToAction("Login", "Account");

        private async Task<SelectList> GetDeptSelectList(int? selectedId = null)
        {
            var depts = await _deptRepo.GetAllAsync();
            return new SelectList(depts, "Id", "DeptName", selectedId);
        }

        // 画像保存ヘルパー
        private async Task<string?> SaveProfileImage(IFormFile? file, int empId)
        {
            if (file == null || file.Length == 0) return null;

            var imgDir = Path.Combine(_env.WebRootPath, "img", "employees");
            Directory.CreateDirectory(imgDir);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"emp_{empId}{ext}";
            var filePath = Path.Combine(imgDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/img/employees/{fileName}";
        }

        // 画像パス取得ヘルパー
        private string? GetProfileImagePath(int empId)
        {
            var imgDir = Path.Combine(_env.WebRootPath, "img", "employees");
            foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" })
            {
                var path = Path.Combine(imgDir, $"emp_{empId}{ext}");
                if (System.IO.File.Exists(path))
                    return $"/img/employees/emp_{empId}{ext}";
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emps = await _empRepo.GetAllWithDepartmentAsync();
            var vms = emps.Select(e => new EmployeeViewModel
            {
                Id = e.Id,
                EmployeeNo = e.EmployeeNo,
                LastName = e.LastName ?? string.Empty,
                FirstName = e.FirstName ?? string.Empty,
                Email = e.Email ?? string.Empty,
                Phone = e.Phone ?? string.Empty,
                Status = e.Status ?? 0,
                DeptId = e.DeptId ?? 0,
                DeptName = e.Department?.DeptName,
                ProfileImagePath = GetProfileImagePath(e.Id)
            }).ToList();
            return View(vms);
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emp = await _empRepo.GetWithDepartmentAsync(id);
            if (emp == null) return NotFound();

            var vm = new EmployeeViewModel
            {
                Id = emp.Id,
                EmployeeNo = emp.EmployeeNo,
                LastName = emp.LastName ?? string.Empty,
                FirstName = emp.FirstName ?? string.Empty,
                Email = emp.Email ?? string.Empty,
                Phone = emp.Phone ?? string.Empty,
                Status = emp.Status ?? 0,
                DeptId = emp.DeptId ?? 0,
                DeptName = emp.Department?.DeptName,
                HireDate = emp.HireDate,
                BirthDate = emp.BirthDate ?? DateOnly.MinValue,
                ProfileImagePath = GetProfileImagePath(emp.Id)
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewBag.Departments = await GetDeptSelectList();
            return View(new EmployeeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!IsLoggedIn()) return RequireLogin();
            // 画像はバリデーション対象外
            ModelState.Remove("ProfileImageFile");

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await GetDeptSelectList(model.DeptId);
                return View(model);
            }

            var emp = new Employee
            {
                EmployeeNo = model.EmployeeNo,
                LastName = model.LastName,
                FirstName = model.FirstName,
                Email = model.Email,
                Phone = model.Phone,
                DeptId = model.DeptId,
                Status = model.Status,
                HireDate = model.HireDate,
                BirthDate = model.BirthDate ?? DateOnly.MinValue,
                IsDeleted = false
            };
            await _empRepo.AddAsync(emp);

            // 画像保存（登録後にIDが確定するので再保存）
            if (model.ProfileImageFile != null)
                await SaveProfileImage(model.ProfileImageFile, emp.Id);

            TempData["Success"] = $"「{model.FullName}」を登録しました";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emp = await _empRepo.GetByIdAsync(id);
            if (emp == null) return NotFound();

            var vm = new EmployeeViewModel
            {
                Id = emp.Id,
                EmployeeNo = emp.EmployeeNo,
                LastName = emp.LastName ?? string.Empty,
                FirstName = emp.FirstName ?? string.Empty,
                Email = emp.Email ?? string.Empty,
                Phone = emp.Phone ?? string.Empty,
                Status = emp.Status ?? 0,
                DeptId = emp.DeptId ?? 0,
                HireDate = emp.HireDate,
                BirthDate = emp.BirthDate ?? DateOnly.MinValue,
                ProfileImagePath = GetProfileImagePath(emp.Id)
            };
            ViewBag.Departments = await GetDeptSelectList(vm.DeptId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ModelState.Remove("ProfileImageFile");

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await GetDeptSelectList(model.DeptId);
                model.ProfileImagePath = GetProfileImagePath(id);
                return View(model);
            }

            var emp = await _empRepo.GetByIdAsync(id);
            if (emp == null) return NotFound();

            emp.LastName = model.LastName;
            emp.FirstName = model.FirstName;
            emp.Email = model.Email;
            emp.Phone = model.Phone;
            emp.DeptId = model.DeptId;
            emp.Status = model.Status;
            emp.HireDate = model.HireDate;
            emp.BirthDate = model.BirthDate;

            await _empRepo.UpdateAsync(emp);

            // 画像更新
            if (model.ProfileImageFile != null)
                await SaveProfileImage(model.ProfileImageFile, id);

            TempData["Success"] = $"「{model.FullName}」を更新しました";
            return RedirectToAction("Detail", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emp = await _empRepo.GetByIdAsync(id);
            if (emp == null) return NotFound();

            var name = $"{emp.LastName} {emp.FirstName}".Trim();
            await _empRepo.DeleteAsync(id);
            TempData["Success"] = $"「{name}」を削除しました";
            return RedirectToAction("Index");
        }
    }
}
