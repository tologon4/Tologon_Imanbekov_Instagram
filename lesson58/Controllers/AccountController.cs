using Microsoft.AspNetCore.Mvc;

namespace lesson58.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        return View();
    }
}