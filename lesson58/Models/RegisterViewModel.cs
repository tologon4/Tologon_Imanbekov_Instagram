using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace lesson58.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please provide a FullName")]
    public string FullName { get; set; }
    [Required(ErrorMessage = "Please provide an Email")]
    [Remote(action: "CheckEmail", controller:"Validation", ErrorMessage = "This email is busy! Try again")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Please provide a Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required(ErrorMessage = "Please provide a Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Confirm Password is invalid! Try again")]
    public string ConfirmPassword { get; set; }
    [Required(ErrorMessage = "Please provide a Username")]
    [Remote(action: "CheckUsername", controller:"Validation", ErrorMessage = "This username is busy! Try again")]
    [RegularExpression(@"^\S+(?:\S+)?$", ErrorMessage = "Provide with no space!")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Please provide a Phone Number")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Provide in format: 0123456789")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Please choose a Gender")]
    public string Gender { get; set; }
    public string? UserInfo { get; set; }
}