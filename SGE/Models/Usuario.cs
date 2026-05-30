namespace SGE.Models;

public enum EstadoUsuario { Activo, Inactivo }

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Contrasena { get; set; } = "";
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
    public string Rol { get; set; } = "";
    public DateTime FechaCreacion { get; set; } = DateTime.Today;
    public bool MfaActivo { get; set; } = false;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    public string Iniciales => $"{(Nombre.Length > 0 ? Nombre[0] : ' ')}{(Apellido.Length > 0 ? Apellido[0] : ' ')}".Trim().ToUpper();
}
