using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class ComercialController : Controller
    {
        public IActionResult Clientes() => PartialView();
        public IActionResult Proveedores() => PartialView();
        public IActionResult Productos() => PartialView();
     
    }
}
