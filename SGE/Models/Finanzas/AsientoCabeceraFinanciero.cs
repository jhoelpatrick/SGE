namespace SGE.Models;

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
