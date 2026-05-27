using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class OperacionesController : Controller
    {
        public IActionResult Ventas() => PartialView();
        public IActionResult Compras() => PartialView();
        public IActionResult Facturacion() => PartialView();
        public IActionResult Inventario() => PartialView();
        public IActionResult Proyectos() => PartialView();
    }
}
