using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGE.Data;
using SGE.Extensions;
using SGE.Helpers;
using SGE.Models;
using SGE.ViewModels;

namespace SGE.Controllers;

public class ProveedoresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(ApplicationDbContext context, ILogger<ProveedoresController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(ProveedorListViewModel filtros)
    {
        try
        {
            await CargarListadoProveedoresAsync(filtros);
            return View(filtros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el directorio de proveedores.");
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            filtros.Proveedores = new List<Proveedor>();
            return View(filtros);
        }
    }

    public IActionResult Create()
    {
        return View(new Proveedor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Proveedor proveedor)
    {
        try
        {
            if (await _context.Proveedores.AnyAsync(p =>
                    p.TipoDocumento == proveedor.TipoDocumento &&
                    p.NumeroDocumento == proveedor.NumeroDocumento &&
                    p.Estado))
            {
                ModelState.AddModelError(nameof(Proveedor.NumeroDocumento), "Ya existe un proveedor con este documento.");
            }

            if (!ModelState.IsValid)
            {
                var errores = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));

                if (EsSolicitudAjax())
                    return BadRequest(new { success = false, message = errores });

                return View(proveedor);
            }

            proveedor.Estado = proveedor.Activo;

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(proveedor.NombreContacto))
            {
                _context.ContactosProveedores.Add(new ContactoProveedor
                {
                    ProveedorId = proveedor.ProveedorId,
                    Nombre = proveedor.NombreContacto.Trim(),
                    Cargo = proveedor.CargoContacto,
                    Telefono = proveedor.TelefonoContacto,
                    Email = proveedor.EmailContacto,
                    Estado = true
                });
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Proveedor creado correctamente.";

            if (EsSolicitudAjax())
                return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor.");
            var mensaje = DatabaseErrorHelper.ObtenerMensaje(ex);

            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = mensaje });

            TempData["Error"] = mensaje;
            return View(proveedor);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Proveedor proveedor)
    {
        if (id != proveedor.ProveedorId) return BadRequest();

        try
        {
            var db = await _context.Proveedores
                .Include(p => p.Contactos)
                .FirstOrDefaultAsync(p => p.ProveedorId == id && p.Estado);
            if (db is null) return NotFound();

            db.RazonSocial = proveedor.RazonSocial;
            db.TipoDocumento = proveedor.TipoDocumento;
            db.NumeroDocumento = proveedor.NumeroDocumento;
            db.DireccionFiscal = proveedor.DireccionFiscal;
            db.Email = proveedor.Email;
            db.Telefono = proveedor.Telefono;
            db.Estado = proveedor.Activo;

            await ActualizarContactoPrincipalAsync(db, proveedor);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Proveedor actualizado correctamente.";

            if (EsSolicitudAjax()) return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar proveedor {id}.", id);
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = DatabaseErrorHelper.ObtenerMensaje(ex) });
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound();

            proveedor.Estado = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Proveedor archivado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al archivar proveedor {id}.", id);
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListadoProveedoresAsync(ProveedorListViewModel filtros)
    {
        filtros.Pagina = filtros.Pagina < 1 ? 1 : filtros.Pagina;
        filtros.RegistrosPorPagina = new[] { 10, 25, 50, 100 }.Contains(filtros.RegistrosPorPagina)
            ? filtros.RegistrosPorPagina : 10;

        var consulta = _context.Proveedores
            .AsNoTracking()
            .Include(p => p.Contactos)
            .Where(p => p.Estado);

        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var t = filtros.Busqueda.Trim().ToLower();
            consulta = consulta.Where(p =>
                p.RazonSocial.ToLower().Contains(t) ||
                p.NumeroDocumento.Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(filtros.FiltroEstado))
        {
            var activo = filtros.FiltroEstado == "Activo";
            consulta = consulta.Where(p => p.Estado == activo);
        }

        filtros.TotalRegistros = await consulta.CountAsync();
        var proveedores = await consulta
            .OrderBy(p => p.RazonSocial)
            .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
            .Take(filtros.RegistrosPorPagina)
            .ToListAsync();

        foreach (var proveedor in proveedores)
        {
            MapearContactoPrincipal(proveedor);
        }

        filtros.Proveedores = proveedores;
        filtros.Kpis = await ObtenerKpisAsync();
    }

    private async Task<ProveedorKpiViewModel> ObtenerKpisAsync()
    {
        var proveedores = await _context.Proveedores.AsNoTracking().Where(p => p.Estado).CountAsync();
        var contactos = await _context.ContactosProveedores.AsNoTracking().CountAsync(c => c.Estado);

        return new ProveedorKpiViewModel
        {
            TotalProveedores = proveedores,
            ProveedoresActivos = proveedores,
            TotalContactos = contactos,
            ComprasTotales = 125000m
        };
    }

    private static void MapearContactoPrincipal(Proveedor proveedor)
    {
        var contacto = proveedor.Contactos.FirstOrDefault(c => c.Estado);
        if (contacto is null) return;

        proveedor.NombreContacto = contacto.Nombre;
        proveedor.CargoContacto = contacto.Cargo;
        proveedor.TelefonoContacto = contacto.Telefono;
        proveedor.EmailContacto = contacto.Email;
    }

    private async Task ActualizarContactoPrincipalAsync(Proveedor db, Proveedor proveedor)
    {
        if (string.IsNullOrWhiteSpace(proveedor.NombreContacto))
        {
            return;
        }

        var contacto = db.Contactos.FirstOrDefault(c => c.Estado);
        if (contacto is null)
        {
            _context.ContactosProveedores.Add(new ContactoProveedor
            {
                ProveedorId = db.ProveedorId,
                Nombre = proveedor.NombreContacto.Trim(),
                Cargo = proveedor.CargoContacto,
                Telefono = proveedor.TelefonoContacto,
                Email = proveedor.EmailContacto,
                Estado = true
            });
            return;
        }

        contacto.Nombre = proveedor.NombreContacto.Trim();
        contacto.Cargo = proveedor.CargoContacto;
        contacto.Telefono = proveedor.TelefonoContacto;
        contacto.Email = proveedor.EmailContacto;
    }

    private bool EsSolicitudAjax() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
