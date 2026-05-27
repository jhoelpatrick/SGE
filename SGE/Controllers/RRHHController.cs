using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class RRHHController : Controller
    {
        public IActionResult Recursos() => PartialView();
        public IActionResult Nominas() => PartialView();
    }
}
