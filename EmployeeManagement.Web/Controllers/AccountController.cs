using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.Infrastructures.Repositories;
using EmployeeManagement.Web.ViewModels;

namespace EmployeeManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        // adminフォールバック用ハードコード認証
        private const string AdminId = "admin";
        private const string AdminPassword = "admin123";

        private readonly ILoginRepository? _loginRepo;
        private readonly IEmployeeRepository? _empRepo;

        // コンストラクタは1つだけ。オプショナル引数でテストでも使える形に
        public AccountController(ILoginRepository? loginRepo = null, IEmployeeRepository? empRepo = null)
        {
            _loginRepo = loginRepo;
            _empRepo = empRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("logged_in") == "true")
                return RedirectToAction("Index", "Employee");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // ① admin フォールバック認証
            if (model.LoginId == AdminId && model.Password == AdminPassword)
            {
                HttpContext.Session.SetString("logged_in", "true");
                HttpContext.Session.SetString("login_user", "admin");
                // admin の場合は login_emp_id を設定しない (null扱い)
                return RedirectToAction("Index", "Employee");
            }

            // ② DB認証 (login テーブル)
            if (_loginRepo != null && _empRepo != null)
            {
                var loginRecord = await _loginRepo.FindByEmployeeNoAsync(model.LoginId);
                if (loginRecord != null && loginRecord.Password == model.Password)
                {
                    // 対応する社員レコードを取得して社員IDを取得
                    var allEmps = await _empRepo.GetAllAsync();
                    var emp = allEmps.FirstOrDefault(e => e.EmployeeNo == model.LoginId);
                    if (emp != null)
                    {
                        HttpContext.Session.SetString("logged_in", "true");
                        HttpContext.Session.SetString("login_user", model.LoginId);
                        HttpContext.Session.SetInt32("login_emp_id", emp.Id);
                        return RedirectToAction("Index", "Employee");
                    }
                }
            }

            ModelState.AddModelError("", "IDまたはパスワードが正しくありません");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
