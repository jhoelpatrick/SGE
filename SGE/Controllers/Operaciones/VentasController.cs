using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class VentasController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Módulo de Ventas";
            return View("~/Views/Operaciones/Ventas.cshtml");
        }
    }
}
