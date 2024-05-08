using lesson58.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lesson58.Controllers;

public class PostController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private InstagramDb _db;
    private IWebHostEnvironment _environment;

    public PostController(UserManager<User> userManager, SignInManager<User> signInManager, InstagramDb db, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _environment = environment;
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
            string newFileName = Path.ChangeExtension($"{user.UserName.Trim()}-PostN{user.Posts.Count}", Path.GetExtension(uploadedFile.FileName));
            string path= $"/userImages/" + newFileName.Trim();
            using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }
            post.FilePath = path;
            post.LikesCount = 0;
            post.CommentCount = 0;
            _db.Posts.Add(post);
            _db.SaveChangesAsync();
            return RedirectToAction("Profile", "Account", new {id = user.Id});
        }
        return View(post);
    }

    public IActionResult Post(int? id)
    {
        return View();
    }
}