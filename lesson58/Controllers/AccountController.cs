using lesson58.Models;
using lesson58.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace lesson58.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private InstagramDb _db;
    private IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer<AccountController> _localizer;
    private CacheService _service;
 
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        IWebHostEnvironment environment, InstagramDb db, IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AccountController> localizer, CacheService service)
    {
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
        _localizer = localizer;
        _service = service;
        _db = db;
    }

    [Authorize]
    public async Task<IActionResult> Search(string? searchParam)
    {
        User currentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        ViewBag.CurrentUser = currentUser;
        ViewBag.SuggestedUsers = _db.Users
            .Include(u => u.Followers)
            .Where(u => u.Id != currentUser.Id && 
                        !u.Followers.Any(f => f.FollowFromId == currentUser.Id))
            .ToList();
        IQueryable<User>? users = _db.Users.Include(p => p.Posts);
        List<User> usersNew = new List<User>();
        usersNew.AddRange(users.Where(u => u.UserInfo.Contains(searchParam)).ToList());
        usersNew.AddRange(users.Where(u => u.FullName.Contains(searchParam)).ToList());
        usersNew.AddRange(users.Where(u => u.Email.Contains(searchParam)).ToList());
        usersNew.AddRange(users.Where(u => u.UserName.Contains(searchParam)).ToList());
        return View(usersNew.ToHashSet().ToList());
    }
    
    
    [Authorize]
    public async Task<IActionResult> Explore()
    {
        User currentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        List<User> users  = await _db.Users
            .Include(u => u.Followers).Include(p => p.Posts)
            .Where(u => u.Id != currentUser.Id &&
                        !u.Followers.Any(f => f.FollowFromId == currentUser.Id))
            .ToListAsync(); 
        List<Post> posts = new List<Post>();
            
        foreach (var user in users)
            if (user.Posts.Count>0)
                foreach (var post in user.Posts)
                    posts.Add(post);
        posts = posts.OrderByDescending(p => p.AddedDate).ToList();
        return View(posts);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Follow(int? id)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!id.HasValue)
            return Redirect(referrer);
        User? followToUser = await _db.Users.Include(p => p.Followers).FirstOrDefaultAsync(u => u.Id == id);
        User? curUser = await _db.Users.Include(p => p.Followings).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        var followIdent = !followToUser.Followers.Any(u => u.FollowFromId == curUser.Id);
        if (followIdent)
        {
            SubAndSub relation = new SubAndSub()
            {
                FollowToId = followToUser.Id, 
                FollowTo = followToUser, 
                FollowFromId = curUser.Id, 
                FollowFrom = curUser
            };
            curUser.FollowingsCount ++;
            followToUser.FollowersCount++;
            curUser.Followings.Add(relation);
            followToUser.Followers.Add(relation);
            await _db.SubAndSubs.AddAsync(relation);
        }
        else
        {
            SubAndSub relation = await _db.SubAndSubs.FirstOrDefaultAsync(f => f.FollowToId == followToUser.Id && f.FollowFromId == curUser.Id);
            curUser.FollowingsCount --;
            followToUser.FollowersCount--;
            curUser.Followings.Remove(relation);
            followToUser.Followers.Remove(relation);
            _db.SubAndSubs.Remove(relation);
        }
        _db.Users.Update(curUser);
        _db.Users.Update(followToUser);
        await _db.SaveChangesAsync();
        return  Json( new { followersCount = followToUser.FollowersCount, followIdentVar = followIdent});
    }
    
    [Authorize]
    [ResponseCache(CacheProfileName = "Caching")]
    public async Task<IActionResult> Profile(int? id)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!id.HasValue)
            return Redirect(referrer);
        User? curUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        User? followToUser = await _service.GetUser(id);
        if (followToUser == null)
        {
            followToUser = await _db.Users.Include(p => p.Posts)
                .Include(l => l.Likes).Include(f => f.Followers)
                .Include(c => c.Comments).FirstOrDefaultAsync(u => u.Id == id);
            await _service.AddUser(followToUser);
        }
            
        ViewBag.CurrentUser = curUser;
        ViewBag.FollowIdent = followToUser.Followers.Any(u => u.FollowFromId == curUser.Id);
        return View(followToUser);
    }

    [HttpPost]
    public async Task<IActionResult> HomePageLikeIdent(int? postId, int? curUserId)
    {
        Post? post = await _db.Posts.Include(l => l.LikeUsers).FirstOrDefaultAsync(p => p.Id == postId);
        return Json(new {likeIdentVar = post.LikeUsers.Any(u => u.UserId == curUserId)});
    }
    
    [Authorize]
    public async Task<IActionResult> Home()
    {
        User currentUser = await _db.Users.Include(f=>f.Followings).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        ViewBag.CurrentUser = currentUser;
        ViewBag.SuggestedUsers = _db.Users
            .Include(u => u.Followers)
            .Where(u => u.Id != currentUser.Id && u.Followers.All(f => f.FollowFromId != currentUser.Id))
            .ToList(); 
        var followingIds = currentUser?.Followings?.Select(f => f.FollowToId).ToList();
        List<Post> posts = _db.Posts.Include(o => o.OwnerUser)
            .Include(u => u.LikeUsers)
            .Where(o => o.OwnerUserId != currentUser.Id && followingIds.Contains(o.OwnerUserId))
            .ToList();
        posts = posts?.OrderByDescending(p => p.AddedDate).ToList();
        return View(posts);
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

    public async Task<IActionResult> InfoToEmail()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            EmailService emailService = new EmailService();
            await emailService.SendEmail(user.Email, "Ваши данные в Instagram", 
                $"Данные по пользователю");
            return Ok(true);
        }
        return Ok(false);
    }
    
    public IActionResult Register()
    {
        ViewBag.Genders = new string[] {_localizer["Male"], _localizer["Female"]};
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model, IFormFile uploadedFile)
    {
        ViewBag.Genders = new string[] { _localizer["Male"], _localizer["Female"]};
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
                await _service.AddUser(user);
                EmailService emailService = new EmailService();
                await emailService.SendEmail(user.Email, "Успешная регистрация в Instagram", $"Поздравляю вас {user.FullName} ! \n" +
                    $"Вы успешно зарегистрировались в Instagram, ваши данные для входа в приложение \n" +
                    $"Логин {user.UserName} \n" +
                    $"Электронная почта {user.Email}");
                return RedirectToAction("Home");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
        ModelState.AddModelError("", "Something went wrong! Please check all info");
        return View(model);
    }
    
    [Authorize]
    public IActionResult Edit()
    {
        ViewBag.Genders = new string[] { _localizer["Male"], _localizer["Female"]};
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
        ViewBag.Genders = new string[] { _localizer["Male"], _localizer["Female"]};
        User user = _db.Users.Include(u => u.Followers)
            .Include(u => u.Likes)
            .Include(u => u.Followings)
            .Include(u => u.Comments)
            .Include(u => u.Posts).FirstOrDefault(u=>u.Id==int.Parse(_userManager.GetUserId(User)));
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

            List<string?> message = new List<string?>();
            message.Add(user.Email != model.Email ? $"Ваш Email был изменен на {model.Email}" : null );
            message.Add(user.UserName != model.UserName ? $"Ваш UserName был изменен на {model.UserName}" : null );
            message.Add(user.FullName != model.FullName ? $"Ваш FullName был изменен на {model.FullName}" : null );
            message.Add(user.Gender != model.Gender ? $"Ваш пол был изменен на {model.Gender}" : null );
            message.Add(user.PhoneNumber != model.PhoneNumber ? $"Ваш номер телефона был изменен на {model.PhoneNumber}" : null );
            message.Add(user.Avatar != path ? $"Ваше фото профиля было изменено" : null );
            message.Add(user.UserInfo != model.UserInfo ? $"Ваш Bio был изменен на {model.UserInfo}" : null );
            message.Add(user.PasswordHash != _userManager.PasswordHasher.HashPassword(user, model.Password) ? $"Ваш пароль был изменен" : null );
            EmailService emailService = new EmailService();
            await emailService.SendEmail(user.Email, "Редактирование профиля", 
                "Вы успешно изменили ваши персональные данные\n" + 
                "Ваши измененные данные : \n" +
                $"{string.Join("\n", message)}"); 
            user.Id = user.Id;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.Avatar = path != null ? path : user.Avatar;
            user.UserInfo = model.UserInfo;
            user.PasswordHash = model.Password != null ? _userManager.PasswordHasher.HashPassword(user, model.Password) : user.PasswordHash;
            await _userManager.UpdateAsync(user);
            _db.Users.Update(user);
            int n = await _db.SaveChangesAsync();
            if (n> 0)
            {
                await _service.RemoveUser(user.Id);
                await _service.AddUser(user);
            }
            
            return RedirectToAction("Profile", new {id=user.Id});
        }
        ModelState.AddModelError("", "Something went wrong! Please check all info");
        return View(model);
    }
    
    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}