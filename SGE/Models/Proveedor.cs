using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("proveedores", Schema = "comercial")]
public class Proveedor
{
    [Key]
    [Column("proveedorid")]
    public int ProveedorId { get; set; }

    [Required]
    [Column("tipodocumento")]
    [StringLength(1)]
    public string TipoDocumento { get; set; } = string.Empty;

    [Required]
    [Column("numerodocumento")]
    [StringLength(15)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required]
    [Column("razonsocial")]
    [StringLength(250)]
    public string RazonSocial { get; set; } = string.Empty;

    [Column("direccionfiscal")]
    [StringLength(500)]
    public string? DireccionFiscal { get; set; }

    [Column("ubigeo")]
    [StringLength(6)]
    public string? UbigeoCodigo { get; set; }

    [ForeignKey(nameof(UbigeoCodigo))]
    public Ubigeo? UbigeoRef { get; set; }

    [Column("telefono")]
    [StringLength(50)]
    public string? Telefono { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("estado")]
    public bool Estado { get; set; } = true;

    public ICollection<ContactoProveedor> Contactos { get; set; } = new List<ContactoProveedor>();

    [NotMapped]
    public bool Activo
    {
        get => Estado;
        set => Estado = value;
    }

    [NotMapped]
    public string? NombreComercial { get; set; }

    [NotMapped]
    public string? NombreContacto { get; set; }

    [NotMapped]
    public string? CargoContacto { get; set; }

    [NotMapped]
    public string? TelefonoContacto { get; set; }

    [NotMapped]
    public string? EmailContacto { get; set; }
}
