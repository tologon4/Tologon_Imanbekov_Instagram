using lesson58.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Controllers;
[Authorize]
public class PostController : Controller
{
    private readonly UserManager<User> _userManager;
    private InstagramDb _db;
    private IWebHostEnvironment _environment;
    private IHttpContextAccessor _httpContextAccessor;

    public PostController(UserManager<User> userManager, InstagramDb db, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
    {
        _db = db;
        _userManager = userManager;
        _environment = environment;
        _httpContextAccessor = contextAccessor;
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Comment(int? userId, int? postId, string? comment)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!userId.HasValue || !postId.HasValue)
            return Redirect(referrer);  
        User? user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == userId);
        User? curUser = await _db.Users.Include(p => p.Comments).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        Post? post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        UserPostComm relation = new UserPostComm()
        {
            PostId = post.Id,
            Post = post,
            UserId = curUser.Id,
            User = curUser,
            Comment = comment
        };
        post.CommentCount++;
        curUser.Comments?.Add(relation);
        post.CommentUsers?.Add(relation);
        _db.UserPostComms.Add(relation);
        _db.Posts.Update(post);
        _db.Users.Update(curUser);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new {id = post.Id});
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(int? postId, int? userId)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!postId.HasValue || !userId.HasValue)
            return Redirect(referrer);
        User? user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == userId);
        User? curUser = await _db.Users.FirstOrDefaultAsync(u => u.Id==int.Parse(_userManager.GetUserId(User)));
        if (user ==null || user.Id != curUser.Id)
            return Redirect(referrer);
        Post? post = user.Posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            user.Posts.Remove(post);
            user.PostCount--;
            _db.Posts.Remove(post);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
        return Json(new {deleteSucces = "deleted"});
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(int? postId, int? userId, string content)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!postId.HasValue || !userId.HasValue)
            return Redirect(referrer);
        User? user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == userId);
        User? curUser = await _db.Users.FirstOrDefaultAsync(u => u.Id==int.Parse(_userManager.GetUserId(User)));
        if (user ==null || user.Id != curUser.Id)
            return Redirect(referrer);
        Post? post = user.Posts.FirstOrDefault(p => p.Id == postId);
        if (post != null)
        {
            post.Description = content;
            _db.Posts.Update(post);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
        return Json(new {contentVar = content});
    }
    
    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!id.HasValue)
            return Redirect(referrer);
        User? curUser = await _db.Users.Include(p => p.Posts)
            .FirstOrDefaultAsync(u => u.Id==int.Parse(_userManager.GetUserId(User)));
        Post? post = await _db.Posts.Include(l => l.LikeUsers)
            .Include(o => o.OwnerUser)
            .Include(c => c.CommentUsers)
            .FirstOrDefaultAsync(p => p.Id == id);
        User? followToUser = await _db.Users
            .Include(f => f.Followers)
            .FirstOrDefaultAsync(u => u.Id == post.OwnerUserId);
        ViewBag.Comments = _db.UserPostComms.Include(u => u.User)
            .Include(p => p.Post)
            .Where(c => c.PostId == post.Id);;
        ViewBag.CurrentUser = curUser;
        ViewBag.LikeIdent = post.LikeUsers.Any(u => u.UserId == curUser.Id);
        ViewBag.FollowIdent = followToUser.Followers.Any(u => u.FollowFromId == curUser.Id);
        return View(post);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Like(int? userId, int? postId)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        if (!userId.HasValue || !postId.HasValue)
            return Redirect(referrer);
        User? user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == userId);
        User? curUser = await _db.Users.Include(p => p.Likes).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        Post? post = await _db.Posts.Include(u => u.LikeUsers).FirstOrDefaultAsync(p => p.Id == postId);
        var likeIdent = !post.LikeUsers.Any(u => u.UserId == curUser.Id);
        if (likeIdent)
        {
            UserPostLike relation = new UserPostLike()
            {
                PostId = post.Id,
                Post = post,
                UserId = curUser.Id,
                User = curUser
            };
            post.LikesCount++;
            curUser.Likes?.Add(relation);
            post.LikeUsers?.Add(relation);
            _db.UserPostLikes.Add(relation);
        }
        else
        {
            UserPostLike relation = await _db.UserPostLikes.FirstOrDefaultAsync(p=>p.PostId == post.Id && p.UserId==curUser.Id);
            post.LikesCount--;
            curUser?.Likes?.Remove(relation);
            post.LikeUsers?.Remove(relation);
            _db.UserPostLikes.Remove(relation);
        }
        _db.Posts.Update(post);
        _db.Users.Update(curUser);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return  Json( new { likesCount = post.LikesCount, likeIdentVar = likeIdent});
    }
    [Authorize]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.UserId = _db.Users.Include(p => p.Posts).FirstOrDefault(u=> u.Id==int.Parse(_userManager.GetUserId(User))).Id;
        return View();
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Post post, IFormFile uploadedFile)
    {
        User user = await _db.Users.Include(p => p.Posts).FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        ViewBag.UserId = user.Id;
        if (ModelState.IsValid)
        {
            string newFileName = Path.ChangeExtension($"{user.UserName.Trim()}-PostN={user.Posts.Count}", Path.GetExtension(uploadedFile.FileName));
            string path= $"/userImages/" + newFileName.Trim();
            using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            post.FilePath = path;
            post.OwnerUser = user;
            post.LikesCount = 0;
            post.CommentCount = 0;
            post.AddedDate = DateTime.UtcNow;
            user.PostCount++;
            user.Posts.Add(post);
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Profile", "Account", new {id = user.Id});
        }
        return View(post);
    }
}