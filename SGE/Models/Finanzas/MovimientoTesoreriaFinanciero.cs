namespace SGE.Models;

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
