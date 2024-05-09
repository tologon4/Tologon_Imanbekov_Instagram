using Microsoft.AspNetCore.Identity;

namespace lesson58.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    public string? FullName { get; set; }
    public string? UserInfo { get; set; }
    public string? Gender { get; set; }
    public ICollection<SubAndSub> Followers { get; set; }
    public ICollection<SubAndSub> Followings { get; set; }
    public ICollection<Post>? Posts { get; set; }
    public ICollection<UserPostLike>? Likes { get; set; }
    public int? PostCount { get; set; }
    public int? FollowersCount { get; set; }
    public int? FollowingsCount { get; set; }
}