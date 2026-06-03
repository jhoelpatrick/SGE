using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("contactosclientes", Schema = "comercial")]
public class ContactoCliente
{
    [Key]
    [Column("contactoclienteid")]
    public int ContactoClienteId { get; set; }

    [Column("clienteid")]
    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

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
