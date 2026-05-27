using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models.ViewModels;

namespace SyS_ERP.Controllers
{
    /// <summary>
    /// Módulo de Ventas — CRUD visual de pedidos + JsonResults para Chart.js.
    /// Incluye lógica Enterprise de límites de crédito, descuentos complejos y simulación de pagos.
    /// </summary>
    public class VentasController : Controller
    {
        private readonly ILogger<VentasController> _logger;

        // Lista estática para simular persistencia
        private static readonly List<Pedido> _pedidos = new()
        {
            new() { Id=1, NroPedido="PED-2024-001", Cliente="Distribuidora Norte SAC",  Fecha="2024-05-01", Total=4850.00m,  Estado="Despachado", Moneda="PEN", Descuento=150m, MetodoPago="Visa", TransaccionId="TX-98214" },
            new() { Id=2, NroPedido="PED-2024-002", Cliente="Comercial Lima EIRL",       Fecha="2024-05-03", Total=12300.00m, Estado="Aprobado",   Moneda="PEN", Descuento=0m,    MetodoPago="Crédito" },
            new() { Id=3, NroPedido="PED-2024-003", Cliente="Tech Solutions Perú SAC",   Fecha="2024-05-05", Total=7250.00m,  Estado="Pendiente",  Moneda="USD", Descuento=725m,  MetodoPago="Paypal", TransaccionId="TX-34981" },
            new() { Id=4, NroPedido="PED-2024-004", Cliente="Grupo Andino Corp",         Fecha="2024-05-07", Total=3100.00m,  Estado="Despachado", Moneda="PEN", Descuento=0m,    MetodoPago="Mastercard", TransaccionId="TX-55109" },
            new() { Id=5, NroPedido="PED-2024-005", Cliente="Importaciones Sur EIRL",    Fecha="2024-05-09", Total=9800.00m,  Estado="Aprobado",   Moneda="PEN", Descuento=980m,  MetodoPago="Visa", TransaccionId="TX-11824" },
            new() { Id=6, NroPedido="PED-2024-006", Cliente="Ferretería Central SAC",    Fecha="2024-05-10", Total=1540.00m,  Estado="Pendiente",  Moneda="PEN", Descuento=0m },
            new() { Id=7, NroPedido="PED-2024-007", Cliente="Distribuidora Oriente SRL", Fecha="2024-05-12", Total=6720.00m,  Estado="Cancelado",  Moneda="EUR", Descuento=300m,  MetodoPago="Paypal", TransaccionId="TX-00492" },
            new() { Id=8, NroPedido="PED-2024-008", Cliente="Megacom Distribuciones SAC",Fecha="2024-05-14", Total=15000.00m, Estado="Aprobado",   Moneda="PEN", Descuento=0m,    MetodoPago="Crédito" },
        };

        // Límites de Crédito empresariales (Cliente -> (Límite, Deuda Actual))
        private static readonly Dictionary<string, (decimal Limite, decimal Deuda)> _limitesCredito = new()
        {
            { "Distribuidora Norte SAC", (20000.00m, 17500.00m) }, // Espacio libre: 2,500
            { "Comercial Lima EIRL", (50000.00m, 15000.00m) },
            { "Tech Solutions Perú SAC", (15000.00m, 12000.00m) },
            { "Grupo Andino Corp", (80000.00m, 20000.00m) },
            { "Importaciones Sur EIRL", (30000.00m, 5000.00m) },
            { "Ferretería Central SAC", (5000.00m, 4500.00m) },
            { "Distribuidora Oriente SRL", (25000.00m, 10000.00m) },
            { "Megacom Distribuciones SAC", (100000.00m, 35000.00m) }
        };

        public VentasController(ILogger<VentasController> logger)
            => _logger = logger;

