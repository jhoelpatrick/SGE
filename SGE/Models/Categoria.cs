using System.ComponentModel.DataAnnotations;

namespace SGE.Models;

public class Categoria
{
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "El nombre de la categoria es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
