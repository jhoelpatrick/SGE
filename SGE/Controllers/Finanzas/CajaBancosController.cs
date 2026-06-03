using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

public class CajaBancosController : FinanzasBaseController
{
    private readonly ILogger<CajaBancosController> _logger;

    public CajaBancosController(IFinanzasDataService finanzasData, ILogger<CajaBancosController> logger)
        : base(finanzasData)
    {
        _logger = logger;
    }

    [HttpGet("Caja_y_Bancos")]
    public IActionResult Index()
    {
        try
        {
            PrepararPermisosFinanzas("Caja y Bancos");
            var data = FinanzasData.CargarFinanzas();
            var model = new CajaBancosViewModel
            {
                Cuentas = data.CuentasBancarias,
                Movimientos = data.MovimientosTesoreria
            };

            model.Kpis = new List<FinanzasKpi>
            {
                new() { Titulo = "Cuentas", Valor = model.Cuentas.Count.ToString(), Detalle = "finanzas.cuentasbancarias", Icono = "bi-bank", Tono = "teal" },
                new() { Titulo = "Saldo total", Valor = FinanzasFormat.Money(model.SaldoTotal), Detalle = "Disponible", Icono = "bi-wallet2", Tono = "green" },
                new() { Titulo = "Ingresos", Valor = FinanzasFormat.Money(model.TotalIngresos), Detalle = "tipoflujo ING", Icono = "bi-arrow-down-circle", Tono = "indigo" },
                new() { Titulo = "Egresos", Valor = FinanzasFormat.Money(model.TotalEgresos), Detalle = "tipoflujo EGR", Icono = "bi-arrow-up-circle", Tono = "amber" }
            };

            return PartialView("~/Views/Finanzas/Caja_y_Bancos.cshtml", model);
        }
        catch (Exception ex)
        {
            return ErrorPartial("Caja_y_Bancos", new CajaBancosViewModel(), ex, _logger);
        }
    }

    [HttpGet("ExportarCajaBancos")]
    public IActionResult Exportar()
    {
        if (!PuedeReportarFinanzas()) return DenegarOperacion("/Finanzas/Caja_y_Bancos");

        var data = FinanzasData.CargarFinanzas();
        var rows = new List<string[]> { new[] { "Fecha", "Cuenta", "Flujo", "Medio", "Glosa", "Monto" } };
        rows.AddRange(data.MovimientosTesoreria.Select(x =>
        {
            var cuenta = data.CuentasBancarias.FirstOrDefault(c => c.CuentaBancariaId == x.CuentaBancariaId);
            return new[]
            {
                x.FechaMovimiento.ToString("yyyy-MM-dd HH:mm"),
                cuenta == null ? x.CuentaBancariaId.ToString() : $"{cuenta.BancoNombre} - {cuenta.NumeroCuenta}",
                x.TipoFlujo,
                x.MedioPagoSunat,
                x.GlosaMovimiento ?? "",
                x.Monto.ToString("0.00")
            };
        }));

        return FinanzasFormat.Csv("caja_bancos_finanzas.csv", rows, this);
    }

    [HttpPost("RegistrarMovimiento")]
    public IActionResult RegistrarMovimiento(int cuentaBancariaId, string tipoFlujo, string medioPagoSunat, decimal monto, string glosaMovimiento)
    {
        if (!PuedeEditarFinanzas()) return DenegarOperacion("/Finanzas/Caja_y_Bancos");

        if (monto <= 0)
        {
            TempData["Error"] = "El monto debe ser mayor a cero.";
            return Redirect("/Finanzas/Caja_y_Bancos");
        }

        try
        {
            var flujo = tipoFlujo == "egr" ? "egr" : "ing";
            FinanzasData.ExecuteNonQuery(@"
insert into finanzas.movimientostesoreria (cuentabancariaid, tipoflujo, mediopagosunat, monto, glosamovimiento, fechamovimiento)
values (@cuenta, @flujo, @medio, @monto, @glosa, getdate());

update finanzas.cuentasbancarias
set saldoactual = saldoactual + case when @flujo = 'ing' then @monto else -@monto end
where cuentabancariaid = @cuenta;", p =>
            {
                p.AddWithValue("@cuenta", cuentaBancariaId);
                p.AddWithValue("@flujo", flujo);
                p.AddWithValue("@medio", string.IsNullOrWhiteSpace(medioPagoSunat) ? "003" : medioPagoSunat);
                p.AddWithValue("@monto", monto);
                p.AddWithValue("@glosa", string.IsNullOrWhiteSpace(glosaMovimiento) ? "Movimiento de tesoreria" : glosaMovimiento.Trim());
            });

            TempData["Ok"] = "Movimiento registrado en SQL Server.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar movimiento de tesoreria para la cuenta {CuentaBancariaId}.", cuentaBancariaId);
            TempData["Error"] = "No se pudo registrar el movimiento. Verifica la cuenta bancaria.";
        }

