using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers
{
    /// <summary>
    /// Controlador del módulo Comercial.
    /// Acciones MVC (GET) → devuelven las PartialViews con el diseño ya existente.
    /// Acciones API JSON → permiten al JavaScript de las vistas sincronizar datos con SQL Server.
    /// </summary>
    public class ComercialController : Controller
    {
        private readonly IClienteRepository   _clientes;
        private readonly IProductoRepository  _productos;
        private readonly IProveedorRepository _proveedores;

        public ComercialController(
            IClienteRepository   clientes,
            IProductoRepository  productos,
            IProveedorRepository proveedores)
        {
            _clientes    = clientes;
            _productos   = productos;
            _proveedores = proveedores;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── VISTAS PARCIALES (devuelven el HTML del módulo) ──────────────────
        // ══════════════════════════════════════════════════════════════════════

        public IActionResult Clientes()    => PartialView();
        public IActionResult Productos()   => PartialView();
        public IActionResult Proveedores() => PartialView();

        // ══════════════════════════════════════════════════════════════════════
        // ── API JSON: CLIENTES ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /Comercial/GetClientesJson — devuelve la lista completa de clientes.</summary>
        [HttpGet]
        public async Task<IActionResult> GetClientesJson()
        {
            try
            {
                var lista = await _clientes.GetAllAsync();
                return Json(new { ok = true, data = lista });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        /// <summary>POST /Comercial/CreateCliente — crea un nuevo cliente desde el formulario del modal.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCliente([FromBody] Cliente model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                var newId = await _clientes.CreateAsync(model);
                return Json(new { ok = true, id = newId, mensaje = "Cliente registrado exitosamente." });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Violación de UNIQUE (tipodocumento + numerodocumento)
                return Json(new { ok = false, error = "Ya existe un cliente con ese número de documento." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/UpdateCliente — actualiza un cliente existente.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCliente([FromBody] Cliente model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                await _clientes.UpdateAsync(model);
                return Json(new { ok = true, mensaje = "Cliente actualizado exitosamente." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/DeleteCliente — elimina un cliente.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCliente([FromBody] int id)
        {
            try
            {
                await _clientes.DeleteAsync(id);
                return Json(new { ok = true, mensaje = "Cliente eliminado." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/ToggleClienteEstado — activa o desactiva el estado de un cliente.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleClienteEstado([FromBody] ToggleEstadoRequest request)
        {
            try
            {
                await _clientes.ToggleEstadoAsync(request.Id, request.Estado);
                return Json(new { ok = true });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API JSON: PRODUCTOS ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /Comercial/GetProductosJson — devuelve la lista completa de productos.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProductosJson()
        {
            try
            {
                var lista = await _productos.GetAllAsync();
                return Json(new { ok = true, data = lista });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/CreateProducto — crea un nuevo producto.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProducto([FromBody] Producto model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                var newId = await _productos.CreateAsync(model);
                return Json(new { ok = true, id = newId, mensaje = "Producto registrado exitosamente." });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { ok = false, error = "Ya existe un producto con ese código SKU." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/UpdateProducto — actualiza un producto existente.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProducto([FromBody] Producto model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                await _productos.UpdateAsync(model);
                return Json(new { ok = true, mensaje = "Producto actualizado exitosamente." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/DeleteProducto — elimina un producto.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProducto([FromBody] int id)
        {
            try
            {
                await _productos.DeleteAsync(id);
                return Json(new { ok = true, mensaje = "Producto eliminado." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/ToggleProductoEstado — activa o desactiva el estado de un producto.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductoEstado([FromBody] ToggleEstadoRequest request)
        {
            try
            {
                await _productos.ToggleEstadoAsync(request.Id, request.Estado);
                return Json(new { ok = true });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API JSON: PROVEEDORES ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>GET /Comercial/GetProveedoresJson — devuelve la lista completa de proveedores.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProveedoresJson()
        {
            try
            {
                var lista = await _proveedores.GetAllAsync();
                return Json(new { ok = true, data = lista });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/CreateProveedor — crea un nuevo proveedor.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProveedor([FromBody] Proveedor model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                var newId = await _proveedores.CreateAsync(model);
                return Json(new { ok = true, id = newId, mensaje = "Proveedor registrado exitosamente." });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Json(new { ok = false, error = "Ya existe un proveedor con ese número de documento." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/UpdateProveedor — actualiza un proveedor existente.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProveedor([FromBody] Proveedor model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { ok = false, errores });
            }

            try
            {
                await _proveedores.UpdateAsync(model);
                return Json(new { ok = true, mensaje = "Proveedor actualizado exitosamente." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/DeleteProveedor — elimina un proveedor.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProveedor([FromBody] int id)
        {
            try
            {
                await _proveedores.DeleteAsync(id);
                return Json(new { ok = true, mensaje = "Proveedor eliminado." });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }

        /// <summary>POST /Comercial/ToggleProveedorEstado — activa o desactiva el estado de un proveedor.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProveedorEstado([FromBody] ToggleEstadoRequest request)
        {
            try
            {
                await _proveedores.ToggleEstadoAsync(request.Id, request.Estado);
                return Json(new { ok = true });
            }
            catch (SqlException ex)
            {
                return Json(new { ok = false, error = $"Error de base de datos: {ex.Message}" });
            }
        }
    }

    /// <summary>DTO para las acciones de toggle de estado (compartido por Clientes, Productos y Proveedores).</summary>
    public class ToggleEstadoRequest
    {
        public int  Id     { get; set; }
        public bool Estado { get; set; }
    }
}
