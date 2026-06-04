namespace SGE.Models;

public class CajaBancosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<CuentaBancariaFinanciera> Cuentas { get; set; } = new();
    public List<MovimientoTesoreriaFinanciero> Movimientos { get; set; } = new();
    public decimal TotalIngresos => Movimientos.Where(x => x.TipoFlujo == "ing").Sum(x => x.Monto);
    public decimal TotalEgresos => Movimientos.Where(x => x.TipoFlujo == "egr").Sum(x => x.Monto);
    public decimal SaldoTotal => Cuentas.Where(x => x.Estado).Sum(x => x.SaldoActual);
}
