using System.Text.Json.Serialization;

namespace SGE.Models;

public class FinanzasDataStore
{
    public List<ImpuestoFinanciero> Impuestos { get; set; } = new();
    public List<PlanCuentaFinanciero> PlanCuentas { get; set; } = new();
    public List<AsientoCabeceraFinanciero> AsientosCabecera { get; set; } = new();
    public List<AsientoDetalleFinanciero> AsientosDetalle { get; set; } = new();
    public List<CuentaBancariaFinanciera> CuentasBancarias { get; set; } = new();
    public List<MovimientoTesoreriaFinanciero> MovimientosTesoreria { get; set; } = new();
    public List<ActivoFijoFinanciero> ActivosFijos { get; set; } = new();
}

public class ImpuestoFinanciero
{
    public int ImpuestoId { get; set; }
    public string CodigoImpuestoSunat { get; set; } = "";
    public string NombreImpuesto { get; set; } = "";
    public decimal Porcentaje { get; set; }
    public bool Estado { get; set; } = true;
}

public class PlanCuentaFinanciero
{
    public string CuentaCodigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string TipoCuenta { get; set; } = "";
    public int NivelInt { get; set; } = 5;
    public bool AceptaAsiento { get; set; } = true;
}

public class AsientoCabeceraFinanciero
{
    public long AsientoId { get; set; }
    public string NumeroAsiento { get; set; } = "";
    public DateTime FechaAsiento { get; set; } = DateTime.Today;
    public string TipoLibroSunat { get; set; } = "01";
    public string Glosa { get; set; } = "";
    public string? DocumentoReferencia { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

public class AsientoDetalleFinanciero
{
    public long AsientoDetalleId { get; set; }
    public long AsientoId { get; set; }
    public string CuentaCodigo { get; set; } = "";
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

public class CuentaBancariaFinanciera
{
    public int CuentaBancariaId { get; set; }
    public string BancoNombre { get; set; } = "";
    public string NumeroCuenta { get; set; } = "";
    public string? CuentaCciExterno { get; set; }
    public string TipoCuenta { get; set; } = "corriente";
    public string Moneda { get; set; } = "pen";
    public decimal SaldoActual { get; set; }
    public bool Estado { get; set; } = true;
}

public class MovimientoTesoreriaFinanciero
{
    public long MovimientoTesoreriaId { get; set; }
    public int CuentaBancariaId { get; set; }
    public string TipoFlujo { get; set; } = "ing";
    public string MedioPagoSunat { get; set; } = "003";
    public decimal Monto { get; set; }
    public int? ComprobanteId { get; set; }
    public int? OrdenId { get; set; }
    public string? GlosaMovimiento { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.Now;
}

public class ActivoFijoFinanciero
{
    public int ActivoId { get; set; }
    public string CodigoActivo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int? ProductoId { get; set; }
    public DateTime FechaAdquisicion { get; set; } = DateTime.Today;
    public decimal ValorInicial { get; set; }
    public decimal TasaDepreciacionAnual { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorNetoLibros => ValorInicial - DepreciacionAcumulada;
    public string Estado { get; set; } = "activo";
}

public class FinanzasKpi
{
    public string Titulo { get; set; } = "";
    public string Valor { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string Icono { get; set; } = "";
    public string Tono { get; set; } = "teal";
}

public class ImpuestosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<ImpuestoFinanciero> Impuestos { get; set; } = new();
    public decimal DebitoFiscal { get; set; }
    public decimal CreditoFiscal { get; set; }
    public decimal Retenciones { get; set; }
    public decimal Percepciones { get; set; }
    public decimal IgvNeto => DebitoFiscal - CreditoFiscal;
    public decimal CreditosDeducibles => Retenciones + Percepciones;
    public decimal MontoEstimadoSunat => IgvNeto - CreditosDeducibles;
}

public class ContabilidadFinanzasViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<PlanCuentaFinanciero> PlanCuentas { get; set; } = new();
    public List<AsientoCabeceraFinanciero> Asientos { get; set; } = new();
    public List<AsientoDetalleFinanciero> Detalles { get; set; } = new();
    public List<LibroDiarioFinanciero> LibroDiario { get; set; } = new();
    public decimal TotalDebe => Detalles.Sum(x => x.Debe);
    public decimal TotalHaber => Detalles.Sum(x => x.Haber);
    public decimal Diferencia => TotalDebe - TotalHaber;
}

public class LibroDiarioFinanciero
{
    public long AsientoId { get; set; }
    public string NumeroAsiento { get; set; } = "";
    public DateTime FechaAsiento { get; set; }
    public string TipoLibroSunat { get; set; } = "";
    public string Glosa { get; set; } = "";
    public string? DocumentoReferencia { get; set; }
    public string CuentaCodigo { get; set; } = "";
    public string NombreCuenta { get; set; } = "";
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}

public class CajaBancosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<CuentaBancariaFinanciera> Cuentas { get; set; } = new();
    public List<MovimientoTesoreriaFinanciero> Movimientos { get; set; } = new();
    public decimal TotalIngresos => Movimientos.Where(x => x.TipoFlujo == "ing").Sum(x => x.Monto);
    public decimal TotalEgresos => Movimientos.Where(x => x.TipoFlujo == "egr").Sum(x => x.Monto);
    public decimal SaldoTotal => Cuentas.Where(x => x.Estado).Sum(x => x.SaldoActual);
}

public class ActivosFijosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<ActivoFijoFinanciero> Activos { get; set; } = new();
    public decimal ValorTotal => Activos.Sum(x => x.ValorInicial);
    public decimal DepreciacionTotal => Activos.Sum(x => x.DepreciacionAcumulada);
    public decimal ValorNetoTotal => Activos.Sum(x => x.ValorNetoLibros);
}