        // ── GET /Ventas ────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new VentasViewModel
            {
                TotalMes          = _pedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.Total),
                PedidosPendientes = _pedidos.Count(p => p.Estado == "Pendiente"),
                PedidosAprobados  = _pedidos.Count(p => p.Estado == "Aprobado"),
                Pedidos = _pedidos.OrderByDescending(p => p.Id).ToList()
            };
            return View(vm);
        }

        // ── JsonResult: Datos de Limites de Crédito por Cliente ────────────────
        [HttpGet]
        public IActionResult GetCreditoCliente(string cliente)
        {
            if (_limitesCredito.TryGetValue(cliente, out var info))
            {
                return Json(new { success = true, limite = info.Limite, deuda = info.Deuda, disponible = info.Limite - info.Deuda });
            }
            return Json(new { success = true, limite = 10000.00m, deuda = 0.00m, disponible = 10000.00m }); // Default para nuevos
        }

        // ── POST: Crear Pedido ────────────────────────────────────────────────
        [HttpPost]
        public IActionResult CrearPedido([FromBody] Pedido nuevo)
        {
            if (nuevo == null || string.IsNullOrWhiteSpace(nuevo.Cliente))
            {
                return Json(new { success = false, message = "Datos del pedido inválidos." });
            }

            // Validar Límite de Crédito si el método de pago es "Crédito" o si el estado es para Aprobación
            if (_limitesCredito.TryGetValue(nuevo.Cliente, out var info))
            {
                decimal totalEnSoles = nuevo.Moneda == "USD" ? nuevo.Total * 3.75m : nuevo.Moneda == "EUR" ? nuevo.Total * 4.10m : nuevo.Total;
                if (info.Deuda + totalEnSoles > info.Limite)
                {
                    return Json(new { 
                        success = false, 
                        creditoExcedido = true,
                        message = $"Límite de crédito excedido para {nuevo.Cliente}. Límite: S/. {info.Limite:N2}, Deuda Actual: S/. {info.Deuda:N2}, Disponible: S/. {(info.Limite - info.Deuda):N2}. Pedido actual equivale a S/. {totalEnSoles:N2}." 
                    });
                }
            }

            nuevo.Id = _pedidos.Any() ? _pedidos.Max(p => p.Id) + 1 : 1;
            nuevo.NroPedido = $"PED-2024-{nuevo.Id:D3}";
            nuevo.Fecha = DateTime.Now.ToString("yyyy-MM-dd");
            _pedidos.Add(nuevo);

            // Si es Crédito, sumar a la deuda del cliente temporalmente para simulaciones
            if (nuevo.MetodoPago == "Crédito" && _limitesCredito.ContainsKey(nuevo.Cliente))
            {
                var current = _limitesCredito[nuevo.Cliente];
                decimal totalEnSoles = nuevo.Moneda == "USD" ? nuevo.Total * 3.75m : nuevo.Moneda == "EUR" ? nuevo.Total * 4.10m : nuevo.Total;
                _limitesCredito[nuevo.Cliente] = (current.Limite, current.Deuda + totalEnSoles);
            }

            return Json(new { success = true, message = "Pedido registrado con éxito.", pedido = nuevo });
        }

        // ── POST: Editar Pedido ────────────────────────────────────────────────
        [HttpPost]
        public IActionResult EditarPedido([FromBody] Pedido editado)
        {
            var target = _pedidos.FirstOrDefault(p => p.Id == editado.Id);
            if (target == null) return Json(new { success = false, message = "Pedido no encontrado." });

            target.Cliente = editado.Cliente;
            target.Total = editado.Total;
            target.Moneda = editado.Moneda;
            target.Estado = editado.Estado;
            target.Descuento = editado.Descuento;

            return Json(new { success = true, message = "Pedido actualizado correctamente." });
        }

        // ── POST: Eliminar Pedido ──────────────────────────────────────────────
        [HttpPost]
        public IActionResult EliminarPedido(int id)
        {
            var target = _pedidos.FirstOrDefault(p => p.Id == id);
            if (target == null) return Json(new { success = false, message = "Pedido no encontrado." });

            _pedidos.Remove(target);
            return Json(new { success = true, message = "Pedido eliminado." });
        }

        // ── POST: Simular Pago ─────────────────────────────────────────────────
        [HttpPost]
        public IActionResult ProcesarPago(int id, string metodo, string moneda, decimal monto)
        {
            var target = _pedidos.FirstOrDefault(p => p.Id == id);
            if (target == null) return Json(new { success = false, message = "Pedido no encontrado." });

            target.MetodoPago = metodo;
            target.Moneda = moneda;
            target.TransaccionId = $"TX-{Random.Shared.Next(10000, 99999)}";
            target.Estado = "Aprobado"; // Al pagar se aprueba el pedido

            return Json(new { 
                success = true, 
                message = "Pago conciliado con éxito en la pasarela de pagos.",
                transaccionId = target.TransaccionId,
                estado = target.Estado
            });
        }

        // ── POST: Calcular Descuento (Strategy Pattern Sim) ───────────────────
        [HttpPost]
        public IActionResult CalcularDescuentoValido(decimal total, string cupon, int cantidadItems)
        {
            decimal descPorcentaje = 0;
            string estrategia = "Ninguna";

            // Regla 1: Descuento por Volumen (Strategy 1)
            if (cantidadItems >= 10 || total >= 8000m)
            {
                descPorcentaje = 0.10m; // 10%
                estrategia = "Descuento por Volumen (10%)";
            }
            
            // Regla 2: Cupón Enterprise (Strategy 2, acumulable o sustituto, aplicamos el mayor)
            if (cupon == "ENT2026")
            {
                if (0.15m > descPorcentaje)
                {
                    descPorcentaje = 0.15m; // 15%
                    estrategia = "Cupón Corporativo ENT2026 (15%)";
                }
            }

            decimal totalDescuento = total * descPorcentaje;
            decimal totalFinal = total - totalDescuento;

            return Json(new { 
                success = true, 
                descuento = totalDescuento, 
                totalNeto = totalFinal, 
                estrategiaAplicada = estrategia 
            });
        }

        // ── JsonResult: Tendencia de Ingresos Semanales ───────────────────────
        [HttpGet]
        public IActionResult GetDatosIngresosSemanales()
        {
            var data = new
            {
                labels = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" },
                datasets = new[]
                {
                    new
                    {
                        label           = "Ingresos esta semana (S/.)",
                        data            = new[] { 8200, 11500, 9800, 14200, 18700, 21000, 16400 },
                        borderColor     = "#7c3aed",
                        backgroundColor = "rgba(124,58,237,0.15)",
                        fill            = true,
                        tension         = 0.4
                    }
                }
            };
            return Json(data);
        }

        // ── JsonResult: Tendencia de Ingresos Mensuales ───────────────────────
        [HttpGet]
        public IActionResult GetDatosIngresosMensuales()
        {
            // Agrupar ingresos reales por mes o simular
            var data = new
            {
                labels = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun",
                                  "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" },
                datasets = new[]
                {
                    new
                    {
                        label           = "Ingresos mensuales (S/.)",
                        data            = new[] { 52000, 65000, 71200, 68000, (int)_pedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.Total), 0, 0, 0, 0, 0, 0, 0 },
                        borderColor     = "#06b6d4",
                        backgroundColor = "rgba(6,182,212,0.12)",
                        fill            = true,
                        tension         = 0.4
                    }
                }
            };
            return Json(data);
        }

        // ── JsonResult: Contadores KPI (polling AJAX) ─────────────────────────
        [HttpGet]
        public IActionResult GetKpis()
        {
            decimal total = _pedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.Total);
            return Json(new
            {
                totalMes          = "S/. " + total.ToString("N2"),
                pedidosPendientes = _pedidos.Count(p => p.Estado == "Pendiente"),
                pedidosAprobados  = _pedidos.Count(p => p.Estado == "Aprobado"),
                variacion         = "+12.4%"
            });
        }
    }
}
