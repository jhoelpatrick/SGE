using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using SGE.Services;

namespace SGE.Controllers.Finanzas;

public class ActivosFijosController : FinanzasBaseController
{
    private readonly ILogger<ActivosFijosController> _logger;

    public ActivosFijosController(IFinanzasDataService finanzasData, ILogger<ActivosFijosController> logger)
        : base(finanzasData)
    {
        _logger = logger;
    }

    [HttpGet("ActivosFijos")]
    public IActionResult Index()
    {
        try
        {
            var data = FinanzasData.CargarFinanzas();
            var model = new ActivosFijosViewModel
            {
                Activos = data.ActivosFijos
            };

            model.Kpis = new List<FinanzasKpi>
            {
                new() { Titulo = "Activos", Valor = model.Activos.Count.ToString(), Detalle = "finanzas.activosfijos", Icono = "bi-box-seam", Tono = "indigo" },
                new() { Titulo = "Valor total", Valor = FinanzasFormat.Money(model.ValorTotal), Detalle = "Adquisicion", Icono = "bi-cash-stack", Tono = "green" },
                new() { Titulo = "Depreciacion", Valor = FinanzasFormat.Money(model.DepreciacionTotal), Detalle = "Acumulada", Icono = "bi-graph-down", Tono = "amber" },
                new() { Titulo = "Valor neto", Valor = FinanzasFormat.Money(model.ValorNetoTotal), Detalle = "Libros", Icono = "bi-journal-check", Tono = "teal" }
            };

            return PartialView("~/Views/Finanzas/ActivosFijos.cshtml", model);
        }
        catch (Exception ex)
        {
            return ErrorPartial("ActivosFijos", new ActivosFijosViewModel(), ex, _logger);
        }
    }

    [HttpGet("ExportarActivosFijos")]
    public IActionResult Exportar()
    {
        var data = FinanzasData.CargarFinanzas();
        var rows = new List<string[]> { new[] { "Codigo", "Descripcion", "Fecha", "Valor inicial", "Tasa", "Depreciacion", "Valor neto", "Estado" } };
        rows.AddRange(data.ActivosFijos.Select(x => new[]
        {
            x.CodigoActivo,
            x.Descripcion,
            x.FechaAdquisicion.ToString("yyyy-MM-dd"),
            x.ValorInicial.ToString("0.00"),
            x.TasaDepreciacionAnual.ToString("0.##"),
            x.DepreciacionAcumulada.ToString("0.00"),
            x.ValorNetoLibros.ToString("0.00"),
            x.Estado
        }));

        return FinanzasFormat.Csv("activos_fijos_finanzas.csv", rows, this);
    }

