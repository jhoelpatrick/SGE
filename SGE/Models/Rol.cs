namespace SGE.Models;

/// <summary>
/// Representa un rol del sistema con sus metadatos descriptivos.
/// La lista de roles válidos vive en SistemaRoles.Lista (Permiso.cs).
/// Al migrar a BD esta clase mapea a la tabla Roles.
/// </summary>
public class Rol
{
    public int    Id          { get; set; }
    public string Nombre      { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public bool   EsSistema   { get; set; } = false; // true = no se puede eliminar
}
