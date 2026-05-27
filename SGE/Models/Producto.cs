using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

public class Producto
{
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [StringLength(120)]
    public string? Marca { get; set; }

    [StringLength(150)]
    public string? Proveedor { get; set; }

    [StringLength(120)]
    public string? Almacen { get; set; }

    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    [Range(0, 999999999, ErrorMessage = "El costo de compra debe ser mayor o igual a cero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoCompra { get; set; }

    [Range(0, 999999999, ErrorMessage = "El precio unitario debe ser mayor o igual a cero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [NotMapped]
    public decimal PrecioVenta
    {
        get => PrecioUnitario;
        set => PrecioUnitario = value;
    }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(50)]
    public string UnidadDeMedida { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "El peso debe ser mayor o igual a cero.")]
    [Column(TypeName = "decimal(18,3)")]
    public decimal Peso { get; set; }

    [StringLength(100)]
    public string? Dimensiones { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock actual debe ser mayor o igual a cero.")]
    public int StockActual { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock minimo debe ser mayor o igual a cero.")]
    public int StockMinimo { get; set; }

    public bool RequiereInventario { get; set; } = true;

    [NotMapped]
    public bool EsServicio
    {
        get => !RequiereInventario;
        set => RequiereInventario = !value;
    }

    public bool Activo { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    [StringLength(100)]
    public string UsuarioCreacion { get; set; } = "Sistema";

    [StringLength(100)]
    public string? UsuarioActualizacion { get; set; }

    [Required(ErrorMessage = "La categoria es obligatoria.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public decimal ValorInventario => RequiereInventario ? StockActual * CostoCompra : 0m;

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
