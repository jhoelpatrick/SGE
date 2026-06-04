namespace SGE.Models;

public class ActivoFijoFinanciero
{
    public int ActivoId { get; set; }
    public string CodigoActivo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int? ProductoId { get; set; }
    public string? ProductoSku { get; set; }
    public string? ProductoDescripcion { get; set; }
    public DateTime FechaAdquisicion { get; set; } = DateTime.Today;
    public decimal ValorInicial { get; set; }
    public decimal TasaDepreciacionAnual { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorNetoLibros => ValorInicial - DepreciacionAcumulada;
    public string Estado { get; set; } = "activo";
}
