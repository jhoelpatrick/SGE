using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Services
{
    /// <summary>
    /// Repositorio de Clientes. Accede a la base de datos sge_crm usando ADO.NET
    /// con Microsoft.Data.SqlClient. Todas las operaciones son asíncronas y usan
    /// parámetros SQL para prevenir inyección SQL.
    /// </summary>
    public class ClienteRepository : IClienteRepository
    {
        private readonly string _connectionString;

        public ClienteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json.");
        }

        // ── GET ALL ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Consulta la vista optimizada comercial.vw_crm_clientes_bandeja, que ya incluye
        /// la dirección formateada con ubigeo y la descripción del tipo de documento.
        /// </summary>
        public async Task<List<Cliente>> GetAllAsync()
        {
            var lista = new List<Cliente>();
            const string sql = @"
                SELECT clienteid, tipodocumento, tipodocumentodesc, numerodocumento,
                       razonsocial, nombrecomercial, email, telefono,
                       tipocliente, estado, direccioncompletaui
                FROM   comercial.vw_crm_clientes_bandeja
                ORDER BY razonsocial";

            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            using var rd  = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(MapCliente(rd));
            }
            return lista;
        }

        // ── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<Cliente?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT clienteid, tipodocumento, tipodocumentodesc, numerodocumento,
                       razonsocial, nombrecomercial, email, telefono,
                       tipocliente, estado, direccioncompletaui
                FROM   comercial.vw_crm_clientes_bandeja
                WHERE  clienteid = @clienteid";

            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@clienteid", id);
            using var rd  = await cmd.ExecuteReaderAsync();

            return await rd.ReadAsync() ? MapCliente(rd) : null;
        }

        // ── CREATE ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Inserta un nuevo cliente y devuelve el ID auto-generado usando SCOPE_IDENTITY().
        /// </summary>
        public async Task<int> CreateAsync(Cliente c)
        {
            const string sql = @"
                INSERT INTO comercial.clientes
                    (tipodocumento, numerodocumento, razonsocial, nombrecomercial,
                     direccionfiscal, ubigeo, email, telefono, tipocliente, estado)
                VALUES
                    (@tipodocumento, @numerodocumento, @razonsocial, @nombrecomercial,
                     @direccionfiscal, @ubigeo, @email, @telefono, @tipocliente, @estado);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            AddClienteParameters(cmd, c);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        // ── UPDATE ──────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Cliente c)
        {
            const string sql = @"
                UPDATE comercial.clientes
                SET    tipodocumento   = @tipodocumento,
                       numerodocumento = @numerodocumento,
                       razonsocial     = @razonsocial,
                       nombrecomercial = @nombrecomercial,
                       direccionfiscal = @direccionfiscal,
                       ubigeo          = @ubigeo,
                       email           = @email,
                       telefono        = @telefono,
                       tipocliente     = @tipocliente,
                       estado          = @estado
                WHERE  clienteid = @clienteid";

            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            AddClienteParameters(cmd, c);
            cmd.Parameters.AddWithValue("@clienteid", c.ClienteId);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── DELETE ──────────────────────────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM comercial.clientes WHERE clienteid = @clienteid";
            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@clienteid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── TOGGLE ESTADO ────────────────────────────────────────────────────────
        public async Task ToggleEstadoAsync(int id, bool estado)
        {
            const string sql = "UPDATE comercial.clientes SET estado = @estado WHERE clienteid = @clienteid";
            using var cn = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@estado", estado);
            cmd.Parameters.AddWithValue("@clienteid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── HELPERS ─────────────────────────────────────────────────────────────
        /// <summary>Agrega todos los parámetros de INSERT/UPDATE de manera segura.</summary>
        private static void AddClienteParameters(SqlCommand cmd, Cliente c)
        {
            cmd.Parameters.AddWithValue("@tipodocumento",   c.TipoDocumento);
            cmd.Parameters.AddWithValue("@numerodocumento", c.NumeroDocumento);
            cmd.Parameters.AddWithValue("@razonsocial",     c.RazonSocial);
            cmd.Parameters.AddWithValue("@nombrecomercial", (object?)c.NombreComercial ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@direccionfiscal", (object?)c.DireccionFiscal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ubigeo",          (object?)c.Ubigeo          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email",           (object?)c.Email            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefono",        (object?)c.Telefono         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tipocliente",     c.TipoCliente);
            cmd.Parameters.AddWithValue("@estado",          c.Estado);
        }

        /// <summary>Mapea una fila del SqlDataReader a un objeto Cliente.</summary>
        private static Cliente MapCliente(SqlDataReader rd) => new()
        {
            ClienteId           = rd.GetInt32(0),
            TipoDocumento       = rd.GetString(1),
            TipoDocumentoDesc   = rd.IsDBNull(2) ? "" : rd.GetString(2),
            NumeroDocumento     = rd.GetString(3),
            RazonSocial         = rd.GetString(4),
            NombreComercial     = rd.IsDBNull(5) ? null : rd.GetString(5),
            Email               = rd.IsDBNull(6) ? null : rd.GetString(6),
            Telefono            = rd.IsDBNull(7) ? null : rd.GetString(7),
            TipoCliente         = rd.IsDBNull(8) ? "prospecto" : rd.GetString(8),
            Estado              = rd.GetBoolean(9),
            DireccionCompletaUI = rd.IsDBNull(10) ? "" : rd.GetString(10),
        };
    }
}
