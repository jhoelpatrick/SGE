using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("clientes", Schema = "comercial")]
public class Cliente
{
    [Key]
    [Column("clienteid")]
    public int ClienteId { get; set; }

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

    [Column("nombrecomercial")]
    [StringLength(250)]
    public string? NombreComercial { get; set; }

    [Column("direccionfiscal")]
    [StringLength(500)]
    public string? DireccionFiscal { get; set; }

    [Column("ubigeo")]
    [StringLength(6)]
    public string? UbigeoCodigo { get; set; }

    [ForeignKey(nameof(UbigeoCodigo))]
    public Ubigeo? UbigeoRef { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("telefono")]
    [StringLength(50)]
    public string? Telefono { get; set; }

    [Column("tipocliente")]
    [StringLength(20)]
    public string TipoCliente { get; set; } = "prospecto";

    [Column("fecharegistro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Column("estado")]
    public bool Estado { get; set; } = true;

    public ICollection<ContactoCliente> Contactos { get; set; } = new List<ContactoCliente>();

    [NotMapped]
    public bool Activo
    {
        get => Estado;
        set => Estado = value;
    }
}
