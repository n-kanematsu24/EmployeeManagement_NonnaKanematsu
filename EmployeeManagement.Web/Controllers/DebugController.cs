using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Web.Controllers
{
    public class DebugController : Controller
    {
        public IActionResult Culture()
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            var cookie = Request.Cookies[".AspNetCore.Culture"];
            return Content($"CurrentUICulture: {culture}\nCookie: {cookie}");
        }
    }
}
