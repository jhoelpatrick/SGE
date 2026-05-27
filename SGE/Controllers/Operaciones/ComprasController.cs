using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class ComprasController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Módulo de Compras";
            return View("~/Views/Operaciones/Compras.cshtml");
        }
    }
}
