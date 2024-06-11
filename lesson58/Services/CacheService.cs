using lesson58.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace lesson58.Services;

public class CacheService
{
    private InstagramDb _db;
    private IMemoryCache _cache;

    public CacheService(InstagramDb db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<Post>> GetPosts()
    {
        return await _db.Posts.ToListAsync();
    }

    public async Task AddPost(Post post)
    {
        _cache.Set(post.Id, post, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
    }
    

    public async Task<Post> GetPost(int? id)
    {
        Post post = null;
        if (!_cache.TryGetValue(id, out post))
        {
            post = await _db.Posts.Include(p => p.LikeUsers)
                .Include(p => p.CommentUsers)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post != null)
                _cache.Set(post.Id, post, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
        }

        return post;
    }

    public async Task RemovePost(object key)
    {
        _cache.Remove(key);
    }

    public async Task RemoveUser(object key)
    {
        _cache.Remove(key);
    }
    public async Task AddUser(User user)
    {
        _cache.Set(user.Id, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
    }
    

    public async Task<User> GetUser(int? id)
    {
        User user = null;
        if (!_cache.TryGetValue(id, out user))
        {
            user = await _db.Users.Include(u => u.Posts)
                .Include(u => u.Followers)
                .Include(u => u.Followings)
                .Include(u => u.Comments)
                .Include(u => u.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (user != null)
                _cache.Set(user.Id, user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
        }
        return user;
    }
    

}