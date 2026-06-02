using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

public class ContabilidadController : FinanzasBaseController
{
    private readonly ILogger<ContabilidadController> _logger;

    public ContabilidadController(IFinanzasDataService finanzasData, ILogger<ContabilidadController> logger)
        : base(finanzasData)
    {
        _logger = logger;
    }

    [HttpGet("Contabilidad")]
    public IActionResult Index()
    {
        try
        {
            var data = FinanzasData.CargarFinanzas();
            var model = new ContabilidadFinanzasViewModel
            {
                PlanCuentas = data.PlanCuentas,
                Asientos = data.AsientosCabecera,
                Detalles = data.AsientosDetalle,
                LibroDiario = BuildLibroDiario(data)
            };

            model.Kpis = new List<FinanzasKpi>
            {
                new() { Titulo = "Plan cuentas", Valor = model.PlanCuentas.Count.ToString(), Detalle = "finanzas.plancuentas", Icono = "bi-diagram-3", Tono = "teal" },
                new() { Titulo = "Asientos", Valor = model.Asientos.Count.ToString(), Detalle = "finanzas.asientoscabecera", Icono = "bi-journal-text", Tono = "indigo" },
                new() { Titulo = "Total debe", Valor = FinanzasFormat.Money(model.TotalDebe), Detalle = "finanzas.asientosdetalle", Icono = "bi-arrow-down-left", Tono = "amber" },
                new() { Titulo = "Diferencia", Valor = FinanzasFormat.Money(model.Diferencia), Detalle = model.Diferencia == 0 ? "Partida doble OK" : "Revisar", Icono = "bi-balance-scale", Tono = model.Diferencia == 0 ? "green" : "red" }
            };

            return PartialView("~/Views/Finanzas/Contabilidad.cshtml", model);
        }
        catch (Exception ex)
        {
            return ErrorPartial("Contabilidad", new ContabilidadFinanzasViewModel(), ex, _logger);
        }
    }

    [HttpGet("ExportarContabilidad")]
    public IActionResult Exportar()
    {
        var data = FinanzasData.CargarFinanzas();
        var rows = new List<string[]> { new[] { "Fecha", "Asiento", "Libro", "Cuenta", "Glosa", "Debe", "Haber" } };
        rows.AddRange(BuildLibroDiario(data).Select(x => new[]
        {
            x.FechaAsiento.ToString("yyyy-MM-dd"),
            x.NumeroAsiento,
            x.TipoLibroSunat,
            $"{x.CuentaCodigo} - {x.NombreCuenta}",
            x.Glosa,
            x.Debe.ToString("0.00"),
            x.Haber.ToString("0.00")
        }));

        return FinanzasFormat.Csv("contabilidad_finanzas.csv", rows, this);
    }

