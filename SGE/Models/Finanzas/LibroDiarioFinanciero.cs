namespace SGE.Models;

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
