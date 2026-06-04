namespace SGE.Models;

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
