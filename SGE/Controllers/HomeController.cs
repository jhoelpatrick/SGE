using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using System.Diagnostics;

namespace SGE.Controllers
{
    public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dashboard");
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
}
