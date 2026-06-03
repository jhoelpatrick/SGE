namespace SGE.Models;

public class ActivosFijosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<ActivoFijoFinanciero> Activos { get; set; } = new();
    public decimal ValorTotal => Activos.Sum(x => x.ValorInicial);
    public decimal DepreciacionTotal => Activos.Sum(x => x.DepreciacionAcumulada);
    public decimal ValorNetoTotal => Activos.Sum(x => x.ValorNetoLibros);
}
