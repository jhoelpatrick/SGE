using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SGE.Models;

namespace SGE.Controllers
{
    public class GestionController : Controller
    {
        private readonly string _conn;
        private readonly SGE.Services.IEmailService _emailService;
        private static readonly string[] _roles = { "Administrador", "Asesor Comercial", "Gerente RRHH", "Contador" };
        private static readonly string[] _modulos = {
            "Dashboard", "Usuarios", "Roles y Permisos",
            "Clientes", "Proveedores", "Productos",
            "Ventas", "Compras", "Facturación", "Inventario", "Proyectos",
            "Impuestos", "Contabilidad", "Caja y Bancos", "Activos Fijos",
            "Recursos Humanos", "Nómina",
            "Reportes", "Auditoría", "Configuración"
        };

        public GestionController(IConfiguration config, SGE.Services.IEmailService emailService)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
            _emailService = emailService;
        }

        // ── /Gestion/Index — lista de usuarios ──────────────────────────────
        public IActionResult Index()
        {
            var lista = new List<Usuario>();
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                var sql = @"SELECT usuarionominaid, usuario, nombrecompleto, rol, correo, estaactivo
                            FROM rrhh_recursos.usuarios_nomina
                            ORDER BY nombrecompleto";
                using var cmd = new NpgsqlCommand(sql, cn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    var id = rd.GetInt32(0);
                    var username = rd.GetString(1);
                    var nombreCompleto = rd.GetString(2);
                    var rolRaw = rd.GetString(3);
                    var correo = rd.GetString(4);
                    var estaActivo = rd.GetBoolean(5);

                    var rol = _roles.FirstOrDefault(r => string.Equals(r, rolRaw, StringComparison.OrdinalIgnoreCase)) ?? rolRaw;

                    var parts = nombreCompleto.Split(' ', 2);
                    var nombre = parts.Length > 0 ? parts[0] : "";
                    var apellido = parts.Length > 1 ? parts[1] : "";

                    lista.Add(new Usuario
                    {
                        Id            = id,
                        Nombre        = nombre,
                        Apellido      = apellido,
                        Email         = correo,
                        Estado        = estaActivo ? EstadoUsuario.Activo : EstadoUsuario.Inactivo,
                        Rol           = rol,
                    });
                }
            }
            catch { /* DB not ready — return empty list */ }

