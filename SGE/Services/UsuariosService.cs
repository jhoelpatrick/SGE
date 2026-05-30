using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Services;

public class UsuariosService
{
    private readonly DbConnectionFactory _db;

    public UsuariosService(DbConnectionFactory db) => _db = db;

    // ── Consultas ─────────────────────────────────────────────────────────────

    public List<Usuario> ObtenerTodos()
    {
        var lista = new List<Usuario>();
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand("SELECT * FROM dbo.vw_Usuarios", con);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            lista.Add(MapearUsuario(rd));
        }
        return lista;
    }

    public Usuario? ObtenerPorId(int id)
    {
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand(
            "SELECT * FROM dbo.vw_Usuarios WHERE Id = @id", con);
        cmd.Parameters.AddWithValue("@id", id);
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? MapearUsuario(rd) : null;
    }

    // ── Escritura ─────────────────────────────────────────────────────────────

    public void Crear(Usuario u)
    {
        using var con = _db.Create();
        con.Open();
        var sql = @"INSERT INTO dbo.Usuarios
                    (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
                    VALUES (@n, @a, @e, @t, @h,
                        (SELECT Id FROM dbo.Roles WHERE Nombre = @rol),
                        @est, GETDATE())";
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@n", u.Nombre);
        cmd.Parameters.AddWithValue("@a", u.Apellido);
        cmd.Parameters.AddWithValue("@e", u.Email);
        cmd.Parameters.AddWithValue("@t", u.Telefono ?? "");
        cmd.Parameters.AddWithValue("@h", "$2a$12$placeholder");
        cmd.Parameters.AddWithValue("@rol", u.Rol);
        cmd.Parameters.AddWithValue("@est", u.Estado == EstadoUsuario.Activo ? 1 : 2);
        cmd.ExecuteNonQuery();
    }

    public void Editar(Usuario u)
    {
        using var con = _db.Create();
        con.Open();
        var sql = @"UPDATE dbo.Usuarios SET
                        Nombre   = @n,
                        Apellido = @a,
                        Email    = @e,
                        Telefono = @t,
                        RolId    = (SELECT Id FROM dbo.Roles WHERE Nombre = @rol),
                        EstadoId = @est
                    WHERE Id = @id";
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@n", u.Nombre);
        cmd.Parameters.AddWithValue("@a", u.Apellido);
        cmd.Parameters.AddWithValue("@e", u.Email);
        cmd.Parameters.AddWithValue("@t", u.Telefono ?? "");
        cmd.Parameters.AddWithValue("@rol", u.Rol);
        cmd.Parameters.AddWithValue("@est", u.Estado == EstadoUsuario.Activo ? 1 : 2);
        cmd.Parameters.AddWithValue("@id", u.Id);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.Usuarios WHERE Id = @id", con);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void CambiarEstado(int id)
    {
        using var con = _db.Create();
        con.Open();
        using var cmd = new SqlCommand("dbo.sp_CambiarEstadoUsuario", con);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@UsuarioId", id);
        cmd.Parameters.AddWithValue("@RealizadoPor", DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    // ── Helper privado ────────────────────────────────────────────────────────

    private static Usuario MapearUsuario(SqlDataReader rd) => new()
    {
        Id = rd.GetInt32(rd.GetOrdinal("Id")),
        Nombre = rd.GetString(rd.GetOrdinal("Nombre")),
        Apellido = rd.GetString(rd.GetOrdinal("Apellido")),
        Email = rd.GetString(rd.GetOrdinal("Email")),
        Telefono = rd.GetString(rd.GetOrdinal("Telefono")),
        Rol = rd.GetString(rd.GetOrdinal("Rol")),
        Estado = rd.GetString(rd.GetOrdinal("Estado")) == "Activo"
                        ? EstadoUsuario.Activo : EstadoUsuario.Inactivo,
        FechaCreacion = rd.GetDateTime(rd.GetOrdinal("FechaCreacion"))
    };
}