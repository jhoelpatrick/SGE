using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Services
{
    /// <summary>
    /// Repositorio de Proveedores. Accede a comercial.proveedores en sge_crm.
    /// </summary>
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly string _connectionString;

        public ProveedorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json.");
        }

        // ── GET ALL ─────────────────────────────────────────────────────────────
        public async Task<List<Proveedor>> GetAllAsync()
        {
            var lista = new List<Proveedor>();
            const string sql = @"
                SELECT proveedorid, tipodocumento, numerodocumento, razonsocial,
                       direccionfiscal, ubigeo, telefono, email, estado
                FROM   comercial.proveedores
                ORDER BY razonsocial";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            using var rd  = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(MapProveedor(rd));
            }
            return lista;
        }

        // ── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<Proveedor?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT proveedorid, tipodocumento, numerodocumento, razonsocial,
                       direccionfiscal, ubigeo, telefono, email, estado
                FROM   comercial.proveedores
                WHERE  proveedorid = @proveedorid";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@proveedorid", id);
            using var rd  = await cmd.ExecuteReaderAsync();

            return await rd.ReadAsync() ? MapProveedor(rd) : null;
        }

        // ── CREATE ──────────────────────────────────────────────────────────────
        public async Task<int> CreateAsync(Proveedor p)
        {
            const string sql = @"
                INSERT INTO comercial.proveedores
                    (tipodocumento, numerodocumento, razonsocial,
                     direccionfiscal, ubigeo, telefono, email, estado)
                VALUES
                    (@tipodocumento, @numerodocumento, @razonsocial,
                     @direccionfiscal, @ubigeo, @telefono, @email, @estado);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            AddProveedorParameters(cmd, p);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        // ── UPDATE ──────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Proveedor p)
        {
            const string sql = @"
                UPDATE comercial.proveedores
                SET    tipodocumento   = @tipodocumento,
                       numerodocumento = @numerodocumento,
                       razonsocial     = @razonsocial,
                       direccionfiscal = @direccionfiscal,
                       ubigeo          = @ubigeo,
                       telefono        = @telefono,
                       email           = @email,
                       estado          = @estado
                WHERE  proveedorid = @proveedorid";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            AddProveedorParameters(cmd, p);
            cmd.Parameters.AddWithValue("@proveedorid", p.ProveedorId);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── DELETE ──────────────────────────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM comercial.proveedores WHERE proveedorid = @proveedorid";
            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@proveedorid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── TOGGLE ESTADO ────────────────────────────────────────────────────────
        public async Task ToggleEstadoAsync(int id, bool estado)
        {
            const string sql = "UPDATE comercial.proveedores SET estado = @estado WHERE proveedorid = @proveedorid";
            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@estado",      estado);
            cmd.Parameters.AddWithValue("@proveedorid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── HELPERS ─────────────────────────────────────────────────────────────
        private static void AddProveedorParameters(SqlCommand cmd, Proveedor p)
        {
            cmd.Parameters.AddWithValue("@tipodocumento",   p.TipoDocumento);
            cmd.Parameters.AddWithValue("@numerodocumento", p.NumeroDocumento);
            cmd.Parameters.AddWithValue("@razonsocial",     p.RazonSocial);
            cmd.Parameters.AddWithValue("@direccionfiscal", (object?)p.DireccionFiscal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ubigeo",          (object?)p.Ubigeo          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefono",        (object?)p.Telefono         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",           (object?)p.Email            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado",          p.Estado);
        }

        private static Proveedor MapProveedor(SqlDataReader rd) => new()
        {
            ProveedorId     = rd.GetInt32(0),
            TipoDocumento   = rd.GetString(1),
            NumeroDocumento = rd.GetString(2),
            RazonSocial     = rd.GetString(3),
            DireccionFiscal = rd.IsDBNull(4) ? null : rd.GetString(4),
            Ubigeo          = rd.IsDBNull(5) ? null : rd.GetString(5),
            Telefono        = rd.IsDBNull(6) ? null : rd.GetString(6),
            Email           = rd.IsDBNull(7) ? null : rd.GetString(7),
            Estado          = rd.GetBoolean(8),
        };
    }
}
