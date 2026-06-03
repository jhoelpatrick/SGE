using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGE.Models;

[Table("ubigeos", Schema = "comercial")]
public class Ubigeo
{
    [Key]
    [Column("codigoubigeo")]
    [StringLength(6)]
    public string CodigoUbigeo { get; set; } = string.Empty;

    [Column("departamento")]
    [StringLength(100)]
    public string Departamento { get; set; } = string.Empty;

    [Column("provincia")]
    [StringLength(100)]
    public string Provincia { get; set; } = string.Empty;

    [Column("distrito")]
    [StringLength(100)]
    public string Distrito { get; set; } = string.Empty;
}
