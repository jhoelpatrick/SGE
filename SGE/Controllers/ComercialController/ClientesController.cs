using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGE.Data;
using SGE.Extensions;
using SGE.Helpers;
using SGE.Models;
using SGE.ViewModels;

namespace SGE.Controllers;

public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ApplicationDbContext context, ILogger<ClientesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(ClienteListViewModel filtros)
    {
        try
        {
            await CargarListadoClientesAsync(filtros);
            return View(filtros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el directorio de clientes.");
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            filtros.Clientes = new List<Cliente>();
            return View(filtros);
        }
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cliente.TipoDocumento))
                ModelState.AddModelError(nameof(Cliente.TipoDocumento), "Seleccione el tipo de documento.");

            if (string.IsNullOrWhiteSpace(cliente.NumeroDocumento))
                ModelState.AddModelError(nameof(Cliente.NumeroDocumento), "Ingrese el numero de documento.");

            if (string.IsNullOrWhiteSpace(cliente.RazonSocial))
                ModelState.AddModelError(nameof(Cliente.RazonSocial), "Ingrese la razon social.");

            if (await _context.Clientes.AnyAsync(c =>
                    c.TipoDocumento == cliente.TipoDocumento &&
                    c.NumeroDocumento == cliente.NumeroDocumento &&
                    c.Estado))
            {
                ModelState.AddModelError(nameof(Cliente.NumeroDocumento), "Ya existe un cliente con este documento.");
            }

            if (!ModelState.IsValid)
            {
                var errores = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));

                if (EsSolicitudAjax())
                    return BadRequest(new { success = false, message = errores });

                return View(cliente);
            }

            cliente.FechaRegistro = DateTime.Now;
            cliente.Estado = cliente.Activo;
            cliente.TipoCliente = string.IsNullOrWhiteSpace(cliente.TipoCliente) ? "prospecto" : cliente.TipoCliente;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente creado correctamente.";

            if (EsSolicitudAjax())
                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente.");
            var mensaje = DatabaseErrorHelper.ObtenerMensaje(ex);

            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = mensaje });

            TempData["Error"] = mensaje;
            return View(cliente);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.ClienteId) return BadRequest();

        try
        {
            var db = await _context.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id && c.Estado);
            if (db is null) return NotFound();

            db.RazonSocial = cliente.RazonSocial;
            db.NombreComercial = cliente.NombreComercial;
            db.TipoDocumento = cliente.TipoDocumento;
            db.NumeroDocumento = cliente.NumeroDocumento;
            db.DireccionFiscal = cliente.DireccionFiscal;
            db.Email = cliente.Email;
            db.Telefono = cliente.Telefono;
            db.Estado = cliente.Activo;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cliente actualizado correctamente.";

            if (EsSolicitudAjax()) return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar cliente {id}.", id);
            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = DatabaseErrorHelper.ObtenerMensaje(ex) });

            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null) return NotFound();

            cliente.Estado = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cliente archivado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al archivar cliente {id}.", id);
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Historial() => View();

    public IActionResult Contactos() => View();

    private async Task CargarListadoClientesAsync(ClienteListViewModel filtros)
    {
        filtros.Pagina = filtros.Pagina < 1 ? 1 : filtros.Pagina;
        filtros.RegistrosPorPagina = new[] { 10, 25, 50, 100 }.Contains(filtros.RegistrosPorPagina)
            ? filtros.RegistrosPorPagina : 10;

        var consulta = _context.Clientes.AsNoTracking().Where(c => c.Estado);

        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var t = filtros.Busqueda.Trim().ToLower();
            consulta = consulta.Where(c =>
                c.RazonSocial.ToLower().Contains(t) ||
                c.NumeroDocumento.Contains(t) ||
                (c.NombreComercial != null && c.NombreComercial.ToLower().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(filtros.FiltroEstado))
        {
            var activo = filtros.FiltroEstado == "Activo";
            consulta = consulta.Where(c => c.Estado == activo);
        }

        filtros.TotalRegistros = await consulta.CountAsync();
        filtros.Clientes = await consulta
            .OrderBy(c => c.RazonSocial)
            .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
            .Take(filtros.RegistrosPorPagina)
            .ToListAsync();

        filtros.Kpis = await ObtenerKpisAsync();
    }

    private async Task<ClienteKpiViewModel> ObtenerKpisAsync()
    {
        var clientes = await _context.Clientes.AsNoTracking().Where(c => c.Estado).ToListAsync();
        var totalContactos = await _context.ContactosClientes.AsNoTracking().CountAsync(c => c.Estado);

        return new ClienteKpiViewModel
        {
            TotalClientes = clientes.Count,
            ClientesActivos = clientes.Count(c => c.Estado),
            TotalContactos = totalContactos,
            VentasTotales = 85000m
        };
    }

    private bool EsSolicitudAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
