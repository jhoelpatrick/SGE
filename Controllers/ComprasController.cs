using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models.ViewModels;

namespace SyS_ERP.Controllers
{
    /// <summary>
    /// Módulo de Compras — Órdenes de compra con flujo de aprobación multinivel,
    /// persistencia estática, y métricas de evaluación de proveedores (SLA).
    /// </summary>
    public class ComprasController : Controller
    {
        private readonly ILogger<ComprasController> _logger;

        private static readonly List<OrdenCompra> _ordenes = new()
        {
            new() { Id=1, NroOrden="OC-2024-001", Proveedor="Importadora Global SAC",    Fecha="2024-05-02", Monto=8500.00m,  Estado="Aprobado",  Solicitante="Carlos Ríos"    },
            new() { Id=2, NroOrden="OC-2024-002", Proveedor="Distribuidora Rápida EIRL", Fecha="2024-05-04", Monto=3200.00m,  Estado="Aprobado",  Solicitante="Ana Torres"     },
            new() { Id=3, NroOrden="OC-2024-003", Proveedor="Suministros Tech Perú SAC", Fecha="2024-05-06", Monto=25000.00m, Estado="Bloqueado", Solicitante="Luis Mamani"    }, // Mayor a 20k
            new() { Id=4, NroOrden="OC-2024-004", Proveedor="Ferremax Industrial SRL",   Fecha="2024-05-08", Monto=4750.00m,  Estado="Rechazado", Solicitante="María Condori"  },
            new() { Id=5, NroOrden="OC-2024-005", Proveedor="Logística Norte SAC",       Fecha="2024-05-10", Monto=6100.00m,  Estado="Pendiente", Solicitante="Pedro Vargas"   },
            new() { Id=6, NroOrden="OC-2024-006", Proveedor="Importadora Global SAC",    Fecha="2024-05-11", Monto=5700.00m,  Estado="Aprobado",  Solicitante="Carlos Ríos"    },
            new() { Id=7, NroOrden="OC-2024-007", Proveedor="Proveedor Plus EIRL",       Fecha="2024-05-13", Monto=28000.00m, Estado="Bloqueado", Solicitante="Sandra León"    }, // Mayor a 20k
            new() { Id=8, NroOrden="OC-2024-008", Proveedor="Suministros Tech Perú SAC", Fecha="2024-05-14", Monto=9200.00m,  Estado="Aprobado",  Solicitante="Luis Mamani"    },
        };

        public ComprasController(ILogger<ComprasController> logger)
            => _logger = logger;

        // ── GET /Compras ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new ComprasViewModel
            {
                PendientesAprobacion = _ordenes.Count(o => o.Estado == "Pendiente" || o.Estado == "Bloqueado"),
                GastoMes = _ordenes.Where(o => o.Estado == "Aprobado").Sum(o => o.Monto),
                Ordenes = _ordenes.OrderByDescending(o => o.Id).ToList()
            };
            return View(vm);
        }

        // ── POST: Crear Orden de Compra ───────────────────────────────────────
        [HttpPost]
        public IActionResult CrearOrden([FromBody] OrdenCompra nueva)
        {
            if (nueva == null || string.IsNullOrWhiteSpace(nueva.Proveedor) || nueva.Monto <= 0)
            {
                return Json(new { success = false, message = "Datos de orden inválidos." });
            }

            nueva.Id = _ordenes.Any() ? _ordenes.Max(o => o.Id) + 1 : 1;
            nueva.NroOrden = $"OC-2024-{nueva.Id:D3}";
            nueva.Fecha = DateTime.Now.ToString("yyyy-MM-dd");
            nueva.Solicitante = "Usuario Enterprise";

            // Flujo Jerárquico: Si supera los S/. 20,000, requiere aprobación de Finanzas
            if (nueva.Monto > 20000m)
            {
                nueva.Estado = "Bloqueado";
                _ordenes.Add(nueva);
                return Json(new { 
                    success = true, 
                    bloqueado = true,
                    message = $"Orden registrada pero BLOQUEADA. Excede S/. 20,000. Requiere firma digital del Gerente de Finanzas.",
                    orden = nueva
                });
            }

            nueva.Estado = "Pendiente";
            _ordenes.Add(nueva);
            return Json(new { success = true, bloqueado = false, message = "Orden de compra registrada pendiente de aprobación ordinaria.", orden = nueva });
        }

