using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models.ViewModels;

namespace SyS_ERP.Controllers
{
    /// <summary>
    /// Módulo de Inventario — Control de stock con alertas condicionales,
    /// persistencia estática, gestión multi-almacén, transferencias y trazabilidad por lotes/vencimiento.
    /// </summary>
    public class InventarioController : Controller
    {
        private readonly ILogger<InventarioController> _logger;

        private static readonly List<ProductoStock> _productos = new()
        {
            new() { Id=1,  Codigo="PRD-001", Nombre="Laptop HP 15\" i7",          Categoria="Electrónica",  StockActual=12, StockMinimo=5,  Unidad="UND", Lote="LT-2024-HP01", FechaVencimiento="2027-05-20", Almacen="Lima Central",   NroSerie="SN-HP-8871" },
            new() { Id=2,  Codigo="PRD-002", Nombre="Mouse Inalámbrico Logitech", Categoria="Periféricos",  StockActual=0,  StockMinimo=10, Unidad="UND", Lote="LT-2024-LG09", FechaVencimiento="2028-10-15", Almacen="Lima Central",   NroSerie="SN-LG-0024" },
            new() { Id=3,  Codigo="PRD-003", Nombre="Teclado Mecánico RGB",        Categoria="Periféricos",  StockActual=3,  StockMinimo=8,  Unidad="UND", Lote="LT-2024-KB12", FechaVencimiento="2028-09-01", Almacen="Arequipa Sur",   NroSerie="SN-KB-1109" },
            new() { Id=4,  Codigo="PRD-004", Nombre="Monitor 24\" Full HD",        Categoria="Electrónica",  StockActual=8,  StockMinimo=4,  Unidad="UND", Lote="LT-2024-MN02", FechaVencimiento="2027-12-05", Almacen="Trujillo Norte", NroSerie="SN-MN-4456" },
            new() { Id=5,  Codigo="PRD-005", Nombre="Papel A4 (Resma)",            Categoria="Oficina",      StockActual=0,  StockMinimo=20, Unidad="RES", Lote="LT-2024-PA88", FechaVencimiento="2026-06-30", Almacen="Lima Central" }, // Cerca a vencer en 30 días!
            new() { Id=6,  Codigo="PRD-006", Nombre="Bolígrafos Azules (Caja)",    Categoria="Oficina",      StockActual=15, StockMinimo=10, Unidad="CJA", Lote="LT-2024-BO11", FechaVencimiento="2029-01-01", Almacen="Lima Central" },
            new() { Id=7,  Codigo="PRD-007", Nombre="Escritorio Ejecutivo",        Categoria="Mobiliario",   StockActual=4,  StockMinimo=2,  Unidad="UND", Lote="LT-2024-MB04", FechaVencimiento="2035-12-31", Almacen="Arequipa Sur" },
            new() { Id=8,  Codigo="PRD-008", Nombre="Silla Ergonómica",            Categoria="Mobiliario",   StockActual=2,  StockMinimo=5,  Unidad="UND", Lote="LT-2024-MB08", FechaVencimiento="2035-12-31", Almacen="Lima Central" },
            new() { Id=9,  Codigo="PRD-009", Nombre="Aceite Industrial 5L",        Categoria="Insumos",      StockActual=45, StockMinimo=15, Unidad="LTS", Lote="LT-2024-AC02", FechaVencimiento="2026-08-10", Almacen="Trujillo Norte" },
            new() { Id=10, Codigo="PRD-010", Nombre="Cable UTP Cat6 (Rollo 100m)", Categoria="Redes",        StockActual=1,  StockMinimo=3,  Unidad="ROL", Lote="LT-2024-RD07", FechaVencimiento="2030-04-11", Almacen="Lima Central" },
            new() { Id=11, Codigo="PRD-011", Nombre="Switch 24 Puertos Cisco",     Categoria="Redes",        StockActual=5,  StockMinimo=2,  Unidad="UND", Lote="LT-2024-RD11", FechaVencimiento="2031-03-12", Almacen="Lima Central",   NroSerie="SN-CS-0044" },
            new() { Id=12, Codigo="PRD-012", Nombre="Impresora Térmica POS",       Categoria="Electrónica",  StockActual=0,  StockMinimo=2,  Unidad="UND", Lote="LT-2024-EP90", FechaVencimiento="2026-06-25", Almacen="Arequipa Sur",   NroSerie="SN-EP-9981" }, // Cerca a vencer en 30 días!
        };

