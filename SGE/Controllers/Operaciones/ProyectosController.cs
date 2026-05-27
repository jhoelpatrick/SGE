using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class ProyectosController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Módulo de Proyectos";
            return View("~/Views/Operaciones/Proyectos.cshtml");
        }
    }
}
