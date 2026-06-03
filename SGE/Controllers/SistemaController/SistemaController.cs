using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers.SistemaController
{
    public class SistemaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}