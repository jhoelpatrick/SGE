namespace SGE.Models;

public class Permiso
{
    public string Modulo { get; set; } = "";
    public bool Ver { get; set; }
    public bool CrearEditar { get; set; }
    public bool Eliminar { get; set; }
    public bool Reportes { get; set; }
}

public static class SistemaRoles
{
    // Valores por defecto (inmutables - solo lectura)
    public static readonly Dictionary<string, List<Permiso>> MatrizDefecto = new()
    {
        ["Administrador"] = new()
        {
            new() { Modulo = "Clientes",     Ver = true,  CrearEditar = true,  Eliminar = true,  Reportes = true  },
            new() { Modulo = "Ventas",        Ver = true,  CrearEditar = true,  Eliminar = true,  Reportes = true  },
            new() { Modulo = "Productos",     Ver = true,  CrearEditar = true,  Eliminar = true,  Reportes = true  },
            new() { Modulo = "Facturación",   Ver = true,  CrearEditar = true,  Eliminar = true,  Reportes = true  },
            new() { Modulo = "Configuración", Ver = true,  CrearEditar = true,  Eliminar = true,  Reportes = true  },
        },
        ["Asesor Comercial"] = new()
        {
            new() { Modulo = "Clientes",      Ver = true,  CrearEditar = true,  Eliminar = false, Reportes = true  },
            new() { Modulo = "Ventas",        Ver = true,  CrearEditar = true,  Eliminar = false, Reportes = true  },
            new() { Modulo = "Productos",     Ver = true,  CrearEditar = false, Eliminar = false, Reportes = false },
            new() { Modulo = "Facturación",   Ver = true,  CrearEditar = true,  Eliminar = false, Reportes = false },
            new() { Modulo = "Configuración", Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
        },
        ["Gerente RRHH"] = new()
        {
            new() { Modulo = "Clientes",      Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
            new() { Modulo = "Ventas",        Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
            new() { Modulo = "Productos",     Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
            new() { Modulo = "Facturación",   Ver = true,  CrearEditar = false, Eliminar = false, Reportes = true  },
            new() { Modulo = "Configuración", Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
        },
        ["Contador"] = new()
        {
            new() { Modulo = "Clientes",      Ver = true,  CrearEditar = false, Eliminar = false, Reportes = true  },
            new() { Modulo = "Ventas",        Ver = true,  CrearEditar = false, Eliminar = false, Reportes = true  },
            new() { Modulo = "Productos",     Ver = true,  CrearEditar = false, Eliminar = false, Reportes = false },
            new() { Modulo = "Facturación",   Ver = true,  CrearEditar = true,  Eliminar = false, Reportes = true  },
            new() { Modulo = "Configuración", Ver = false, CrearEditar = false, Eliminar = false, Reportes = false },
        },
    };

    // Acceso a la matriz mutable vive en DataStore (sin referencia circular aquí)
    public static readonly string[] Lista = { "Administrador", "Asesor Comercial", "Gerente RRHH", "Contador" };
}
