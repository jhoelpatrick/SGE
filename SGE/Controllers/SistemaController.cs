using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class SistemaController : Controller
    {
        public IActionResult Reportes() => PartialView();
        public IActionResult Auditoria() => PartialView();
        public IActionResult Configuracion() => PartialView();
    }
}
