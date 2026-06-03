using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class ComercialController : Controller
    {
        public IActionResult Clientes() => RedirectToAction("Index", "Clientes");
        public IActionResult Proveedores() => RedirectToAction("Index", "Proveedores");
        public IActionResult Productos() => RedirectToAction("Index", "Productos");
    }
}
