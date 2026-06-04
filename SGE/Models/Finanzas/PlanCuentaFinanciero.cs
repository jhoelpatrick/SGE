namespace SGE.Models;

public class PlanCuentaFinanciero
{
    public string CuentaCodigo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string TipoCuenta { get; set; } = "";
    public int NivelInt { get; set; } = 5;
    public bool AceptaAsiento { get; set; } = true;
}
