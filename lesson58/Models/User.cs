using Microsoft.AspNetCore.Identity;

namespace lesson58.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    public string FullName { get; set; }
    public string? UserInfo { get; set; }
    public string Gender { get; set; }
    public ICollection<SubAndSub> Subscribers { get; set; }
    public ICollection<SubAndSub> Subscribtions { get; set; }
    public ICollection<Post>? Posts { get; set; }
    public ICollection<UserPostLike>? Likes { get; set; }
    public ICollection<UserPostComm>? Comments { get; set; }
    public int? PostCount { get; set; }
    public int? SubscribersCount { get; set; }
    public int? SubscribtionsCount { get; set; }
}