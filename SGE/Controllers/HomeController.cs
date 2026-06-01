using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers;

public class HomeController : Controller
{
    private readonly UsuariosService _usuarios;

    public HomeController(UsuariosService usuarios)
    {
        _usuarios = usuarios;
    }

    public IActionResult Index()
    {
        ViewBag.TotalUsuarios = _usuarios.ObtenerTodos().Count;
        ViewBag.TotalRoles    = SistemaRoles.Lista.Length;
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
