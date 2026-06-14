using Microsoft.AspNetCore.Mvc;

namespace project_web2.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsAdmin() => HttpContext.Session.GetString("Role") == "admin";
        protected bool IsCustomer() => HttpContext.Session.GetString("Role") == "customer";
        protected string? GetSessionUser() => HttpContext.Session.GetString("Name");

        protected IActionResult? RequireLogin() =>
            HttpContext.Session.GetString("Role") == null
                ? RedirectToAction("Login", "useraccounts", new { returnUrl = Request.Path.Value })
                : null;
    }
}
