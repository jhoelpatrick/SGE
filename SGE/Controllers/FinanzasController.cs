using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class FinanzasController : Controller
    {
        public IActionResult Impuestos() => PartialView();
        public IActionResult Contabilidad() => PartialView();
        public IActionResult Caja_y_Bancos() => PartialView();
        public IActionResult ActivosFijos() => PartialView();
    }
}
