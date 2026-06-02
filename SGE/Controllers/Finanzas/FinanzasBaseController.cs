using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

[Route("Finanzas")]
public abstract class FinanzasBaseController : Controller
{
    protected readonly IFinanzasDataService FinanzasData;

    protected FinanzasBaseController(IFinanzasDataService finanzasData)
    {
        FinanzasData = finanzasData;
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
}
