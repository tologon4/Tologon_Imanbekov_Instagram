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
}