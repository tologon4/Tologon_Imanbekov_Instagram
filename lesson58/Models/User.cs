using Microsoft.AspNetCore.Identity;

namespace lesson58.Models;

public class User : IdentityUser<int>
{
    public string Avatar { get; set; }
    public string Login { get; set; }
    public string UserInfo { get; set; }
    public string Gender { get; set; }
}