    [HttpPost("CrearActivo")]
    public IActionResult CrearActivo(ActivoFijoFinanciero activo)
    {
        if (activo.ValorInicial <= 0 || activo.TasaDepreciacionAnual < 0 || activo.DepreciacionAcumulada < 0)
        {
            TempData["Error"] = "El valor inicial debe ser mayor a cero y la depreciacion no puede ser negativa.";
            return Redirect("/Finanzas/ActivosFijos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
insert into finanzas.activosfijos (codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada, estado)
values (@codigo, @descripcion, null, @fecha, @valor, @tasa, @depreciacion, @estado)", p =>
            {
                p.AddWithValue("@codigo", string.IsNullOrWhiteSpace(activo.CodigoActivo) ? $"AF-2026-{DateTime.Now:HHmmss}" : activo.CodigoActivo.Trim());
                p.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(activo.Descripcion) ? "Nuevo activo fijo" : activo.Descripcion.Trim());
                p.AddWithValue("@fecha", activo.FechaAdquisicion == default ? DateTime.Today : activo.FechaAdquisicion);
                p.AddWithValue("@valor", activo.ValorInicial);
                p.AddWithValue("@tasa", activo.TasaDepreciacionAnual);
                p.AddWithValue("@depreciacion", activo.DepreciacionAcumulada);
                p.AddWithValue("@estado", string.IsNullOrWhiteSpace(activo.Estado) ? "activo" : activo.Estado.Trim());
            });

            TempData["Ok"] = "Activo registrado en SQL Server.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el activo fijo {CodigoActivo}.", activo.CodigoActivo);
            TempData["Error"] = "No se pudo registrar el activo. Verifica si el codigo ya existe.";
        }

        return Redirect("/Finanzas/ActivosFijos");
    }

    [HttpPost("ActualizarActivo")]
    public IActionResult ActualizarActivo(ActivoFijoFinanciero activo)
    {
        if (activo.ActivoId <= 0 || activo.ValorInicial <= 0 || activo.TasaDepreciacionAnual < 0 || activo.DepreciacionAcumulada < 0)
        {
            TempData["Error"] = "No se pudo actualizar el activo. Revisa valores y depreciacion.";
            return Redirect("/Finanzas/ActivosFijos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.activosfijos
set codigoactivo = @codigo,
    descripcion = @descripcion,
    fechadquisicion = @fecha,
    valorinicial = @valor,
    tasadepreciacionanual = @tasa,
    depreciacionacumulada = @depreciacion,
    estado = @estado
where activoid = @id", p =>
            {
                p.AddWithValue("@id", activo.ActivoId);
                p.AddWithValue("@codigo", string.IsNullOrWhiteSpace(activo.CodigoActivo) ? $"AF-{activo.ActivoId}" : activo.CodigoActivo.Trim());
                p.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(activo.Descripcion) ? "Activo fijo" : activo.Descripcion.Trim());
                p.AddWithValue("@fecha", activo.FechaAdquisicion == default ? DateTime.Today : activo.FechaAdquisicion);
                p.AddWithValue("@valor", activo.ValorInicial);
                p.AddWithValue("@tasa", activo.TasaDepreciacionAnual);
                p.AddWithValue("@depreciacion", Math.Min(activo.DepreciacionAcumulada, activo.ValorInicial));
                p.AddWithValue("@estado", string.IsNullOrWhiteSpace(activo.Estado) ? "activo" : activo.Estado.Trim());
            });

            TempData["Ok"] = "Activo fijo actualizado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar activo {ActivoId}.", activo.ActivoId);
            TempData["Error"] = "No se pudo actualizar el activo. Verifica codigo duplicado.";
        }

        return Redirect("/Finanzas/ActivosFijos");
    }

    [HttpPost("RegistrarDepreciacionActivo")]
    public IActionResult RegistrarDepreciacionActivo(int activoId, decimal montoDepreciacion)
    {
        if (activoId <= 0 || montoDepreciacion <= 0)
        {
            TempData["Error"] = "Indica un monto de depreciacion mayor a cero.";
            return Redirect("/Finanzas/ActivosFijos");
        }

        try
        {
            FinanzasData.ExecuteNonQuery(@"
update finanzas.activosfijos
set depreciacionacumulada = case
        when depreciacionacumulada + @monto >= valorinicial then valorinicial
        else depreciacionacumulada + @monto
    end,
    estado = case
        when depreciacionacumulada + @monto >= valorinicial then 'depreciado por completo'
        else estado
    end
where activoid = @id", p =>
            {
                p.AddWithValue("@id", activoId);
                p.AddWithValue("@monto", montoDepreciacion);
            });

            TempData["Ok"] = "Depreciacion registrada.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar depreciacion para activo {ActivoId}.", activoId);
            TempData["Error"] = "No se pudo registrar la depreciacion.";
        }

        return Redirect("/Finanzas/ActivosFijos");
    }

    [HttpPost("CambiarEstadoActivo")]
    public IActionResult CambiarEstadoActivo(int activoId, string estado)
    {
        try
        {
            FinanzasData.ExecuteNonQuery("update finanzas.activosfijos set estado = @estado where activoid = @id", p =>
            {
                p.AddWithValue("@id", activoId);
                p.AddWithValue("@estado", string.IsNullOrWhiteSpace(estado) ? "activo" : estado.Trim());
            });

            TempData["Ok"] = "Estado del activo actualizado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo cambiar estado del activo {ActivoId}.", activoId);
            TempData["Error"] = "No se pudo cambiar el estado del activo.";
        }

        return Redirect("/Finanzas/ActivosFijos");
    }
}
