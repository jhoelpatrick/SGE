using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class InventarioController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Módulo de Inventario";
            return View("~/Views/Operaciones/Inventario.cshtml");
        }
    }
}