    [HttpPost("CrearCuentaPlan")]
    public IActionResult CrearCuentaPlan(PlanCuentaFinanciero cuenta)
    {
        if (string.IsNullOrWhiteSpace(cuenta.CuentaCodigo) || string.IsNullOrWhiteSpace(cuenta.Descripcion))
        {
            TempData["Error"] = "Completa codigo y descripcion de la cuenta contable.";
            return Redirect("/Finanzas/Contabilidad");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
insert into finanzas.plancuentas (cuentacodigo, descripcion, tipocuenta, nivelint, aceptaasiento)
values (@codigo, @descripcion, @tipo, @nivel, @acepta)", p =>
            {
                p.AddWithValue("@codigo", cuenta.CuentaCodigo.Trim());
                p.AddWithValue("@descripcion", cuenta.Descripcion.Trim());
                p.AddWithValue("@tipo", string.IsNullOrWhiteSpace(cuenta.TipoCuenta) ? "activo" : cuenta.TipoCuenta.Trim());
                p.AddWithValue("@nivel", cuenta.NivelInt <= 0 ? 5 : cuenta.NivelInt);
                p.AddWithValue("@acepta", cuenta.AceptaAsiento);
            });

            TempData["Ok"] = "Cuenta contable registrada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar cuenta contable {CuentaCodigo}.", cuenta.CuentaCodigo);
            TempData["Error"] = "No se pudo registrar la cuenta. Verifica si el codigo ya existe.";
        }

        return Redirect("/Finanzas/Contabilidad");
    }

    [HttpPost("ActualizarCuentaPlan")]
    public IActionResult ActualizarCuentaPlan(string cuentaCodigoOriginal, string descripcion, string tipoCuenta, int nivelInt, bool aceptaAsiento)
    {
        if (string.IsNullOrWhiteSpace(cuentaCodigoOriginal) || string.IsNullOrWhiteSpace(descripcion))
        {
            TempData["Error"] = "No se pudo actualizar la cuenta contable.";
            return Redirect("/Finanzas/Contabilidad");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.plancuentas
set descripcion = @descripcion,
    tipocuenta = @tipo,
    nivelint = @nivel,
    aceptaasiento = @acepta
where cuentacodigo = @codigo", p =>
            {
                p.AddWithValue("@codigo", cuentaCodigoOriginal);
                p.AddWithValue("@descripcion", descripcion.Trim());
                p.AddWithValue("@tipo", string.IsNullOrWhiteSpace(tipoCuenta) ? "activo" : tipoCuenta.Trim());
                p.AddWithValue("@nivel", nivelInt <= 0 ? 5 : nivelInt);
                p.AddWithValue("@acepta", aceptaAsiento);
            });

            TempData["Ok"] = "Cuenta contable actualizada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar cuenta {CuentaCodigo}.", cuentaCodigoOriginal);
            TempData["Error"] = "No se pudo actualizar la cuenta contable.";
        }

        return Redirect("/Finanzas/Contabilidad");
    }

    [HttpPost("CrearAsiento")]
    public IActionResult CrearAsiento(string glosa, string tipoLibroSunat, string documentoReferencia, string cuentaDebe, decimal montoDebe, string cuentaHaber, decimal montoHaber)
    {
        if (montoDebe <= 0 || montoDebe != montoHaber)
        {
            TempData["Error"] = "El asiento debe estar cuadrado y tener importes mayores a cero.";
            return Redirect("/Finanzas/Contabilidad");
        }

        try
        {
            FinanzasData.CrearAsiento(tipoLibroSunat, glosa, documentoReferencia, cuentaDebe, montoDebe, cuentaHaber, montoHaber);
            TempData["Ok"] = "Asiento registrado en SQL Server.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el asiento contable.");
            TempData["Error"] = "No se pudo registrar el asiento. Revisa cuentas y montos.";
        }

        return Redirect("/Finanzas/Contabilidad");
    }

    [HttpPost("ActualizarAsiento")]
    public IActionResult ActualizarAsiento(long asientoId, DateTime fechaAsiento, string tipoLibroSunat, string glosa, string documentoReferencia)
    {
        if (asientoId <= 0 || string.IsNullOrWhiteSpace(glosa))
        {
            TempData["Error"] = "No se pudo actualizar el asiento. La glosa es obligatoria.";
            return Redirect("/Finanzas/Contabilidad");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.asientoscabecera
set fechaasiento = @fecha,
    tipolibrosunat = @libro,
    glosa = @glosa,
    documentoreferencia = @documento
where asientoid = @id", p =>
            {
                p.AddWithValue("@id", asientoId);
                p.AddWithValue("@fecha", fechaAsiento == default ? DateTime.Today : fechaAsiento);
                p.AddWithValue("@libro", string.IsNullOrWhiteSpace(tipoLibroSunat) ? "01" : tipoLibroSunat.PadLeft(2, '0')[..2]);
                p.AddWithValue("@glosa", glosa.Trim());
                p.AddWithValue("@documento", string.IsNullOrWhiteSpace(documentoReferencia) ? DBNull.Value : documentoReferencia.Trim());
            });

            TempData["Ok"] = "Cabecera del asiento actualizada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar asiento {AsientoId}.", asientoId);
            TempData["Error"] = "No se pudo actualizar el asiento.";
        }

        return Redirect("/Finanzas/Contabilidad");
    }

    [HttpPost("ActualizarDetalleAsiento")]
    public IActionResult ActualizarDetalleAsiento(long asientoDetalleId, string cuentaCodigo, decimal debe, decimal haber)
    {
        if (asientoDetalleId <= 0 || string.IsNullOrWhiteSpace(cuentaCodigo) || debe < 0 || haber < 0 || (debe > 0 && haber > 0))
        {
            TempData["Error"] = "El detalle debe tener cuenta valida y solo debe o haber con valor positivo.";
            return Redirect("/Finanzas/Contabilidad");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.asientosdetalle
set cuentacodigo = @cuenta,
    debe = @debe,
    haber = @haber
where asientodetalleid = @id", p =>
            {
                p.AddWithValue("@id", asientoDetalleId);
                p.AddWithValue("@cuenta", cuentaCodigo);
                p.AddWithValue("@debe", debe);
                p.AddWithValue("@haber", haber);
            });

            TempData["Ok"] = "Detalle del asiento actualizado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar detalle {AsientoDetalleId}.", asientoDetalleId);
            TempData["Error"] = "No se pudo actualizar el detalle. Revisa la cuenta contable.";
        }

        return Redirect("/Finanzas/Contabilidad");
    }
}