        private static readonly List<MovimientoAlmacen> _movimientos = new()
        {
            new() { Id=1, Fecha="2024-05-14", Producto="Laptop HP 15\" i7",          Tipo="Entrada", Cantidad=10, Usuario="Admin" },
            new() { Id=2, Fecha="2024-05-14", Producto="Mouse Inalámbrico Logitech", Tipo="Salida",  Cantidad=5,  Usuario="Carlos R." },
            new() { Id=3, Fecha="2024-05-13", Producto="Papel A4 (Resma)",            Tipo="Salida",  Cantidad=30, Usuario="Ana T." },
            new() { Id=4, Fecha="2024-05-13", Producto="Aceite Industrial 5L",        Tipo="Entrada", Cantidad=50, Usuario="Admin" },
            new() { Id=5, Fecha="2024-05-12", Producto="Monitor 24\" Full HD",        Tipo="Entrada", Cantidad=5,  Usuario="Admin" },
            new() { Id=6, Fecha="2024-05-11", Producto="Silla Ergonómica",            Tipo="Salida",  Cantidad=3,  Usuario="María C." },
        };

        public InventarioController(ILogger<InventarioController> logger)
            => _logger = logger;

        // ── GET /Inventario ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new InventarioViewModel
            {
                Productos      = _productos,
                Movimientos    = _movimientos.OrderByDescending(m => m.Id).ToList(),
                AlertasSinStock = _productos.Count(p => p.StockActual == 0),
                AlertasCriticas = _productos.Count(p => p.StockActual > 0 && p.StockActual <= p.StockMinimo)
            };
            return View(vm);
        }

        // ── POST: Agregar Producto ────────────────────────────────────────────
        [HttpPost]
        public IActionResult AgregarProducto([FromBody] ProductoStock nuevo)
        {
            if (nuevo == null || string.IsNullOrWhiteSpace(nuevo.Nombre) || string.IsNullOrWhiteSpace(nuevo.Codigo))
            {
                return Json(new { success = false, message = "Datos de producto inválidos." });
            }

            nuevo.Id = _productos.Any() ? _productos.Max(p => p.Id) + 1 : 1;
            _productos.Add(nuevo);

            // Log de movimiento
            _movimientos.Add(new MovimientoAlmacen
            {
                Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Producto = nuevo.Nombre,
                Tipo = "Entrada",
                Cantidad = nuevo.StockActual,
                Usuario = "Admin"
            });

            return Json(new { success = true, message = "Producto agregado con éxito y stock registrado." });
        }

        // ── POST: Editar Producto ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult EditarProducto([FromBody] ProductoStock editado)
        {
            var target = _productos.FirstOrDefault(p => p.Id == editado.Id);
            if (target == null) return Json(new { success = false, message = "Producto no encontrado." });

            target.Nombre = editado.Nombre;
            target.Codigo = editado.Codigo;
            target.Categoria = editado.Categoria;
            target.StockMinimo = editado.StockMinimo;
            target.Unidad = editado.Unidad;
            target.Lote = editado.Lote;
            target.FechaVencimiento = editado.FechaVencimiento;
            target.Almacen = editado.Almacen;
            target.NroSerie = editado.NroSerie;

            // Si el stock cambió, registrar movimiento
            if (target.StockActual != editado.StockActual)
            {
                int diff = editado.StockActual - target.StockActual;
                target.StockActual = editado.StockActual;

                _movimientos.Add(new MovimientoAlmacen
                {
                    Id = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1,
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                    Producto = target.Nombre,
                    Tipo = diff > 0 ? "Entrada" : "Salida",
                    Cantidad = Math.Abs(diff),
                    Usuario = "Admin"
                });
            }

            return Json(new { success = true, message = "Producto actualizado con éxito." });
        }

