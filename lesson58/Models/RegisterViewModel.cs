using System.ComponentModel.DataAnnotations;

namespace lesson58.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please provide a Login")]
    public string Login { get; set; }
    [Required(ErrorMessage = "Please provide an Email")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Please provide a Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required(ErrorMessage = "Please provide a Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Confirm Password is invalid! Try again")]
    public string ConfirmPassword { get; set; }
    [Required(ErrorMessage = "Please provide a Username")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Please provide a Phone Number")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Provide in format: 0123456789")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Please choose a Gender")]
    public string Gender { get; set; }
}