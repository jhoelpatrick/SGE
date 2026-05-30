using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;
using SGE.ViewModels;

namespace SGE.Controllers;

public class GestionUsuariosController : Controller
{
    private readonly UsuariosService  _usuarios;
    private readonly PermisosService  _permisos;

    public GestionUsuariosController(UsuariosService usuarios, PermisosService permisos)
    {
        _usuarios = usuarios;
        _permisos = permisos;
    }

    // ── LISTA / DASHBOARD ────────────────────────────────────────────────────

    public IActionResult Index(string? buscar, string? rol, string? estado)
    {
        var lista = _usuarios.ObtenerTodos().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(buscar))
            lista = lista.Where(u =>
                u.NombreCompleto.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                u.Rol.Contains(buscar, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(rol) && rol != "Todos")
            lista = lista.Where(u => u.Rol == rol);

        if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
        {
            var estadoEnum = estado == "Activo" ? EstadoUsuario.Activo : EstadoUsuario.Inactivo;
            lista = lista.Where(u => u.Estado == estadoEnum);
        }

        var todos = _usuarios.ObtenerTodos();
        ViewBag.TotalActivos   = todos.Count(u => u.Estado == EstadoUsuario.Activo);
        ViewBag.TotalInactivos = todos.Count(u => u.Estado == EstadoUsuario.Inactivo);
        ViewBag.TotalRoles     = SistemaRoles.Lista.Length;
        ViewBag.Roles          = SistemaRoles.Lista;

        if (TempData["Exito"] != null)
            ViewBag.Exito = TempData["Exito"];

        return View(lista.ToList());
    }

    // ── CREAR USUARIO ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult NuevoUsuario()
    {
        ViewBag.Roles  = SistemaRoles.Lista;
        ViewBag.Matriz = _permisos.ObtenerMatriz();
        return View(new Usuario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoUsuario(Usuario usuario)
    {
        ModelState.Remove("FechaCreacion");
        if (!ModelState.IsValid)
        {
            ViewBag.Roles  = SistemaRoles.Lista;
            ViewBag.Matriz = _permisos.ObtenerMatriz();
            return View(usuario);
        }

        _usuarios.Crear(usuario);
        TempData["Exito"] = $"Usuario {usuario.NombreCompleto} creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── EDITAR USUARIO ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Editar(int id)
    {
        var usuario = _usuarios.ObtenerPorId(id);
        if (usuario is null) return NotFound();

        ViewBag.Roles  = SistemaRoles.Lista;
        ViewBag.Matriz = _permisos.ObtenerMatriz();
        return View("NuevoUsuario", usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(Usuario usuario)
    {
        ModelState.Remove("FechaCreacion");
        ModelState.Remove("Contrasena");
        if (!ModelState.IsValid)
        {
            ViewBag.Roles  = SistemaRoles.Lista;
            ViewBag.Matriz = _permisos.ObtenerMatriz();
            return View("NuevoUsuario", usuario);
        }

        // Preservar FechaCreacion y contraseña si viene vacía
        var original = _usuarios.ObtenerPorId(usuario.Id);
        if (original != null)
        {
            usuario.FechaCreacion = original.FechaCreacion;
            if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                usuario.Contrasena = original.Contrasena;
        }

        _usuarios.Editar(usuario);
        TempData["Exito"] = $"Usuario {usuario.NombreCompleto} actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── ELIMINAR USUARIO ─────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        _usuarios.Eliminar(id);
        TempData["Exito"] = "Usuario eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── CAMBIAR ESTADO ───────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id)
    {
        _usuarios.CambiarEstado(id);
        return RedirectToAction(nameof(Index));
    }

    // ── ROLES Y PERMISOS ─────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult RolesPermisos(string rol = "Administrador")
    {
        var vm = new RolPermisosViewModel
        {
            RolSeleccionado = rol,
            Roles           = SistemaRoles.Lista.ToList(),
            Permisos        = _permisos.ObtenerPorRol(rol),
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarPermisos(
        string        rolSeleccionado,
        List<string>  modulos,
        List<string>? ver,
        List<string>? crearEditar,
        List<string>? eliminar,
        List<string>? reportes)
    {
        var permisos = modulos.Select(m => new Permiso
        {
            Modulo      = m,
            Ver         = ver         != null && ver.Contains(m),
            CrearEditar = crearEditar != null && crearEditar.Contains(m),
            Eliminar    = eliminar    != null && eliminar.Contains(m),
            Reportes    = reportes    != null && reportes.Contains(m),
        }).ToList();

        _permisos.GuardarPermisos(rolSeleccionado, permisos);
        TempData["Exito"] = $"Permisos del rol \"{rolSeleccionado}\" guardados correctamente.";
        return RedirectToAction(nameof(RolesPermisos), new { rol = rolSeleccionado });
    }
}
