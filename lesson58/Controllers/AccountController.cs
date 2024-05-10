using lesson58.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


    public IActionResult Search(string? searchParam)
    {
        User currentUser = _db.Users.FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
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
    public IActionResult Explore()
    {
        User currentUser = _db.Users.FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        List<User> users  = _db.Users
            .Include(u => u.Followers).Include(p => p.Posts)
            .Where(u => u.Id != currentUser.Id &&
                        !u.Followers.Any(f => f.FollowFromId == currentUser.Id))
            .ToList(); 
        List<Post> posts = new List<Post>();
            
        foreach (var user in users)
            if (user.Posts.Count>0)
                foreach (var post in user.Posts)
                    posts.Add(post);
        posts = posts.OrderByDescending(p => p.AddedDate).ToList();
        return View(posts);
    }
    public IActionResult Follow(int? id)
    {
        User? followToUser = _db.Users.Include(p => p.Followers).FirstOrDefault(u => u.Id == id);
        User? curUser = _db.Users.Include(p => p.Followings).FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        
        bool Answer()
        {
            
            int res = 0;
            if (followToUser.Followers.Count == 0)
                res = 0;
            else
                foreach (var rel in followToUser.Followers)
                    if (rel.FollowToId == followToUser.Id)
                        if (rel.FollowFromId == curUser.Id)
                            res++;
            if (followToUser.Id == curUser.Id)
                res ++;
            return res >= 1 ? false : true;
        }
        
        if (Answer()==true)
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
            _db.SubAndSubs.Add(relation);
        }
        else
        {
            SubAndSub relation = _db.SubAndSubs.FirstOrDefault(f => f.FollowToId ==followToUser.Id && f.FollowFromId==curUser.Id);
            curUser.FollowingsCount --;
            followToUser.FollowersCount--;
            curUser.Followings.Remove(relation);
            followToUser.Followers.Remove(relation);
            _db.SubAndSubs.Remove(relation);
        }
        _db.Users.Update(curUser);
        _db.Users.Update(followToUser);
        _db.SaveChanges();
        return RedirectToAction("Profile", new {id = followToUser.Id});
    }
    public IActionResult Profile(int? id)
    {
        User? curUser = _db.Users.FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        User? followToUser = _db.Users.Include(p => p.Posts)
            .Include(l => l.Likes).Include(f => f.Followers)
            .Include(c => c.Comments).FirstOrDefault(u => u.Id == id);
        ViewBag.CurrentUser = curUser;
        bool Answer()
        {
            int res = 0;
            if (followToUser.Followers.Count == 0)
                res = 0;
            else
                foreach (var rel in followToUser.Followers)
                    if (rel.FollowToId == followToUser.Id)
                        if (rel.FollowFromId == curUser.Id)
                            res++;
            return res >= 1 ? false : true;
        }
        ViewBag.FollowQue = Answer();
        return View(followToUser);
    }
    
    public IActionResult Home()
    {
        User currentUser = _db.Users.Include(f=>f.Followings).FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        ViewBag.CurrentUser = currentUser;
        ViewBag.SuggestedUsers = _db.Users
            .Include(u => u.Followers)
            .Where(u => u.Id != currentUser.Id &&
                        !u.Followers.Any(f => f.FollowFromId == currentUser.Id))
            .ToList(); 
        List<User> followingUsers = _db.Users.Include(p => p.Posts)
             .Include(u => u.Followers)
             .Where(u => u.Id != currentUser.Id &&
                         u.Followers.Any(f => f.FollowFromId == currentUser.Id)).ToList();

        
        var followingIds = currentUser?.Followings?.Select(f => f.FollowToId).ToList();

        List<Post> posts2 = _db.Posts
            .Include(u => u.LikeUsers)
            .Where(o => o.OwnerUserId != currentUser.Id && followingIds.Contains(o.OwnerUserId))
            .ToList();
        
        List<Post>? posts = _db.Posts.Include(u => u.LikeUsers)
            .Where(o => o.OwnerUserId != currentUser.Id).ToList();
        foreach (var user in followingUsers)
            if (user.Posts.Count>0)
                foreach (var post in user.Posts)
                    posts?.Add(post);
        posts = posts?.OrderByDescending(p => p.AddedDate).ToList();
        return View(posts2);
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
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Profile", new {id=user.Id});
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