        return Redirect("/Finanzas/Caja_y_Bancos");
    }

    [HttpPost("CrearCuentaBancaria")]
    public IActionResult CrearCuentaBancaria(CuentaBancariaFinanciera cuenta)
    {
        if (!PuedeEditarFinanzas()) return DenegarOperacion("/Finanzas/Caja_y_Bancos");

        if (string.IsNullOrWhiteSpace(cuenta.BancoNombre) || string.IsNullOrWhiteSpace(cuenta.NumeroCuenta))
        {
            TempData["Error"] = "Completa banco y numero de cuenta.";
            return Redirect("/Finanzas/Caja_y_Bancos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
insert into finanzas.cuentasbancarias (banconombre, numerocuenta, cuentacciexterno, tipocuenta, moneda, saldoactual, estado)
values (@banco, @numero, @cci, @tipo, @moneda, @saldo, 1)", p =>
            {
                p.AddWithValue("@banco", cuenta.BancoNombre.Trim());
                p.AddWithValue("@numero", cuenta.NumeroCuenta.Trim());
                p.AddWithValue("@cci", string.IsNullOrWhiteSpace(cuenta.CuentaCciExterno) ? DBNull.Value : cuenta.CuentaCciExterno.Trim());
                p.AddWithValue("@tipo", string.IsNullOrWhiteSpace(cuenta.TipoCuenta) ? "corriente" : cuenta.TipoCuenta.Trim());
                p.AddWithValue("@moneda", NormalizarMoneda(cuenta.Moneda));
                p.AddWithValue("@saldo", cuenta.SaldoActual);
            });

            TempData["Ok"] = "Cuenta bancaria registrada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar cuenta bancaria {NumeroCuenta}.", cuenta.NumeroCuenta);
            TempData["Error"] = "No se pudo registrar la cuenta. Verifica si el numero ya existe.";
        }

        return Redirect("/Finanzas/Caja_y_Bancos");
    }

    [HttpPost("ActualizarCuentaBancaria")]
    public IActionResult ActualizarCuentaBancaria(int cuentaBancariaId, string bancoNombre, string numeroCuenta, string cuentaCciExterno, string tipoCuenta, string moneda, bool estado)
    {
        if (!PuedeEditarFinanzas()) return DenegarOperacion("/Finanzas/Caja_y_Bancos");

        if (cuentaBancariaId <= 0 || string.IsNullOrWhiteSpace(bancoNombre) || string.IsNullOrWhiteSpace(numeroCuenta))
        {
            TempData["Error"] = "No se pudo actualizar la cuenta bancaria.";
            return Redirect("/Finanzas/Caja_y_Bancos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.cuentasbancarias
set banconombre = @banco,
    numerocuenta = @numero,
    cuentacciexterno = @cci,
    tipocuenta = @tipo,
    moneda = @moneda,
    estado = @estado
where cuentabancariaid = @id", p =>
            {
                p.AddWithValue("@id", cuentaBancariaId);
                p.AddWithValue("@banco", bancoNombre.Trim());
                p.AddWithValue("@numero", numeroCuenta.Trim());
                p.AddWithValue("@cci", string.IsNullOrWhiteSpace(cuentaCciExterno) ? DBNull.Value : cuentaCciExterno.Trim());
                p.AddWithValue("@tipo", string.IsNullOrWhiteSpace(tipoCuenta) ? "corriente" : tipoCuenta.Trim());
                p.AddWithValue("@moneda", NormalizarMoneda(moneda));
                p.AddWithValue("@estado", estado);
            });

            TempData["Ok"] = "Cuenta bancaria actualizada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar cuenta bancaria {CuentaBancariaId}.", cuentaBancariaId);
            TempData["Error"] = "No se pudo actualizar la cuenta. Verifica numero duplicado.";
        }

        return Redirect("/Finanzas/Caja_y_Bancos");
    }

    [HttpPost("ActualizarMovimiento")]
    public IActionResult ActualizarMovimiento(long movimientoTesoreriaId, int cuentaBancariaId, string tipoFlujo, string medioPagoSunat, decimal monto, string glosaMovimiento)
    {
        if (!PuedeEditarFinanzas()) return DenegarOperacion("/Finanzas/Caja_y_Bancos");

        if (movimientoTesoreriaId <= 0 || cuentaBancariaId <= 0 || monto <= 0)
        {
            TempData["Error"] = "No se pudo actualizar el movimiento. Revisa cuenta y monto.";
            return Redirect("/Finanzas/Caja_y_Bancos");
        }

        try
        {
            FinanzasData.ActualizarMovimientoTesoreria(movimientoTesoreriaId, cuentaBancariaId, tipoFlujo, medioPagoSunat, monto, glosaMovimiento);
            TempData["Ok"] = "Movimiento actualizado y saldo recalculado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar movimiento {MovimientoTesoreriaId}.", movimientoTesoreriaId);
            TempData["Error"] = "No se pudo actualizar el movimiento.";
        }

        return Redirect("/Finanzas/Caja_y_Bancos");
    }
}
