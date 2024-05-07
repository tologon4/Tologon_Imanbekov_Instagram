using lesson58.Models;
using Microsoft.AspNetCore.Mvc;

namespace lesson58.Controllers;

public class ValidationController : Controller
{
    private InstagramDb _context;

    public ValidationController(InstagramDb context)
    {
        _context = context;
    }
    
    [AcceptVerbs("GET", "POST")]
    public bool CheckEmail(string Email)
    {
        bool result = true;
        foreach (var user in _context.Users)
            if (user.Email.ToLower().Trim() == Email.ToLower().Trim())
                result = false;
        return result;
    }
    [AcceptVerbs("GET", "POST")]
    public bool CheckUsername(string UserName)
    {
        bool result = true;
        foreach (var user in _context.Users)
            if (user.UserName.ToLower().Trim() == UserName.ToLower().Trim())
                result = false;
        return result;
    }
}