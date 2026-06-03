using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Services
{
    /// <summary>
    /// Repositorio de Productos. Accede a comercial.productos en sge_crm.
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        private readonly string _connectionString;

        public ProductoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'DefaultConnection' en appsettings.json.");
        }

        // ── GET ALL ─────────────────────────────────────────────────────────────
        public async Task<List<Producto>> GetAllAsync()
        {
            var lista = new List<Producto>();
            const string sql = @"
                SELECT p.productoid, p.codigosku, p.codigosunat, p.descripcion, p.unidadmedida,
                       p.tipoafectacionigv, p.precioventasugerido, p.costopromedio,
                       p.esservicio, p.sevende, p.nosevende, p.sefabrica, p.estado,
                       ISNULL((SELECT SUM(stockactual) FROM operaciones.stockalmacen WHERE productoid = p.productoid), 0) AS stockactual,
                       10 AS stockminimo
                FROM   comercial.productos p
                ORDER BY p.descripcion";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            using var rd  = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(MapProducto(rd));
            }
            return lista;
        }

        // ── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<Producto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.productoid, p.codigosku, p.codigosunat, p.descripcion, p.unidadmedida,
                       p.tipoafectacionigv, p.precioventasugerido, p.costopromedio,
                       p.esservicio, p.sevende, p.nosevende, p.sefabrica, p.estado,
                       ISNULL((SELECT SUM(stockactual) FROM operaciones.stockalmacen WHERE productoid = p.productoid), 0) AS stockactual,
                       10 AS stockminimo
                FROM   comercial.productos p
                WHERE  p.productoid = @productoid";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@productoid", id);
            using var rd  = await cmd.ExecuteReaderAsync();

            return await rd.ReadAsync() ? MapProducto(rd) : null;
        }

        // ── CREATE ──────────────────────────────────────────────────────────────
        public async Task<int> CreateAsync(Producto p)
        {
            const string sql = @"
                INSERT INTO comercial.productos
                    (codigosku, codigosunat, descripcion, unidadmedida,
                     tipoafectacionigv, precioventasugerido, costopromedio,
                     esservicio, sevende, nosevende, sefabrica, estado)
                VALUES
                    (@codigosku, @codigosunat, @descripcion, @unidadmedida,
                     @tipoafectacionigv, @precioventasugerido, @costopromedio,
                     @esservicio, @sevende, @nosevende, @sefabrica, @estado);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx  = await cn.BeginTransactionAsync();
            try
            {
                using var cmd = new SqlCommand(sql, cn, (SqlTransaction)tx);
                AddProductoParameters(cmd, p);

                var result = await cmd.ExecuteScalarAsync();
                int newId = result != null ? (int)result : 0;

                if (newId > 0 && !p.EsServicio && p.StockActual > 0)
                {
                    const string sqlStock = @"
                        INSERT INTO operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido)
                        VALUES (1, @productoid, @stockactual, 0);";
                    using var cmdStock = new SqlCommand(sqlStock, cn, (SqlTransaction)tx);
                    cmdStock.Parameters.AddWithValue("@productoid", newId);
                    cmdStock.Parameters.AddWithValue("@stockactual", p.StockActual);
                    await cmdStock.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return newId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ── UPDATE ──────────────────────────────────────────────────────────────
        public async Task UpdateAsync(Producto p)
        {
            const string sql = @"
                UPDATE comercial.productos
                SET    codigosku            = @codigosku,
                       codigosunat          = @codigosunat,
                       descripcion          = @descripcion,
                       unidadmedida         = @unidadmedida,
                       tipoafectacionigv    = @tipoafectacionigv,
                       precioventasugerido  = @precioventasugerido,
                       costopromedio        = @costopromedio,
                       esservicio           = @esservicio,
                       sevende              = @sevende,
                       nosevende            = @nosevende,
                       sefabrica            = @sefabrica,
                       estado               = @estado
                WHERE  productoid = @productoid";

            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            AddProductoParameters(cmd, p);
            cmd.Parameters.AddWithValue("@productoid", p.ProductoId);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── DELETE ──────────────────────────────────────────────────────────────
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM comercial.productos WHERE productoid = @productoid";
            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@productoid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── TOGGLE ESTADO ────────────────────────────────────────────────────────
        public async Task ToggleEstadoAsync(int id, bool estado)
        {
            const string sql = "UPDATE comercial.productos SET estado = @estado WHERE productoid = @productoid";
            using var cn  = new SqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@estado",     estado);
            cmd.Parameters.AddWithValue("@productoid", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── HELPERS ─────────────────────────────────────────────────────────────
        private static void AddProductoParameters(SqlCommand cmd, Producto p)
        {
            cmd.Parameters.AddWithValue("@codigosku",           p.CodigoSku);
            cmd.Parameters.AddWithValue("@codigosunat",         (object?)p.CodigoSunat ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@descripcion",         p.Descripcion);
            cmd.Parameters.AddWithValue("@unidadmedida",        p.UnidadMedida);
            cmd.Parameters.AddWithValue("@tipoafectacionigv",   p.TipoAfectacionIgv);
            cmd.Parameters.AddWithValue("@precioventasugerido", p.PrecioVentaSugerido);
            cmd.Parameters.AddWithValue("@costopromedio",       p.CostoPromedio);
            cmd.Parameters.AddWithValue("@esservicio",          p.EsServicio);
            cmd.Parameters.AddWithValue("@sevende",             p.SeVende);
            cmd.Parameters.AddWithValue("@nosevende",           p.NoSeVende);
            cmd.Parameters.AddWithValue("@sefabrica",           p.SeFabrica);
            cmd.Parameters.AddWithValue("@estado",              p.Estado);
        }

        private static Producto MapProducto(SqlDataReader rd) => new()
        {
            ProductoId          = rd.GetInt32(0),
            CodigoSku           = rd.GetString(1),
            CodigoSunat         = rd.IsDBNull(2) ? null : rd.GetString(2),
            Descripcion         = rd.GetString(3),
            UnidadMedida        = rd.GetString(4),
            TipoAfectacionIgv   = rd.IsDBNull(5) ? "10" : rd.GetString(5),
            PrecioVentaSugerido = rd.GetDecimal(6),
            CostoPromedio       = rd.GetDecimal(7),
            EsServicio          = rd.GetBoolean(8),
            SeVende             = rd.GetBoolean(9),
            NoSeVende           = rd.GetBoolean(10),
            SeFabrica           = rd.GetBoolean(11),
            Estado              = rd.GetBoolean(12),
            StockActual         = rd.GetDecimal(13),
            StockMinimo         = rd.GetDecimal(14),
        };
    }
}
