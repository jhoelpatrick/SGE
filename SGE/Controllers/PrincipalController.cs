using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    public class PrincipalController : Controller
    {
        public IActionResult Index() => PartialView();
    }
}
