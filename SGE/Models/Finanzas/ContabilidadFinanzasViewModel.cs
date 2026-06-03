namespace SGE.Models;

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
