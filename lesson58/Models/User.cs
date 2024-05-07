using Microsoft.AspNetCore.Identity;

namespace lesson58.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    public string FullName { get; set; }
    public string? UserInfo { get; set; }
    public string Gender { get; set; }
    public ICollection<User> Subscribers { get; set; }
    public ICollection<User> Subscribtions { get; set; }
}