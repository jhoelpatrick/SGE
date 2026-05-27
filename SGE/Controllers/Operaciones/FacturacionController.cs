using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class FacturacionController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Módulo de Facturación";
            return View("~/Views/Operaciones/Facturacion.cshtml");
        }
    }
}
