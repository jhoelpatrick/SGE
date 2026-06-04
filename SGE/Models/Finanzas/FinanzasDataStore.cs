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
