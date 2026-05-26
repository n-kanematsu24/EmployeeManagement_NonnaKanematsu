using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.Models;
using EmployeeManagement.Web.ViewModels;

namespace EmployeeManagement.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _deptRepo;
        private readonly IEmployeeRepository _empRepo;
        private readonly IWebHostEnvironment _env;

        public DepartmentController(IDepartmentRepository deptRepo, IEmployeeRepository empRepo, IWebHostEnvironment env)
        {
            _deptRepo = deptRepo;
            _empRepo = empRepo;
            _env = env;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetString("logged_in") == "true";
        private IActionResult RequireLogin() => RedirectToAction("Login", "Account");

        private async Task<string?> SaveDeptImage(IFormFile? file, int deptId)
        {
            if (file == null || file.Length == 0) return null;
            var imgDir = Path.Combine(_env.WebRootPath, "img", "departments");
            Directory.CreateDirectory(imgDir);
            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"dept_{deptId}{ext}";
            var filePath = Path.Combine(imgDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/img/departments/{fileName}";
        }

        private string? GetDeptImagePath(int deptId)
        {
            var imgDir = Path.Combine(_env.WebRootPath, "img", "departments");
            foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" })
            {
                var path = Path.Combine(imgDir, $"dept_{deptId}{ext}");
                if (System.IO.File.Exists(path)) return $"/img/departments/dept_{deptId}{ext}";
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RequireLogin();
            var depts = await _deptRepo.GetAllAsync();
            var vms = depts.Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                DeptName = d.DeptName ?? string.Empty,
                Phone = d.Phone,
                ImagePath = GetDeptImagePath(d.Id)
            }).ToList();
            return View(vms);
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();
            var allEmps = await _empRepo.GetAllAsync();
            var members = allEmps.Where(e => e.DeptId == id).Select(e => new EmployeeViewModel
            {
                Id = e.Id, EmployeeNo = e.EmployeeNo,
                LastName = e.LastName ?? string.Empty, FirstName = e.FirstName ?? string.Empty,
                Email = e.Email ?? string.Empty, Phone = e.Phone ?? string.Empty,
                Status = e.Status ?? 0, DeptId = e.DeptId ?? 0
            }).ToList();
            var vm = new DepartmentViewModel
            {
                Id = dept.Id, DeptName = dept.DeptName ?? string.Empty,
                Phone = dept.Phone, Members = members,
                ImagePath = GetDeptImagePath(dept.Id)
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RequireLogin();
            return View(new DepartmentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ModelState.Remove("ProfileImageFile");
            if (!ModelState.IsValid) return View(model);
            var dept = new Department { DeptName = model.DeptName, Phone = model.Phone, IsDeleted = false };
            await _deptRepo.AddAsync(dept);
            if (model.ProfileImageFile != null) await SaveDeptImage(model.ProfileImageFile, dept.Id);
            TempData["Success"] = $"「{model.DeptName}」を登録しました";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();
            var vm = new DepartmentViewModel
            {
                Id = dept.Id, DeptName = dept.DeptName ?? string.Empty,
                Phone = dept.Phone, ImagePath = GetDeptImagePath(dept.Id)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ModelState.Remove("ProfileImageFile");
            if (!ModelState.IsValid) return View(model);
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();
            dept.DeptName = model.DeptName;
            dept.Phone = model.Phone;
            await _deptRepo.UpdateAsync(dept);
            if (model.ProfileImageFile != null) await SaveDeptImage(model.ProfileImageFile, id);
            TempData["Success"] = $"「{model.DeptName}」を更新しました";
            return RedirectToAction("Detail", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn()) return RequireLogin();
            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();
            await _deptRepo.DeleteAsync(id);
            TempData["Success"] = $"「{dept.DeptName}」を削除しました";
            return RedirectToAction("Index");
        }
    }
}