        // ── POST: Firmar y Autorizar (Gerente Finanzas) ────────────────────────
        [HttpPost]
        public IActionResult FirmarOrden(int id, string firmaCredencial)
        {
            var target = _ordenes.FirstOrDefault(o => o.Id == id);
            if (target == null) return Json(new { success = false, message = "Orden no encontrada." });

            if (firmaCredencial != "FINANZAS2026")
            {
                return Json(new { success = false, message = "Firma digital inválida o código de autorización incorrecto." });
            }

            target.Estado = "Aprobado";
            return Json(new { success = true, message = $"Firma digital estampada. Orden {target.NroOrden} desbloqueada y aprobada." });
        }

        // ── POST: Aprobar Ordinaria ───────────────────────────────────────────
        [HttpPost]
        public IActionResult AprobarOrdenOrdinaria(int id)
        {
            var target = _ordenes.FirstOrDefault(o => o.Id == id);
            if (target == null) return Json(new { success = false, message = "Orden no encontrada." });

            if (target.Estado == "Bloqueado")
            {
                return Json(new { success = false, message = "Esta orden requiere firma jerárquica y no puede aprobarse ordinariamente." });
            }

            target.Estado = "Aprobado";
            return Json(new { success = true, message = "Orden aprobada." });
        }

        // ── POST: Rechazar Orden ──────────────────────────────────────────────
        [HttpPost]
        public IActionResult RechazarOrden(int id)
        {
            var target = _ordenes.FirstOrDefault(o => o.Id == id);
            if (target == null) return Json(new { success = false, message = "Orden no encontrada." });

            target.Estado = "Rechazado";
            return Json(new { success = true, message = "Orden rechazada." });
        }

        // ── POST: Eliminar Orden ──────────────────────────────────────────────
        [HttpPost]
        public IActionResult EliminarOrden(int id)
        {
            var target = _ordenes.FirstOrDefault(o => o.Id == id);
            if (target == null) return Json(new { success = false, message = "Orden no encontrada." });

            _ordenes.Remove(target);
            return Json(new { success = true, message = "Orden eliminada." });
        }

        // ── JsonResult: Historial Gasto Mensual ──────────────────────────────
        [HttpGet]
        public IActionResult GetGastoMensual()
        {
            var data = new
            {
                labels = new[] { "Ene", "Feb", "Mar", "Abr", "May" },
                datasets = new[]
                {
                    new { label="Materiales",   data=new[]{12000,14500,11200,15800,18200}, backgroundColor="#7c3aed" },
                    new { label="Servicios",    data=new[]{ 8500, 9200,10100, 8700, 9800}, backgroundColor="#06b6d4" },
                    new { label="Equipos",      data=new[]{ 5000, 7800, 6500, 9200, 8100}, backgroundColor="#10b981" },
                    new { label="Logística",    data=new[]{ 3200, 4100, 3800, 4500, (int)_ordenes.Where(o => o.Estado == "Aprobado").Sum(o => o.Monto) / 4}, backgroundColor="#f59e0b" },
                }
            };
            return Json(data);
        }

        // ── JsonResult: Top 5 Proveedores (Pastel) ───────────────────────────
        [HttpGet]
        public IActionResult GetTopProveedores()
        {
            var data = new
            {
                labels = new[] {
                    "Importadora Global SAC",
                    "Suministros Tech Perú SAC",
                    "Distribuidora Rápida EIRL",
                    "Ferremax Industrial SRL",
                    "Logística Norte SAC"
                },
                datasets = new[]
                {
                    new
                    {
                        data = new[] { 38500, 24200, 18900, 15400, 9850 },
                        backgroundColor = new[] { "#7c3aed","#06b6d4","#10b981","#f59e0b","#e85d9c" }
                    }
                }
            };
            return Json(data);
        }

        // ── JsonResult: Evaluación de Proveedores SLA (Radar/Barras) ───────────
        [HttpGet]
        public IActionResult GetSlaProveedores()
        {
            return Json(new
            {
                labels = new[] { "Importadora Global", "Suministros Tech", "Distribuidora Rápida", "Ferremax", "Logística Norte" },
                datasets = new[]
                {
                    new {
                        label = "Cumplimiento de Tiempos (%)",
                        data = new[] { 95, 88, 92, 75, 80 },
                        backgroundColor = "rgba(6, 182, 212, 0.4)",
                        borderColor = "#06b6d4",
                        borderWidth = 1
                    },
                    new {
                        label = "Calidad de Insumos (%)",
                        data = new[] { 98, 94, 89, 82, 90 },
                        backgroundColor = "rgba(124, 58, 237, 0.4)",
                        borderColor = "#7c3aed",
                        borderWidth = 1
                    }
                }
            });
        }
    }
}
