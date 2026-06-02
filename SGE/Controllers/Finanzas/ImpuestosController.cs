using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

public class ImpuestosController : FinanzasBaseController
{
    private readonly ILogger<ImpuestosController> _logger;

    public ImpuestosController(IFinanzasDataService finanzasData, ILogger<ImpuestosController> logger)
        : base(finanzasData)
    {
        _logger = logger;
    }

    [HttpGet("Impuestos")]
    public IActionResult Index()
    {
        try
        {
            var data = FinanzasData.CargarFinanzas();
            var model = new ImpuestosViewModel
            {
                Impuestos = data.Impuestos,
                DebitoFiscal = data.AsientosDetalle.Where(x => x.CuentaCodigo == "4011").Sum(x => x.Haber),
                CreditoFiscal = data.AsientosDetalle.Where(x => x.CuentaCodigo == "4011").Sum(x => x.Debe),
                Retenciones = data.Impuestos.Where(x => x.Estado && x.NombreImpuesto.Contains("Retencion", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Porcentaje) * 100,
                Percepciones = data.Impuestos.Where(x => x.Estado && x.NombreImpuesto.Contains("Percepcion", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Porcentaje) * 70
            };

            model.Kpis = new List<FinanzasKpi>
            {
                new() { Titulo = "Impuestos", Valor = model.Impuestos.Count.ToString(), Detalle = "Tabla finanzas.impuestos", Icono = "bi-file-earmark-text", Tono = "teal" },
                new() { Titulo = "IGV neto", Valor = FinanzasFormat.Money(model.IgvNeto), Detalle = "Debito - credito", Icono = "bi-percent", Tono = "indigo" },
                new() { Titulo = "Creditos", Valor = FinanzasFormat.Money(model.CreditosDeducibles), Detalle = "Retenciones y percepciones", Icono = "bi-shield-check", Tono = "green" },
                new() { Titulo = "Pago SUNAT", Valor = FinanzasFormat.Money(model.MontoEstimadoSunat), Detalle = "Estimado mensual", Icono = "bi-cash-coin", Tono = "amber" }
            };

            return PartialView("~/Views/Finanzas/Impuestos.cshtml", model);
        }
        catch (Exception ex)
        {
            return ErrorPartial("Impuestos", new ImpuestosViewModel(), ex, _logger);
        }
    }

    [HttpGet("ExportarImpuestos")]
    public IActionResult Exportar()
    {
        var data = FinanzasData.CargarFinanzas();
        var rows = new List<string[]> { new[] { "ID", "Codigo SUNAT", "Nombre", "Porcentaje", "Estado" } };
        rows.AddRange(data.Impuestos.Select(x => new[]
        {
            x.ImpuestoId.ToString(),
            x.CodigoImpuestoSunat,
            x.NombreImpuesto,
            x.Porcentaje.ToString("0.##"),
            x.Estado ? "Activo" : "Inactivo"
        }));

        return FinanzasFormat.Csv("impuestos_finanzas.csv", rows, this);
    }

    [HttpPost("CrearImpuesto")]
    public IActionResult Crear(ImpuestoFinanciero impuesto)
    {
        if (string.IsNullOrWhiteSpace(impuesto.CodigoImpuestoSunat) || impuesto.Porcentaje < 0)
        {
            TempData["Error"] = "Completa un codigo SUNAT valido y un porcentaje positivo.";
            return Redirect("/Finanzas/Impuestos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(
                "insert into finanzas.impuestos (codigoimpuestosunat, nombreimpuesto, porcentaje, estado) values (@codigo, @nombre, @porcentaje, 1)",
                p =>
                {
                    p.AddWithValue("@codigo", impuesto.CodigoImpuestoSunat.PadLeft(4, '0')[..4]);
                    p.AddWithValue("@nombre", string.IsNullOrWhiteSpace(impuesto.NombreImpuesto) ? "Nuevo impuesto" : impuesto.NombreImpuesto.Trim());
                    p.AddWithValue("@porcentaje", impuesto.Porcentaje);
                });

            TempData["Ok"] = "Impuesto registrado en SQL Server.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el impuesto {CodigoImpuestoSunat}.", impuesto.CodigoImpuestoSunat);
            TempData["Error"] = "No se pudo registrar el impuesto. Verifica si el codigo SUNAT ya existe.";
        }

        return Redirect("/Finanzas/Impuestos");
    }

    [HttpPost("ActualizarImpuesto")]
    public IActionResult Actualizar(int impuestoId, string codigoImpuestoSunat, string nombreImpuesto, decimal porcentaje, bool estado)
    {
        if (impuestoId <= 0 || string.IsNullOrWhiteSpace(codigoImpuestoSunat) || porcentaje < 0)
        {
            TempData["Error"] = "No se pudo actualizar el impuesto. Revisa codigo y porcentaje.";
            return Redirect("/Finanzas/Impuestos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.impuestos
set codigoimpuestosunat = @codigo,
    nombreimpuesto = @nombre,
    porcentaje = @porcentaje,
    estado = @estado
where impuestoid = @id", p =>
            {
                p.AddWithValue("@id", impuestoId);
                p.AddWithValue("@codigo", codigoImpuestoSunat.PadLeft(4, '0')[..4]);
                p.AddWithValue("@nombre", string.IsNullOrWhiteSpace(nombreImpuesto) ? "Impuesto" : nombreImpuesto.Trim());
                p.AddWithValue("@porcentaje", porcentaje);
                p.AddWithValue("@estado", estado);
            });

            TempData["Ok"] = "Impuesto actualizado en SQL Server.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar impuesto {ImpuestoId}.", impuestoId);
            TempData["Error"] = "No se pudo actualizar el impuesto. Verifica si el codigo SUNAT ya existe.";
        }

        return Redirect("/Finanzas/Impuestos");
    }

    [HttpPost("CambiarEstadoImpuesto")]
    public IActionResult CambiarEstado(int impuestoId, bool estado)
    {
        try
        {
            FinanzasData.ExecuteNonQuery("update finanzas.impuestos set estado = @estado where impuestoid = @id", p =>
            {
                p.AddWithValue("@id", impuestoId);
                p.AddWithValue("@estado", estado);
            });

            TempData["Ok"] = estado ? "Impuesto activado." : "Impuesto desactivado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo cambiar estado de impuesto {ImpuestoId}.", impuestoId);
            TempData["Error"] = "No se pudo cambiar el estado del impuesto.";
        }

        return Redirect("/Finanzas/Impuestos");
    }
}
