using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class GestionController : Controller
    {
        public IActionResult Usuario() => PartialView();
        public IActionResult Roles() => PartialView();
    }
}
