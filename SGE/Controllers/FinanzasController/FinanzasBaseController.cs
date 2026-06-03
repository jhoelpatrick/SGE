using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

[Route("Finanzas")]
public abstract class FinanzasBaseController : Controller
{
    protected readonly IFinanzasDataService FinanzasData;
    private static readonly string[] RolesValidos = { "Administrador", "Asesor Comercial", "Gerente RRHH", "Contador" };

    protected FinanzasBaseController(IFinanzasDataService finanzasData)
    {
        FinanzasData = finanzasData;
    }

    protected void PrepararPermisosFinanzas(string modulo)
    {
        var rol = ResolverRolActual();
        var esAdministrador = rol == "Administrador";
        var esContador = rol == "Contador";

        ViewBag.RolActual = rol;
        ViewBag.ModuloFinanzasActual = modulo;
        ViewBag.FinanzasCanView = true;
        ViewBag.FinanzasCanWrite = esAdministrador || esContador;
        ViewBag.FinanzasCanDelete = esAdministrador;
        ViewBag.FinanzasCanReport = esAdministrador || esContador;
    }

    protected bool PuedeEditarFinanzas()
    {
        var rol = ResolverRolActual();
        return rol == "Administrador" || rol == "Contador";
    }

    protected bool PuedeReportarFinanzas()
    {
        var rol = ResolverRolActual();
        return rol == "Administrador" || rol == "Contador";
    }

    protected IActionResult DenegarOperacion(string redirectUrl)
    {
        TempData["Error"] = "Tu rol actual no tiene permisos para realizar esta accion en Finanzas.";
        return Redirect(redirectUrl);
    }

    protected IActionResult ErrorPartial<TModel>(string viewName, TModel model, Exception exception, ILogger logger)
    {
        logger.LogError(exception, "No se pudo cargar el modulo de finanzas {ViewName}.", viewName);
        TempData["Error"] = "No se pudo cargar la informacion de la base de datos. Revisa la conexion o intenta nuevamente.";
        return PartialView($"~/Views/Finanzas/{viewName}.cshtml", model);
    }

    protected static List<LibroDiarioFinanciero> BuildLibroDiario(FinanzasDataStore data)
    {
        return data.AsientosDetalle
            .Join(data.AsientosCabecera, d => d.AsientoId, a => a.AsientoId, (d, a) => new { d, a })
            .Join(data.PlanCuentas, x => x.d.CuentaCodigo, p => p.CuentaCodigo, (x, p) => new LibroDiarioFinanciero
            {
                AsientoId = x.a.AsientoId,
                NumeroAsiento = x.a.NumeroAsiento,
                FechaAsiento = x.a.FechaAsiento,
                TipoLibroSunat = x.a.TipoLibroSunat,
                Glosa = x.a.Glosa,
                DocumentoReferencia = x.a.DocumentoReferencia,
                CuentaCodigo = x.d.CuentaCodigo,
                NombreCuenta = p.Descripcion,
                Debe = x.d.Debe,
                Haber = x.d.Haber
            })
            .OrderByDescending(x => x.FechaAsiento)
            .ThenBy(x => x.NumeroAsiento)
            .ToList();
    }

    protected static string NormalizarMoneda(string? moneda)
    {
        var value = string.IsNullOrWhiteSpace(moneda) ? "pen" : moneda.Trim().ToLowerInvariant();
        return value.Length > 3 ? value[..3] : value.PadRight(3, ' ');
    }

    private string ResolverRolActual()
    {
        var queryRol = Request.Query["rol"].ToString();
        var rawRol = !string.IsNullOrWhiteSpace(queryRol)
            ? queryRol
            : Request.Cookies["sge_rol"];

        var rol = RolesValidos.FirstOrDefault(x => string.Equals(x, rawRol, StringComparison.OrdinalIgnoreCase))
            ?? "Administrador";

        if (!string.IsNullOrWhiteSpace(queryRol))
        {
            Response.Cookies.Append("sge_rol", rol, new CookieOptions
            {
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
        }

        return rol;
    }
}
