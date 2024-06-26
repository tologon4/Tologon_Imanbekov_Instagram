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
 
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        IWebHostEnvironment environment, InstagramDb db, IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AccountController> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
        _localizer = localizer;
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
        User? curUser = await _userManager.GetUserAsync(User);
        if (curUser != null)
        {
            User? followToUser = await _db.Users.Include(p => p.Posts)
                .Include(l => l.Likes).Include(f => f.Followers)
                .Include(c => c.Comments).FirstOrDefaultAsync(u => u.Id == id);
            ViewBag.CurrentUser = curUser;
            ViewBag.FollowIdent = followToUser.Followers.Any(u => u.FollowFromId == curUser.Id);
            return View(followToUser);
        }
        return RedirectToAction("Login");
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
            if (user != null && user.EmailConfirmed)
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
            ModelState.AddModelError("", "Email is not confirmed");
        }
        ModelState.AddModelError("", "Invalid provided form!");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EmailConfirmMessage(string loginValue)
    {
        User? user = await _userManager.FindByEmailAsync(loginValue);
        if (user == null)
            user = await _userManager.FindByNameAsync(loginValue);
        EmailService emailService = new EmailService();
        if (_db.Users.Any(u => u.Email == user.Email))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("_ConfirmEmail", "Account", new { token, email = user.Email }, Request.Scheme);
            await emailService.SendEmail(user.Email, "Подтверждение Email", $"Пожалуйста, подтвердите свою регистрацию, перейдя по ссылке: <a href='{confirmationLink}'>подтвердить</a>");
            return Ok(true);
        }
        return Ok(false);
    }
    
    public async Task<IActionResult> _ConfirmEmail(string token, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return View("Error");
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return PartialView(model:result.Succeeded.ToString());
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> InfoToEmail()
    {
        User? user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        if (user != null)
        {
            int likesCount = 0;
            foreach (var post in user.Posts)
            {
                likesCount += (int)post.LikesCount!;
            }
            EmailService emailService = new EmailService();
            await emailService.SendEmail(user.Email, "Ваши данные в Instagram", 
                $"Данные по пользователю {user.Id} \n " +
                $"<br/> Email - {user.Email} \n" +
                $"<br/> Логин - {user.UserName} \n" +
                $"<br/> Количесnво подписок - {user.FollowingsCount} \n" +
                $"<br/> Количество подписчиков - {user.FollowersCount} \n" +
                $"<br/> Количесnво публикаций - {user.PostCount} \n" +
                $"<br/> Количесnво лайков - {likesCount} \n" +
                $"<br/> О пользователе {user.UserInfo}");
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
        ViewBag.Genders = new string[] { _localizer["Male"], _localizer["Female"] };

        if (ModelState.IsValid)
        { 
            string newFileName = Path.ChangeExtension($"{model.UserName.Trim()}-ProfileN=1", Path.GetExtension(uploadedFile.FileName)); 
            string path = $"/userImages/" + newFileName.Trim();
        
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
                EmailService emailService = new EmailService(); 
                await emailService.SendEmail(user.Email, "Успешная регистрация в Instagram", $"Поздравляю вас {user.FullName} ! \n" +
                $"<br/> Вы успешно зарегистрировались в Instagram, ваши данные для входа в приложение \n" +
                $"<br/> Логин {user.UserName} \n" +
                $"<br/> Электронная почта {user.Email} \n" +
                $"<br/> <a href=\"http://localhost:5117/Account/Profile/{user.Id}\"> Ваш профиль  </a> ");

                if (user.EmailConfirmed) 
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Home");
                }
                else
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("_ConfirmEmail", "Account", new { token, email = user.Email }, Request.Scheme);
                    await emailService.SendEmail(user.Email, "Подтверждение Email", $"Пожалуйста, подтвердите свою регистрацию, перейдя по ссылке: <a href='{confirmationLink}'>подтвердить</a>");
                    return RedirectToAction("Login");
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ModelState.AddModelError("", "Что-то пошло не так! Пожалуйста, проверьте всю информацию.");
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
            message.Add(path != null ? $"Ваше фото профиля было изменено" : null );
            message.Add(user.UserInfo != model.UserInfo ? $"Ваш Bio был изменен на {model.UserInfo}" : null );
            if (model.Password != null)
                message.Add(user.PasswordHash != _userManager.PasswordHasher.HashPassword(user, model.Password) ? $"Ваш пароль был изменен" : null );
            EmailService emailService = new EmailService();
            await emailService.SendEmail(user.Email, "Редактирование профиля", 
                "Вы успешно изменили ваши персональные данные " + 
                "<br/> Ваши измененные данные : " +
                $"{string.Join("<br/>", message)}"); 
            user.Id = user.Id;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.Avatar = path != null ? path : user.Avatar;
            user.UserInfo = model.UserInfo;
            user.PasswordHash = model.Password != null ? _userManager.PasswordHasher.HashPassword(user, model.Password) : user.PasswordHash;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
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