using SGE.Models;

namespace SGE.Services;

public static class DataStore
{
    public static List<Usuario> Usuarios { get; private set; } = new()
    {
        new() { Id=1, Nombre="Alejandro", Apellido="Rodríguez", Email="a.rodriguez@empresa.com", Telefono="+52 81 1234 5678", Rol="Asesor Comercial",  Estado=EstadoUsuario.Activo,   FechaCreacion=new DateTime(2026,5,12), Contrasena="Pass123!" },
        new() { Id=2, Nombre="María",     Apellido="González",  Email="m.gonzalez@empresa.com",  Telefono="+52 81 9876 5432", Rol="Gerente RRHH",     Estado=EstadoUsuario.Activo,   FechaCreacion=new DateTime(2026,3, 8), Contrasena="Pass456@" },
        new() { Id=3, Nombre="Carlos",    Apellido="López",     Email="c.lopez@empresa.com",     Telefono="+52 55 4567 8901", Rol="Administrador",    Estado=EstadoUsuario.Activo,   FechaCreacion=new DateTime(2026,1, 1), Contrasena="Admin789#" },
        new() { Id=4, Nombre="Laura",     Apellido="Martínez",  Email="l.martinez@empresa.com",  Telefono="+52 33 2345 6789", Rol="Contador",         Estado=EstadoUsuario.Inactivo, FechaCreacion=new DateTime(2026,2,15), Contrasena="Cont321$" },
        new() { Id=5, Nombre="Jorge",     Apellido="Flores",    Email="j.flores@empresa.com",    Telefono="+52 81 3456 7890", Rol="Asesor Comercial", Estado=EstadoUsuario.Activo,   FechaCreacion=new DateTime(2026,4,20), Contrasena="Pass654%" },
    };

    private static int _nextId = 6;

    // ── Matriz de permisos MUTABLE (copia independiente de MatrizDefecto) ──────
    private static Dictionary<string, List<Permiso>> _permisosMatriz =
        SistemaRoles.MatrizDefecto.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(p => new Permiso
            {
                Modulo      = p.Modulo,
                Ver         = p.Ver,
                CrearEditar = p.CrearEditar,
                Eliminar    = p.Eliminar,
                Reportes    = p.Reportes
            }).ToList()
        );

    /// <summary>Devuelve la matriz mutable actual de permisos.</summary>
    public static Dictionary<string, List<Permiso>> ObtenerMatriz() => _permisosMatriz;

    /// <summary>Persiste los permisos de un rol en memoria.</summary>
    public static void GuardarPermisosRol(string rol, List<Permiso> permisos)
    {
        if (_permisosMatriz.ContainsKey(rol))
            _permisosMatriz[rol] = permisos;
    }

    // ── Usuarios ───────────────────────────────────────────────────────────────
    public static void AgregarUsuario(Usuario u)
    {
        u.Id = _nextId++;
        Usuarios.Add(u);
    }

    public static void EliminarUsuario(int id) =>
        Usuarios.RemoveAll(u => u.Id == id);

    public static void ActualizarUsuario(Usuario actualizado)
    {
        var idx = Usuarios.FindIndex(u => u.Id == actualizado.Id);
        if (idx >= 0) Usuarios[idx] = actualizado;
    }

    public static int TotalActivos   => Usuarios.Count(u => u.Estado == EstadoUsuario.Activo);
    public static int TotalInactivos => Usuarios.Count(u => u.Estado == EstadoUsuario.Inactivo);
    public static int TotalRoles     => SistemaRoles.Lista.Length;
}
