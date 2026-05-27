using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models;
using System.Diagnostics;

namespace SyS_ERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
            => _logger = logger;

        // ── Dashboard principal ───────────────────────────────────────────────
        public IActionResult Index()
        {
            var vm = new DashboardViewModel
            {
                UsuariosActivos       = "1,284",
                VentasDelMes          = "348",
                FacturasEmitidas      = "217",
                IngresosTotales       = "S/. 89,540",
                PedidosPendientes     = 24,
                OrdenesCompra         = 11,
                ComprobantesMes       = 217,
                AlertasStock          = 8,
                ProyectosActivos      = 5
            };
            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
