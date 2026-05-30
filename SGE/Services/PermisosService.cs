using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Services;

public class PermisosService
{
    private readonly DbConnectionFactory _db;

    public PermisosService(DbConnectionFactory db) => _db = db;

    // ── Consultas ─────────────────────────────────────────────────────────────

    public Dictionary<string, List<Permiso>> ObtenerMatriz()
    {
        var matriz = new Dictionary<string, List<Permiso>>();
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand("SELECT * FROM dbo.vw_MatrizPermisos", con);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            var rol = rd.GetString(rd.GetOrdinal("Rol"));
            if (!matriz.ContainsKey(rol)) matriz[rol] = new List<Permiso>();
            matriz[rol].Add(MapearPermiso(rd));
        }
        return matriz;
    }

    public List<Permiso> ObtenerPorRol(string rol)
    {
        var lista = new List<Permiso>();
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand(
            "SELECT * FROM dbo.vw_MatrizPermisos WHERE Rol = @rol", con);
        cmd.Parameters.AddWithValue("@rol", rol);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            lista.Add(MapearPermiso(rd));
        return lista;
    }

    // ── Escritura ─────────────────────────────────────────────────────────────

    public void GuardarPermisos(string rol, List<Permiso> permisos)
    {
        using var con = _db.Create();
        con.Open();

        // Resolver el RolId una sola vez antes del loop
        int rolId;
        using (var cmdRol = new SqlCommand(
            "SELECT Id FROM dbo.Roles WHERE Nombre = @rol", con))
        {
            cmdRol.Parameters.AddWithValue("@rol", rol);
            rolId = (int)cmdRol.ExecuteScalar();
        }

        foreach (var p in permisos)
        {
            // Resolver ModuloId por cada módulo
            int moduloId;
            using (var cmdMod = new SqlCommand(
                "SELECT Id FROM dbo.Modulos WHERE Nombre = @mod", con))
            {
                cmdMod.Parameters.AddWithValue("@mod", p.Modulo);
                moduloId = (int)cmdMod.ExecuteScalar();
            }

            // Llamar el SP correctamente
            using var cmd = new SqlCommand("dbo.sp_GuardarPermisosRol", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RolId", rolId);
            cmd.Parameters.AddWithValue("@ModuloId", moduloId);
            cmd.Parameters.AddWithValue("@Ver", p.Ver);
            cmd.Parameters.AddWithValue("@CrearEditar", p.CrearEditar);
            cmd.Parameters.AddWithValue("@Eliminar", p.Eliminar);
            cmd.Parameters.AddWithValue("@Reportes", p.Reportes);
            cmd.ExecuteNonQuery();
        }
    }

    // ── Helper privado ────────────────────────────────────────────────────────

    private static Permiso MapearPermiso(SqlDataReader rd) => new()
    {
        Modulo = rd.GetString(rd.GetOrdinal("NombreModulo")),
        Ver = rd.GetBoolean(rd.GetOrdinal("Ver")),
        CrearEditar = rd.GetBoolean(rd.GetOrdinal("CrearEditar")),
        Eliminar = rd.GetBoolean(rd.GetOrdinal("Eliminar")),
        Reportes = rd.GetBoolean(rd.GetOrdinal("Reportes"))
    };
}