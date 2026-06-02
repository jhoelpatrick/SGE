using Microsoft.AspNetCore.Mvc;

namespace Reportes.Controllers
{
    public class SistemaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}