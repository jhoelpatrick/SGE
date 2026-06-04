using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers
{
    public class OperacionesController : Controller
    {
        private readonly IProyectoRepository _proyectos;
        private readonly IVentaRepository _ventas;
        private readonly ICompraRepository _compras;
        private readonly IFacturacionRepository _facturacion;
        private readonly IInventarioRepository _inventario;
        private readonly IClienteRepository _clientes;
        private readonly IProveedorRepository _proveedores;
        private readonly IConfiguration _configuration;

        public OperacionesController(
            IProyectoRepository proyectos,
            IVentaRepository ventas,
            ICompraRepository compras,
            IFacturacionRepository facturacion,
            IInventarioRepository inventario,
            IClienteRepository clientes,
            IProveedorRepository proveedores,
            IConfiguration configuration)
        {
            _proyectos = proyectos;
            _ventas = ventas;
            _compras = compras;
            _facturacion = facturacion;
            _inventario = inventario;
            _clientes = clientes;
            _proveedores = proveedores;
            _configuration = configuration;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── ACCIONES PARA CARGAR LAS VISTAS PARCIALES ─────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        public IActionResult Ventas() => PartialView();
        public IActionResult Compras() => PartialView();
        public IActionResult Facturacion() => PartialView();
        public IActionResult Inventario() => PartialView();
        public IActionResult Proyectos() => PartialView();

        // ══════════════════════════════════════════════════════════════════════
        // ── API: PROYECTOS ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetProyectosJson()
        {
            try
            {
                var list = await _proyectos.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProyecto([FromBody] Proyecto model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreProyecto))
                return Json(new { ok = false, error = "El nombre del proyecto es obligatorio." });

            try
            {
                var newId = await _proyectos.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTareasJson(int id)
        {
            try
            {
                var list = await _proyectos.GetTareasByProyectoIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTarea([FromBody] ProyectoTarea model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreTarea))
                return Json(new { ok = false, error = "El nombre de la tarea es obligatorio." });

            try
            {
                var newId = await _proyectos.CreateTareaAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTareaEstado([FromBody] UpdateTareaEstadoRequest req)
        {
            try
            {
                await _proyectos.UpdateTareaEstadoAsync(req.TareaId, req.PorcentajeProgreso, req.Estado);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: VENTAS ───────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetVentasJson()
        {
            try
            {
                var list = await _ventas.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVentaDetalleJson(int id)
        {
            try
            {
                var list = await _ventas.GetDetalleByPedidoIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePedido([FromBody] PedidoVenta model)
        {
            try
            {
                var newId = await _ventas.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePedido([FromBody] int id)
        {
            try
            {
                await _ventas.ApproveAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPedido([FromBody] int id)
        {
            try
            {
                await _ventas.CancelAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchPedido([FromBody] DispatchPedidoRequest req)
        {
            try
            {
                await _ventas.DispatchAsync(req.PedId, req.VehId, req.CondId, req.Serie, req.Correlativo);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: COMPRAS ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetComprasJson()
        {
            try
            {
                var list = await _compras.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompraDetalleJson(int id)
        {
            try
            {
                var list = await _compras.GetDetalleByOrdenIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompra([FromBody] OrdenCompra model)
        {
            try
            {
                var newId = await _compras.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCompra([FromBody] int id)
        {
            try
            {
                await _compras.ApproveAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCompra([FromBody] int id)
        {
            try
            {
                await _compras.RejectAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: FACTURACIÓN ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetComprobantesJson()
        {
            try
            {
                var list = await _facturacion.GetAllInvoicesAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGuiasJson()
        {
            try
            {
                var list = await _facturacion.GetAllGuidesAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest req)
        {
            try
            {
                var compId = await _facturacion.EmitirFacturaDesdePedidoAsync(req.PedidoId, req.TipoComprobante, req.Serie);
                return Json(new { ok = true, id = compId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingBillingOrders()
        {
            try
            {
                var list = await _facturacion.GetPendingBillingOrdersAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiculosJson()
        {
            try
            {
                var list = await GetVehiculosAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConductoresJson()
        {
            try
            {
                var list = await GetConductoresAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: INVENTARIO ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetStockJson()
        {
            try
            {
                var list = await _inventario.GetStockSummaryAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKardexJson(int productoId)
        {
            try
            {
                var list = await _inventario.GetKardexByProductoIdAsync(productoId);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMovimientoManual([FromBody] ManualMovementRequest req)
        {
            try
            {
                await _inventario.RegistrarMovimientoManualAsync(req.ProductoId, req.TipoMovimiento, req.Cantidad, req.Motivo);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── HELPERS INTERNOS ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private async Task<List<Vehiculo>> GetVehiculosAsync()
        {
            var list = new List<Vehiculo>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var cn = new NpgsqlConnection(connectionString);
            await cn.OpenAsync();
            const string sql = @"
                SELECT v.vehiculoid, v.proveedorid, v.placa, v.marca, v.modelo, v.tipovehiculo, v.estado, p.razonsocial
                FROM   comercial.vehiculosproveedores v
                INNER JOIN comercial.proveedores p ON v.proveedorid = p.proveedorid
                WHERE  v.estado = 1";
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new Vehiculo
                {
                    VehiculoId = rd.GetInt32(0),
                    ProveedorId = rd.GetInt32(1),
                    Placa = rd.GetString(2),
                    Marca = rd.IsDBNull(3) ? "" : rd.GetString(3),
                    Modelo = rd.IsDBNull(4) ? "" : rd.GetString(4),
                    TipoVehiculo = rd.IsDBNull(5) ? "" : rd.GetString(5),
                    Estado = rd.GetBoolean(6),
                    ProveedorNombre = rd.GetString(7)
                });
            }
            return list;
        }

        private async Task<List<Conductor>> GetConductoresAsync()
        {
            var list = new List<Conductor>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var cn = new NpgsqlConnection(connectionString);
            await cn.OpenAsync();
            const string sql = @"
                SELECT conductorid, proveedorid, nombre, tipodocumento, numerodocumento, licenciaconducir, estado
                FROM   comercial.conductoresproveedores
                WHERE  estado = 1";
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new Conductor
                {
                    ConductorId = rd.GetInt32(0),
                    ProveedorId = rd.GetInt32(1),
                    Nombre = rd.GetString(2),
                    NumeroDocumento = rd.GetString(4),
                    LicenciaConducir = rd.IsDBNull(5) ? "" : rd.GetString(5),
                    Estado = rd.GetBoolean(6)
                });
            }
            return list;
        }
    }

    // DTO classes for request body binding
    public class UpdateTareaEstadoRequest
    {
        public int TareaId { get; set; }
        public decimal PorcentajeProgreso { get; set; }
        public string Estado { get; set; } = "";
    }

    public class DispatchPedidoRequest
    {
        public int PedId { get; set; }
        public int VehId { get; set; }
        public int CondId { get; set; }
        public string Serie { get; set; } = "";
        public string Correlativo { get; set; } = "";
    }

    public class EmitirFacturaRequest
    {
        public int PedidoId { get; set; }
        public string TipoComprobante { get; set; } = "01";
        public string Serie { get; set; } = "F001";
    }

    public class ManualMovementRequest
    {
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string ContextoReferencia { get; set; } = "";
        public string Motivo { get; set; } = "";
    }
}
