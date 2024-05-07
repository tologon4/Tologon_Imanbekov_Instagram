using System.ComponentModel.DataAnnotations;

namespace lesson58.Models;

public class LoginViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
   
    public string? ReturnUrl { get; set; }
}