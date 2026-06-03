using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("productos", Schema = "comercial")]
public class Producto
{
    [Key]
    [Column("productoid")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [Column("codigosku")]
    [StringLength(50)]
    public string CodigoSku { get; set; } = string.Empty;

    [Column("codigosunat")]
    [StringLength(8)]
    public string? CodigoSunat { get; set; }

    [Required(ErrorMessage = "La descripcion es obligatoria.")]
    [Column("descripcion")]
    [StringLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [Column("unidadmedida")]
    [StringLength(3)]
    public string UnidadMedida { get; set; } = "NIU";

    [Column("tipoafectacionigv")]
    [StringLength(2)]
    public string TipoAfectacionIgv { get; set; } = "10";

    [Column("precioventasugerido", TypeName = "decimal(18,4)")]
    public decimal PrecioVentaSugerido { get; set; }

    [Column("costopromedio", TypeName = "decimal(18,4)")]
    public decimal CostoPromedio { get; set; }

    [Column("esservicio")]
    public bool EsServicio { get; set; }

    [Column("sevende")]
    public bool SeVende { get; set; } = true;

    [Column("nosevende")]
    public bool NoSeVende { get; set; }

    [Column("sefabrica")]
    public bool SeFabrica { get; set; }

    [Column("estado")]
    public bool Estado { get; set; } = true;

    [NotMapped]
    public string SKU
    {
        get => CodigoSku;
        set => CodigoSku = value;
    }

    [NotMapped]
    public string Nombre
    {
        get => Descripcion;
        set => Descripcion = value;
    }

    [NotMapped]
    public bool Activo
    {
        get => Estado;
        set => Estado = value;
    }

    [NotMapped]
    public bool RequiereInventario
    {
        get => !EsServicio;
        set => EsServicio = !value;
    }

    [NotMapped]
    public decimal PrecioUnitario
    {
        get => PrecioVentaSugerido;
        set => PrecioVentaSugerido = value;
    }

    [NotMapped]
    public decimal PrecioVenta
    {
        get => PrecioVentaSugerido;
        set => PrecioVentaSugerido = value;
    }

    [NotMapped]
    public decimal CostoCompra
    {
        get => CostoPromedio;
        set => CostoPromedio = value;
    }

    [NotMapped]
    public string UnidadDeMedida
    {
        get => UnidadMedida;
        set => UnidadMedida = value.Length > 3 ? value[..3] : value;
    }

    [NotMapped]
    public bool EsInsumo
    {
        get => SeFabrica;
        set => SeFabrica = value;
    }

    [NotMapped]
    public int CategoriaId { get; set; }

    [NotMapped]
    public Categoria? Categoria { get; set; }

    [NotMapped]
    public string? Marca { get; set; }

    [NotMapped]
    public string? Proveedor { get; set; }

    [NotMapped]
    public string? Almacen { get; set; }

    [NotMapped]
    public string? ImagenUrl { get; set; }

    [NotMapped]
    public decimal Peso { get; set; }

    [NotMapped]
    public string? Dimensiones { get; set; }

    [NotMapped]
    public int StockActual { get; set; }

    [NotMapped]
    public int StockMinimo { get; set; }

    [NotMapped]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public DateTime? FechaActualizacion { get; set; }

    [NotMapped]
    public string UsuarioCreacion { get; set; } = "Sistema";

    [NotMapped]
    public string? UsuarioActualizacion { get; set; }

    [NotMapped]
    public decimal ValorInventario => RequiereInventario ? StockActual * CostoPromedio : 0m;

    [NotMapped]
    public string EstadoStock
    {
        get
        {
            if (!RequiereInventario)
            {
                return "Servicio";
            }

            if (StockActual <= 0)
            {
                return "Agotado";
            }

            return StockActual <= StockMinimo ? "Bajo Stock" : "Disponible";
        }
    }
}
