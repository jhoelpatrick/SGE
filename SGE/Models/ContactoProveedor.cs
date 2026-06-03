using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("contactosproveedores", Schema = "comercial")]
public class ContactoProveedor
{
    [Key]
    [Column("contactoproveedorid")]
    public int ContactoProveedorId { get; set; }

    [Column("proveedorid")]
    public int ProveedorId { get; set; }

    public Proveedor? Proveedor { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Column("cargo")]
    [StringLength(100)]
    public string? Cargo { get; set; }

    [Column("telefono")]
    [StringLength(50)]
    public string? Telefono { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("estado")]
    public bool Estado { get; set; } = true;
}
