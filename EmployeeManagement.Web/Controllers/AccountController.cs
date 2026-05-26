using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Web.ViewModels;

namespace EmployeeManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private const string AdminId = "admin";
        private const string AdminPassword = "admin123";

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("logged_in") == "true")
                return RedirectToAction("Index", "Employee");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.LoginId == AdminId && model.Password == AdminPassword)
            {
                HttpContext.Session.SetString("logged_in", "true");
                HttpContext.Session.SetString("login_user", model.LoginId);
                return RedirectToAction("Index", "Employee");
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
