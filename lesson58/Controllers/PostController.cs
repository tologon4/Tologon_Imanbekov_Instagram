using lesson58.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Controllers;

public class PostController : Controller
{
    private readonly UserManager<User> _userManager;
    private InstagramDb _db;
    private IWebHostEnvironment _environment;
    private IHttpContextAccessor _httpContextAccessor;

    public PostController(UserManager<User> userManager, InstagramDb db, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor)
    {
        _userManager = userManager;
        _db = db;
        _environment = environment;
        _httpContextAccessor = contextAccessor;
    }
    
    [HttpPost]
    public IActionResult Comment(int? userId, int? postId, string? comment)
    {
        User? user = _db.Users.Include(p => p.Posts).FirstOrDefault(u => u.Id == userId);
        User? curUser = _db.Users.Include(p => p.Comments).FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        Post? post = user.Posts.FirstOrDefault(p => p.Id == postId);
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
        _db.SaveChanges();
        return RedirectToAction("Post", new {id = post.Id});
    }
    
    public IActionResult Post(int? id)
    {
        User? curUser = _db.Users.Include(p => p.Posts)
            .FirstOrDefault(u => u.Id==int.Parse(_userManager.GetUserId(User)));
        ViewBag.CurrentUser = curUser;
        Post? post = _db.Posts.Include(l => l.LikeUsers)
            .Include(o => o.OwnerUser)
            .Include(c => c.CommentUsers)
            .Include(u => u.OwnerUser)
            .FirstOrDefault(p => p.Id == id);
        bool Answer()
        {
            int res = 0;
            if (post.LikeUsers == null  || post.LikeUsers.Count == 0)
                res = 0;
            else
                foreach (var usr in post.LikeUsers)
                    if (usr.UserId == curUser.Id)
                        res++;
            return res >= 1 ? false : true;
        }
        User? followToUser = _db.Users
            .Include(f => f.Followers)
            .FirstOrDefault(u => u.Id == post.OwnerUserId);
        ViewBag.CurrentUser = curUser;
        bool AnswerFollow()
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
        ViewBag.LikeQue = Answer();
        ViewBag.FollowQue = AnswerFollow();
        return View(post);
    }
    
    public IActionResult Like(int? userId, int? postId)
    {
        var referrer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        User? user = _db.Users.Include(p => p.Posts).FirstOrDefault(u => u.Id == userId);
        User? curUser = _db.Users.Include(p => p.Likes).FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
        Post? post = user.Posts.FirstOrDefault(p => p.Id == postId);
        bool Answer()
        {
            int res = 0;
            if (post.LikeUsers == null  || post.LikeUsers.Count == 0)
                res = 0;
            else
                foreach (var usr in post.LikeUsers)
                    if (usr.UserId == curUser.Id)
                        res++;
            return res >= 1 ? false : true;
        }
        if (Answer()==true)
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
            UserPostLike relation = _db.UserPostLikes.FirstOrDefault(p=>p.PostId == post.Id && p.UserId==curUser.Id);
            post.LikesCount--;
            curUser.Likes?.Remove(relation);
            post.LikeUsers?.Remove(relation);
            _db.UserPostLikes.Remove(relation);
        }
        _db.Posts.Update(post);
        _db.Users.Update(curUser);
        _db.Users.Update(user);
        _db.SaveChanges();
        return Redirect(referrer);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.UserId = _db.Users.FirstOrDefault(u=> u.Id==int.Parse(_userManager.GetUserId(User))).Id;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Create(Post post, IFormFile uploadedFile)
    {
        User user = _db.Users.Include(p => p.Posts).FirstOrDefault(u => u.Id == int.Parse(_userManager.GetUserId(User)));
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
            _db.SaveChanges();
            return RedirectToAction("Profile", "Account", new {id = user.Id});
        }
        return View(post);
    }
    
}