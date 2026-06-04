namespace SGE.Models;

public class AsientoDetalleFinanciero
{
    public long AsientoDetalleId { get; set; }
    public long AsientoId { get; set; }
    public string CuentaCodigo { get; set; } = "";
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}
