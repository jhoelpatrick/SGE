using Npgsql;
using SGE.Models;

namespace SGE.Services
{
    public class InventarioRepository : IInventarioRepository
    {
        private readonly string _connectionString;

        public InventarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<List<Producto>> GetStockSummaryAsync()
        {
            var lista = new List<Producto>();
            const string sql = @"
                SELECT p.productoid, p.codigosku, p.codigosunat, p.descripcion, p.unidadmedida,
                       p.tipoafectacionigv, p.precioventasugerido, p.costopromedio,
                       p.esservicio, p.sevende, p.nosevende, p.sefabrica, p.estado,
                       COALESCE(SUM(s.stockactual), 0.0000) AS stockactual,
                       COALESCE(MAX(s.stockcomprometido), 0.0000) AS stockcomprometido
                FROM   comercial.productos p
                LEFT JOIN operaciones.stockalmacen s ON p.productoid = s.productoid
                WHERE  p.estado = true
                GROUP BY p.productoid, p.codigosku, p.codigosunat, p.descripcion, p.unidadmedida,
                         p.tipoafectacionigv, p.precioventasugerido, p.costopromedio,
                         p.esservicio, p.sevende, p.nosevende, p.sefabrica, p.estado
                ORDER BY p.codigosku";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new Producto
                {
                    ProductoId = rd.GetInt32(0),
                    CodigoSku = rd.GetString(1),
                    CodigoSunat = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Descripcion = rd.GetString(3),
                    UnidadMedida = rd.GetString(4),
                    TipoAfectacionIgv = rd.GetString(5),
                    PrecioVentaSugerido = rd.GetDecimal(6),
                    CostoPromedio = rd.GetDecimal(7),
                    EsServicio = rd.GetBoolean(8),
                    SeVende = rd.GetBoolean(9),
                    NoSeVende = rd.GetBoolean(10),
                    SeFabrica = rd.GetBoolean(11),
                    Estado = rd.GetBoolean(12),
                    StockActual = rd.GetDecimal(13),
                    StockMinimo = 10m // Default minimum
                });
            }
            return lista;
        }

        public async Task<List<KardexMovimiento>> GetKardexByProductoIdAsync(int productoId)
        {
            var lista = new List<KardexMovimiento>();
            const string sql = @"
                SELECT movimientoid, almacenid, productoid, tipomovimiento, conceptomovimiento,
                       documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento,
                       prod.descripcion, prod.codigosku
                FROM   operaciones.kardexmovimientos km
                INNER JOIN comercial.productos prod ON km.productoid = prod.productoid
                WHERE  km.productoid = @productoId
                ORDER BY km.fechamovimiento ASC, km.movimientoid ASC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@productoId", productoId);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new KardexMovimiento
                {
                    MovimientoId = rd.GetInt64(0),
                    AlmacenId = rd.GetInt32(1),
                    ProductoId = rd.GetInt32(2),
                    TipoMovimiento = rd.GetString(3),
                    ConceptoMovimiento = rd.GetString(4),
                    DocumentoReferencia = rd.IsDBNull(5) ? null : rd.GetString(5),
                    Cantidad = rd.GetDecimal(6),
                    CostoUnitarioMovimiento = rd.GetDecimal(7),
                    FechaMovimiento = rd.GetDateTime(8),
                    ProductoDescripcion = rd.GetString(9),
                    ProductoSku = rd.GetString(10)
                });
            }

            // Calculate running balance chronologically
            decimal runningBalance = 0;
            foreach (var mov in lista)
            {
                string t = mov.TipoMovimiento.ToLower();
                if (t == "ent" || t == "ingreso")
                {
                    runningBalance += mov.Cantidad;
                }
                else if (t == "sal" || t == "salida")
                {
                    runningBalance -= mov.Cantidad;
                }
                mov.SaldoPosterior = runningBalance;
            }

            // Reverse to display newest first
            lista.Reverse();
            return lista;
        }

        public async Task RegistrarMovimientoManualAsync(int productoId, string tipoMovimiento, decimal cantidad, string motivo)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // Obtener primer almacén disponible, o insertar uno por defecto si no hay ninguno.
                const string sqlGetAlmacen = "SELECT almacenid FROM operaciones.almacenes LIMIT 1;";
                using var cmdGetAlm = new NpgsqlCommand(sqlGetAlmacen, cn, tx);
                var almObj = await cmdGetAlm.ExecuteScalarAsync();
                int targetAlmacenId = 1;
                if (almObj != null)
                {
                    targetAlmacenId = Convert.ToInt32(almObj);
                }
                else
                {
                    const string sqlInsertAlm = @"
                        INSERT INTO operaciones.almacenes (almacenid, codigoalmacen, nombre, direccion, ubigeo, estado)
                        VALUES (1, 'ALM-01', 'Almacén Central', 'Dirección Central', '150101', true)
                        ON CONFLICT (almacenid) DO NOTHING;";
                    using var cmdInsAlm = new NpgsqlCommand(sqlInsertAlm, cn, tx);
                    await cmdInsAlm.ExecuteNonQueryAsync();
                }

                // 1. Guarantee stock entry exists
                const string guaranteeStockSql = @"
                    INSERT INTO operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido)
                    VALUES (@almacenId, @prodId, 0.0000, 0.0000)
                    ON CONFLICT (almacenid, productoid) DO NOTHING;";
                using (var cmdGuar = new NpgsqlCommand(guaranteeStockSql, cn, tx))
                {
                    cmdGuar.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                    cmdGuar.Parameters.AddWithValue("@prodId", productoId);
                    await cmdGuar.ExecuteNonQueryAsync();
                }

                // 2. Fetch current stock and product info
                const string getStockSql = @"
                    SELECT s.stockactual, prod.costopromedio, prod.esservicio
                    FROM   operaciones.stockalmacen s
                    INNER JOIN comercial.productos prod ON s.productoid = prod.productoid
                    WHERE  s.almacenid = @almacenId AND s.productoid = @prodId";

                decimal currentStock = 0;
                decimal cost = 0;
                bool isServ = false;

                using (var cmdSelect = new NpgsqlCommand(getStockSql, cn, tx))
                {
                    cmdSelect.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                    cmdSelect.Parameters.AddWithValue("@prodId", productoId);
                    using var rd = await cmdSelect.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        currentStock = rd.GetDecimal(0);
                        cost = rd.GetDecimal(1);
                        isServ = rd.GetBoolean(2);
                    }
                    else
                    {
                        throw new InvalidOperationException("El producto no existe o no tiene registro de stock.");
                    }
                }

                string typeLower = tipoMovimiento.ToLower();
                bool isSalida = typeLower == "sal" || typeLower == "salida";

                if (isSalida && currentStock < cantidad)
                {
                    throw new InvalidOperationException("Stock insuficiente para realizar la salida.");
                }

                // 3. Update stock
                const string updateStockSql = @"
                    UPDATE operaciones.stockalmacen
                    SET    stockactual = stockactual + @qty
                    WHERE  almacenid = @almacenId AND productoid = @prodId";

                decimal qtyAdjust = isSalida ? -cantidad : cantidad;

                using (var cmdUpdate = new NpgsqlCommand(updateStockSql, cn, tx))
                {
                    cmdUpdate.Parameters.AddWithValue("@qty", qtyAdjust);
                    cmdUpdate.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                    cmdUpdate.Parameters.AddWithValue("@prodId", productoId);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                // 4. Log in Kardex
                const string logKardexSql = @"
                    INSERT INTO operaciones.kardexmovimientos
                        (almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento)
                    VALUES
                        (@almacenId, @prodId, @type, @reason, 'AJUSTE', @qty, @cost, NOW())";
                using (var cmdKardex = new NpgsqlCommand(logKardexSql, cn, tx))
                {
                    cmdKardex.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                    cmdKardex.Parameters.AddWithValue("@prodId", productoId);
                    cmdKardex.Parameters.AddWithValue("@type", isSalida ? "sal" : "ent");
                    cmdKardex.Parameters.AddWithValue("@reason", motivo);
                    cmdKardex.Parameters.AddWithValue("@qty", cantidad);
                    cmdKardex.Parameters.AddWithValue("@cost", cost);
                    await cmdKardex.ExecuteNonQueryAsync();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