        // ── POST: Transferir Stock entre Almacenes ───────────────────────────
        [HttpPost]
        public IActionResult TransferirStock(int productoId, string origen, string destino, int cantidad)
        {
            var prod = _productos.FirstOrDefault(p => p.Id == productoId);
            if (prod == null) return Json(new { success = false, message = "Producto no encontrado." });

            if (prod.Almacen != origen)
            {
                return Json(new { success = false, message = $"El producto no se encuentra en el almacén de origen '{origen}'." });
            }

            if (prod.StockActual < cantidad)
            {
                return Json(new { success = false, message = $"Stock insuficiente en '{origen}'. Disponible: {prod.StockActual}." });
            }

            // Realizar transferencia:
            // 1. Reducir stock del origen
            prod.StockActual -= cantidad;

            // 2. Incrementar stock del destino (buscar si ya existe el mismo código en el almacén destino)
            var prodDestino = _productos.FirstOrDefault(p => p.Codigo == prod.Codigo && p.Almacen == destino);
            if (prodDestino != null)
            {
                prodDestino.StockActual += cantidad;
            }
            else
            {
                // Crear una copia del producto en el almacén destino
                var nuevoDest = new ProductoStock
                {
                    Id = _productos.Max(p => p.Id) + 1,
                    Codigo = prod.Codigo,
                    Nombre = prod.Nombre,
                    Categoria = prod.Categoria,
                    StockActual = cantidad,
                    StockMinimo = prod.StockMinimo,
                    Unidad = prod.Unidad,
                    Lote = prod.Lote,
                    FechaVencimiento = prod.FechaVencimiento,
                    Almacen = destino,
                    NroSerie = prod.NroSerie
                };
                _productos.Add(nuevoDest);
            }

            // Registrar movimientos de almacén
            int nextId = _movimientos.Any() ? _movimientos.Max(m => m.Id) + 1 : 1;
            _movimientos.Add(new MovimientoAlmacen
            {
                Id = nextId,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Producto = $"{prod.Nombre} (Salida por Transferencia)",
                Tipo = "Salida",
                Cantidad = cantidad,
                Usuario = "Logística"
            });
            _movimientos.Add(new MovimientoAlmacen
            {
                Id = nextId + 1,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Producto = $"{prod.Nombre} (Entrada por Transferencia)",
                Tipo = "Entrada",
                Cantidad = cantidad,
                Usuario = "Logística"
            });

            return Json(new { 
                success = true, 
                message = $"Transferencia de {cantidad} unidades completada de '{origen}' a '{destino}'." 
            });
        }

        // ── JsonResult: Stock por Categoría (Doughnut) ───────────────────────
        [HttpGet]
        public IActionResult GetStockPorCategoria()
        {
            var agrupado  = _productos.GroupBy(p => p.Categoria)
                                     .Select(g => new { Categoria = g.Key, Total = g.Sum(p => p.StockActual) })
                                     .ToList();
            return Json(new
            {
                labels = agrupado.Select(a => a.Categoria).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data            = agrupado.Select(a => a.Total).ToArray(),
                        backgroundColor = new[] { "#7c3aed","#06b6d4","#10b981","#f59e0b","#e85d9c","#ef4444" }
                    }
                }
            });
        }

        // ── JsonResult: Movimientos Entrada vs Salida ───
        [HttpGet]
        public IActionResult GetMovimientos()
        {
            // Retorna datos de movimientos reales agregados
            int entradas = _movimientos.Where(m => m.Tipo == "Entrada").Sum(m => m.Cantidad);
            int salidas = _movimientos.Where(m => m.Tipo == "Salida").Sum(m => m.Cantidad);

            return Json(new
            {
                labels = new[] { "Total Histórico" },
                datasets = new[]
                {
                    new { label="Entradas", data=new[]{entradas}, backgroundColor="#10b981" },
                    new { label="Salidas",  data=new[]{salidas}, backgroundColor="#ef4444" }
                }
            });
        }

        // ── JsonResult: KPI contadores (polling AJAX) ─────────────────────────
        [HttpGet]
        public IActionResult GetKpis()
        {
            return Json(new
            {
                totalProductos  = _productos.Count,
                sinStock        = _productos.Count(p => p.StockActual == 0),
                stockCritico    = _productos.Count(p => p.StockActual > 0 && p.StockActual <= p.StockMinimo),
                stockNormal     = _productos.Count(p => p.StockActual > p.StockMinimo),
                totalUnidades   = _productos.Sum(p => p.StockActual)
            });
        }
    }
}
