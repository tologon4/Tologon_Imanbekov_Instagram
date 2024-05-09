using System.Net;
using lesson58.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace lesson58.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private InstagramDb _db;
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment, InstagramDb db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
        _db = db;
    }
    
    public IActionResult Profile(int? id)
    {
        User? user = _db.Users.Include(p => p.Posts).FirstOrDefault(u => u.Id == id);
        return View(user);
    }
    
    public IActionResult Home()
    {
        return View(_db.Users.Where(u=> u.Id != int.Parse(_userManager.GetUserId(User))).ToList());
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
            User? user = await _userManager.FindByEmailAsync(model.LoginValue);
            if (user == null)
                user = await _userManager.FindByNameAsync(model.LoginValue);
            if (user != null)
            {
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
            ModelState.AddModelError("", "Invalid email, login or password!");
        }
        ModelState.AddModelError("", "Invalid provided form!");
        return View(model);
    }

    public IActionResult Register()
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, IFormFile uploadedFile)
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        if (ModelState.IsValid)
        {
            string newFileName = Path.ChangeExtension($"{model.UserName.Trim()}-ProfileN=1", Path.GetExtension(uploadedFile.FileName));
            string path= $"/userImages/" + newFileName.Trim();
            using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            User user = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                UserInfo = model.UserInfo,
                Gender = model.Gender,
                Avatar = path,
                PostCount = 0,
                FollowersCount = 0,
                FollowingsCount = 0
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

    public IActionResult Edit()
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        User model = _db.Users.FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        EditViewModel user = new EditViewModel()
        {
            Email = model.Email,
            FullName = model.FullName,
            UserName = model.UserName,
            PhoneNumber = model.PhoneNumber,
            UserInfo = model.UserInfo,
            Gender = model.Gender
        };
        return View(user);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(EditViewModel model, IFormFile? uploadedFile)
    {
        ViewBag.Genders = new string[] { "Male", "Female"};
        User user = _db.Users.FirstOrDefault(u=>u.Id==int.Parse(_userManager.GetUserId(User)));
        var buffer = user.Avatar.Split('=');
        var buffer2 = buffer[buffer.Length - 1].Split('.');
        if (ModelState.IsValid)
        {
            string? path = null;
            if (uploadedFile!=null)
            {
                string newFileName = Path.ChangeExtension($"{model.UserName.Trim()}-ProfileN={int.Parse(buffer2[0])+1}", Path.GetExtension(uploadedFile.FileName));
                path= $"/userImages/" + newFileName.Trim();
                using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
            }
            user.Id = user.Id;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.Avatar = path!= null ? path : user.Avatar;
            user.UserInfo = model.UserInfo;
            user.PasswordHash = model.Password != null ? _userManager.PasswordHasher.HashPassword(user, model.Password) : user.PasswordHash;
            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Home");
        }
        ModelState.AddModelError("", "Something went wrong! Please check all info");
        return View(model);
    }
    
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}