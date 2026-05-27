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

        private bool IsLoggedIn() => HttpContext.Session.GetString("logged_in") == "true";
        private IActionResult RequireLogin() => RedirectToAction("Login", "Account");

        private async Task<SelectList> GetDeptSelectList(int? selectedId = null)
        {
            var depts = await _deptRepo.GetAllAsync();
            return new SelectList(depts, "Id", "DeptName", selectedId);
        }

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

        private string? GetProfileImagePath(int empId)
        {
            var imgDir = Path.Combine(_env.WebRootPath, "img", "employees");
            foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" })
            {
                var path = Path.Combine(imgDir, $"emp_{empId}{ext}");
                if (System.IO.File.Exists(path)) return $"/img/employees/emp_{empId}{ext}";
            }
            return null;
        }

        private EmployeeViewModel ToViewModel(Employee e) => new()
        {
            Id = e.Id,
            EmployeeNo = e.EmployeeNo,
            LastName = e.LastName ?? string.Empty,
            FirstName = e.FirstName ?? string.Empty,
            LastNameEn = e.LastNameEn,
            FirstNameEn = e.FirstNameEn,
            Email = e.Email ?? string.Empty,
            Phone = e.Phone ?? string.Empty,
            Status = e.Status ?? 0,
            DeptId = e.DeptId ?? 0,
            DeptName = e.Department?.DeptName,
            DeptNameEn = e.Department?.DeptNameEn,
            HireDate = e.HireDate,
            BirthDate = e.BirthDate,
            ProfileImagePath = GetProfileImagePath(e.Id),
            UpdatedAt = e.UpdatedAt,
            UpdatedId = e.UpdatedId
        };

        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emps = await _empRepo.GetAllWithDepartmentAsync();
            return View(emps.Select(ToViewModel).ToList());
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var emp = await _empRepo.GetWithDepartmentAsync(id);
            if (emp == null) return NotFound();
            var vm = ToViewModel(emp);
            // 更新者名を解決
            if (vm.UpdatedId.HasValue)
            {
                var updater = await _empRepo.GetByIdAsync(vm.UpdatedId.Value);
                if (updater != null)
                {
                    vm.UpdatedByName = $"{updater.LastName} {updater.FirstName}".Trim();
                }
            }
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
                LastNameEn = model.LastNameEn,
                FirstNameEn = model.FirstNameEn,
                Email = model.Email,
                Phone = model.Phone,
                DeptId = model.DeptId,
                Status = model.Status,
                HireDate = model.HireDate,
                BirthDate = model.BirthDate,
                IsDeleted = false,
                UpdatedId = HttpContext.Session.GetInt32("login_emp_id")  // admin の場合は null
            };
            await _empRepo.AddAsync(emp);
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
            var vm = ToViewModel(emp);
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
            emp.LastNameEn = model.LastNameEn;
            emp.FirstNameEn = model.FirstNameEn;
            emp.Email = model.Email;
            emp.Phone = model.Phone;
            emp.DeptId = model.DeptId;
            emp.Status = model.Status;
            emp.HireDate = model.HireDate;
            emp.BirthDate = model.BirthDate;
            emp.UpdatedId = HttpContext.Session.GetInt32("login_emp_id");  // 更新者ID
            await _empRepo.UpdateAsync(emp);
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
