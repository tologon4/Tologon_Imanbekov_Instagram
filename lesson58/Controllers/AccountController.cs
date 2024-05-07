using lesson58.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace lesson58.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IHostEnvironment _environment;

    public IActionResult Home()
    {
        return View();
    }
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IHostEnvironment environment)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
    }
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel(){ReturnUrl = returnUrl});
    }


    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.Email);
            SignInResult result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                return RedirectToAction("Home");
            }
        }
        ModelState.AddModelError("", "Вы неправильно заполнили форму!");
        return View(model);
    }

    public IActionResult Register()
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        if (ModelState.IsValid)
        {
            User user = new User
            {
                Email = model.Email,
                UserName = model.UserName,
                Login = model.Login,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                Avatar = "../images/Default_pfp.svg.png"
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Home");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        ModelState.AddModelError("", "Something went wrong! Please check all info");
        return View(model);
    }
}