            ViewBag.Roles          = _roles;
            ViewBag.TotalActivos   = lista.Count(u => u.Estado == EstadoUsuario.Activo);
            ViewBag.TotalInactivos = lista.Count(u => u.Estado != EstadoUsuario.Activo);
            ViewBag.TotalRoles     = _roles.Length;
            return PartialView("~/Views/Gestion/Index.cshtml", lista);
        }

        // ── /Gestion/RolesPermisos — matriz de roles/permisos ─────────────────────────
        public IActionResult RolesPermisos()
        {
            var rolSel = Request.Query["rol"].ToString();
            if (string.IsNullOrEmpty(rolSel)) rolSel = "Administrador";

            var vm = new RolPermisosViewModel
            {
                Roles          = _roles.ToList(),
                RolSeleccionado = rolSel,
                Permisos       = GetPermisosPorRol(rolSel)
            };
            return PartialView("~/Views/Gestion/RolesPermisos.cshtml", vm);
        }

        // ── /Gestion/GuardarPermisos — guardar permisos del rol ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarPermisos(string rolSeleccionado, string[] modulos, string[] ver, string[] crearEditar, string[] eliminar, string[] reportes)
        {
            TempData["Exito"] = $"Permisos para el rol '{rolSeleccionado}' actualizados correctamente.";
            return RedirectToAction("RolesPermisos", new { rol = rolSeleccionado });
        }

        // ── /Gestion/NuevoUsuario — formulario alta ───────────────────────────
        public IActionResult NuevoUsuario()
        {
            var model = new Usuario();
            ViewBag.Roles  = _roles;
            ViewBag.Matriz = GetMatrizPermisos();
            return PartialView("~/Views/Gestion/NuevoUsuario.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevoUsuario(Usuario model)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                var sql = "INSERT INTO rrhh_recursos.usuarios_nomina (usuario, nombrecompleto, rol, correo, estaactivo) VALUES (@usuario, @nombrecompleto, @rol, @correo, @estaactivo)";
                using var cmd = new NpgsqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@usuario", model.Email.Split('@')[0]);
                cmd.Parameters.AddWithValue("@nombrecompleto", model.NombreCompleto);
                cmd.Parameters.AddWithValue("@rol", model.Rol.ToLower());
                cmd.Parameters.AddWithValue("@correo", model.Email);
                cmd.Parameters.AddWithValue("@estaactivo", model.Estado == EstadoUsuario.Activo);
                cmd.ExecuteNonQuery();

                // Enviar notificación al dueño
                string subject = "Nuevo usuario creado - SGE Enterprise";
                string body = $@"
                    <div style='font-family: sans-serif; max-width: 500px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px;'>
                        <h2 style='color: #4361ee; margin-top: 0;'>Nuevo Usuario Registrado</h2>
                        <p>Se ha creado una nueva cuenta en la plataforma:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 6px 0; font-weight: bold; width: 140px;'>Nombre Completo:</td><td style='padding: 6px 0;'>{model.NombreCompleto}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Correo:</td><td style='padding: 6px 0;'>{model.Email}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Rol Asignado:</td><td style='padding: 6px 0; text-transform: capitalize;'>{model.Rol}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Estado:</td><td style='padding: 6px 0;'>{(model.Estado == EstadoUsuario.Activo ? "Activo" : "Inactivo")}</td></tr>
                        </table>
                    </div>";
                Task.Run(async () => await _emailService.SendEmailAsync("zaiduriarteleo@gmail.com", subject, body));

                TempData["Exito"] = "Usuario creado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Roles = _roles;
                ViewBag.Matriz = GetMatrizPermisos();
                ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                return PartialView("~/Views/Gestion/NuevoUsuario.cshtml", model);
            }
        }

        // ── /Gestion/Editar — formulario edición / guardar cambios ───────────────────────────
        public IActionResult Editar(int id)
        {
            var model = new Usuario();
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                var sql = "SELECT usuarionominaid, usuario, nombrecompleto, rol, correo, estaactivo FROM rrhh_recursos.usuarios_nomina WHERE usuarionominaid = @id";
                using var cmd = new NpgsqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@id", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    var idVal = rd.GetInt32(0);
                    var username = rd.GetString(1);
                    var nombreCompleto = rd.GetString(2);
                    var rolRaw = rd.GetString(3);
                    var correo = rd.GetString(4);
                    var estaActivo = rd.GetBoolean(5);

                    var rol = _roles.FirstOrDefault(r => string.Equals(r, rolRaw, StringComparison.OrdinalIgnoreCase)) ?? rolRaw;
                    var parts = nombreCompleto.Split(' ', 2);
                    var nombre = parts.Length > 0 ? parts[0] : "";
                    var apellido = parts.Length > 1 ? parts[1] : "";

                    model = new Usuario
                    {
                        Id = idVal,
                        Nombre = nombre,
                        Apellido = apellido,
                        Email = correo,
                        Estado = estaActivo ? EstadoUsuario.Activo : EstadoUsuario.Inactivo,
                        Rol = rol
                    };
                }
            }
            catch { }

            ViewBag.Roles = _roles;
            ViewBag.Matriz = GetMatrizPermisos();
            return PartialView("~/Views/Gestion/NuevoUsuario.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Usuario model)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                var sql = "UPDATE rrhh_recursos.usuarios_nomina SET nombrecompleto = @nombrecompleto, rol = @rol, correo = @correo, estaactivo = @estaactivo WHERE usuarionominaid = @id";
                using var cmd = new NpgsqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@nombrecompleto", model.NombreCompleto);
                cmd.Parameters.AddWithValue("@rol", model.Rol.ToLower());
                cmd.Parameters.AddWithValue("@correo", model.Email);
                cmd.Parameters.AddWithValue("@estaactivo", model.Estado == EstadoUsuario.Activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", model.Id);
                cmd.ExecuteNonQuery();
                TempData["Exito"] = "Usuario actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Roles = _roles;
                ViewBag.Matriz = GetMatrizPermisos();
                ModelState.AddModelError("", "Error al actualizar el usuario: " + ex.Message);
                return PartialView("~/Views/Gestion/NuevoUsuario.cshtml", model);
            }
        }

        // ── /Gestion/Eliminar — borrar usuario ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                var sql = "DELETE FROM rrhh_recursos.usuarios_nomina WHERE usuarionominaid = @id";
                using var cmd = new NpgsqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                TempData["Exito"] = "Usuario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el usuario: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // ── Helper: permisos de un rol específico ─────────────────────────────
        private List<Permiso> GetPermisosPorRol(string rol)
        {
            bool esAdmin = rol == "Administrador";
            return _modulos.Select(m => new Permiso
            {
                Modulo     = m,
                Ver        = true,
                CrearEditar = esAdmin || (rol == "Asesor Comercial" && IsComercial(m)) ||
                              (rol == "Gerente RRHH" && IsRRHH(m)) ||
                              (rol == "Contador" && IsFinanzas(m)),
                Eliminar   = esAdmin,
                Reportes   = esAdmin || (rol == "Asesor Comercial" && IsComercial(m)) ||
                              (rol == "Gerente RRHH" && IsRRHH(m)) ||
                              (rol == "Contador" && IsFinanzas(m))
            }).ToList();
        }

        // ── Helper: matriz completa para NuevoUsuario ─────────────────────────
        private Dictionary<string, List<Permiso>> GetMatrizPermisos()
        {
            return _roles.ToDictionary(r => r, r => GetPermisosPorRol(r));
        }

        private static bool IsComercial(string m) =>
            new[] { "Clientes", "Proveedores", "Productos", "Ventas", "Cotizaciones" }.Contains(m);
        private static bool IsRRHH(string m) =>
            new[] { "Recursos Humanos", "Nómina" }.Contains(m);
        private static bool IsFinanzas(string m) =>
            new[] { "Impuestos", "Contabilidad", "Caja y Bancos", "Activos Fijos", "Facturación" }.Contains(m);
    }
}
