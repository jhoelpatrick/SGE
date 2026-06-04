using Npgsql;
using SGE.Models;

namespace SGE.Services
{
    public class VentaRepository : IVentaRepository
    {
        private readonly string _connectionString;

        public VentaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<List<PedidoVenta>> GetAllAsync()
        {
            var lista = new List<PedidoVenta>();
            const string sql = @"
                SELECT p.pedidoid, p.numeropedido, p.clienteid, p.proyectoid, p.fechaemision,
                       p.moneda, p.tipocambio, p.metodopago, p.cupondescuento,
                       p.montobruto, p.montodescuento, p.totalneto, p.estado,
                       c.razonsocial, c.numerodocumento, proj.nombreproyecto
                FROM   operaciones.pedidosventa p
                INNER JOIN comercial.clientes c ON p.clienteid = c.clienteid
                LEFT JOIN  operaciones.proyectos proj ON p.proyectoid = proj.proyectoid
                ORDER BY p.pedidoid DESC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new PedidoVenta
                {
                    PedidoId = rd.GetInt32(0),
                    NumeroPedido = rd.GetString(1),
                    ClienteId = rd.GetInt32(2),
                    ProyectoId = rd.IsDBNull(3) ? null : rd.GetInt32(3),
                    FechaEmision = rd.GetDateTime(4),
                    Moneda = rd.GetString(5),
                    TipoCambio = rd.GetDecimal(6),
                    MetodoPago = rd.IsDBNull(7) ? null : rd.GetString(7),
                    CuponDescuento = rd.IsDBNull(8) ? null : rd.GetString(8),
                    MontoBruto = rd.GetDecimal(9),
                    MontoDescuento = rd.GetDecimal(10),
                    TotalNeto = rd.GetDecimal(11),
                    Estado = rd.GetString(12),
                    ClienteNombre = rd.GetString(13),
                    ClienteRuc = rd.GetString(14),
                    ProyectoNombre = rd.IsDBNull(15) ? null : rd.GetString(15)
                });
            }
            return lista;
        }

        private class PedidoVentaAdapter : PedidoVenta { } // Just for resolving field mapping if needed, wait, we can map directly.

        public async Task<PedidoVenta?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.pedidoid, p.numeropedido, p.clienteid, p.proyectoid, p.fechaemision,
                       p.moneda, p.tipocambio, p.metodopago, p.cupondescuento,
                       p.montobruto, p.montodescuento, p.totalneto, p.estado,
                       c.razonsocial, c.numerodocumento, proj.nombreproyecto
                FROM   operaciones.pedidosventa p
                INNER JOIN comercial.clientes c ON p.clienteid = c.clienteid
                LEFT JOIN  operaciones.proyectos proj ON p.proyectoid = proj.proyectoid
                WHERE  p.pedidoid = @id";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rd = await cmd.ExecuteReaderAsync();

            if (await rd.ReadAsync())
            {
                var p = new PedidoVenta
                {
                    PedidoId = rd.GetInt32(0),
                    NumeroPedido = rd.GetString(1),
                    ClienteId = rd.GetInt32(2),
                    ProyectoId = rd.IsDBNull(3) ? null : rd.GetInt32(3),
                    FechaEmision = rd.GetDateTime(4),
                    Moneda = rd.GetString(5),
                    TipoCambio = rd.GetDecimal(6),
                    MetodoPago = rd.IsDBNull(7) ? null : rd.GetString(7),
                    CuponDescuento = rd.IsDBNull(8) ? null : rd.GetString(8),
                    MontoBruto = rd.GetDecimal(9),
                    MontoDescuento = rd.GetDecimal(10),
                    TotalNeto = rd.GetDecimal(11),
                    Estado = rd.GetString(12),
                    ClienteNombre = rd.GetString(13),
                    ClienteRuc = rd.GetString(14),
                    ProyectoNombre = rd.IsDBNull(15) ? null : rd.GetString(15)
                };
                rd.Close();
                p.Detalles = await GetDetalleByPedidoIdAsync(id);
                return p;
            }
            return null;
        }

        public async Task<int> CreateAsync(PedidoVenta p)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // 1. Insert header with temp number
                const string insertHeaderSql = @"
                    INSERT INTO operaciones.pedidosventa
                        (numeropedido, clienteid, proyectoid, fechaemision, moneda, tipocambio, metodopago, cupondescuento, montobruto, montodescuento, totalneto, estado)
                    VALUES
                        ('TEMP', @clienteid, @proyectoid, NOW(), @moneda, @tipocambio, @metodopago, @cupondescuento, @montobruto, @montodescuento, @totalneto, 'pendiente')
                    RETURNING pedidoid;";

                using var cmdHeader = new NpgsqlCommand(insertHeaderSql, cn, tx);
                cmdHeader.Parameters.AddWithValue("@clienteid", p.ClienteId);
                cmdHeader.Parameters.AddWithValue("@proyectoid", (object?)p.ProyectoId ?? DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@moneda", p.Moneda);
                cmdHeader.Parameters.AddWithValue("@tipocambio", p.Moneda == "USD" ? 3.7500m : 1.0000m);
                cmdHeader.Parameters.AddWithValue("@metodopago", (object?)p.MetodoPago ?? DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@cupondescuento", (object?)p.CuponDescuento ?? DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@montobruto", p.MontoBruto);
                cmdHeader.Parameters.AddWithValue("@montodescuento", p.MontoDescuento);
                cmdHeader.Parameters.AddWithValue("@totalneto", p.TotalNeto);

                int pedId = Convert.ToInt32(await cmdHeader.ExecuteScalarAsync());

                // 2. Format and update number: PED-2026-XXX
                string num = $"PED-2026-{pedId:D3}";
                const string updateNumSql = "UPDATE operaciones.pedidosventa SET numeropedido = @num WHERE pedidoid = @pedId";
                using var cmdUpdate = new NpgsqlCommand(updateNumSql, cn, tx);
                cmdUpdate.Parameters.AddWithValue("@num", num);
                cmdUpdate.Parameters.AddWithValue("@pedId", pedId);
                await cmdUpdate.ExecuteNonQueryAsync();

                // 3. Insert details
                foreach (var d in p.Detalles)
                {
                    const string insertDetailSql = @"
                        INSERT INTO operaciones.pedidosventadetalle
                            (pedidoid, productoid, cantidad, preciounitariocongiv, descuento, totalfila)
                        VALUES
                            (@pedidoid, @productoid, @cantidad, @precio, @descuento, @totalfila)";

                    using var cmdDetail = new NpgsqlCommand(insertDetailSql, cn, tx);
                    cmdDetail.Parameters.AddWithValue("@pedidoid", pedId);
                    cmdDetail.Parameters.AddWithValue("@productoid", d.ProductoId);
                    cmdDetail.Parameters.AddWithValue("@cantidad", d.Cantidad);
                    cmdDetail.Parameters.AddWithValue("@precio", d.PrecioUnitarioConGiv);
                    cmdDetail.Parameters.AddWithValue("@descuento", d.Descuento);
                    cmdDetail.Parameters.AddWithValue("@totalfila", d.TotalFila);
                    await cmdDetail.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return pedId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task ApproveAsync(int id)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // 1. Get order details and product info
                const string selectOrderSql = @"
                    SELECT p.numeropedido, d.productoid, d.cantidad, prod.esservicio, prod.costopromedio
                    FROM   operaciones.pedidosventa p
                    INNER JOIN operaciones.pedidosventadetalle d ON p.pedidoid = d.pedidoid
                    INNER JOIN comercial.productos prod ON d.productoid = prod.productoid
                    WHERE  p.pedidoid = @id AND p.estado = 'pendiente'";

                var itemsToProcess = new List<(string numPedido, int prodId, decimal qty, bool isServ, decimal cost)>();
                using (var cmdSelect = new NpgsqlCommand(selectOrderSql, cn, tx))
                {
                    cmdSelect.Parameters.AddWithValue("@id", id);
                    using var rd = await cmdSelect.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        itemsToProcess.Add((rd.GetString(0), rd.GetInt32(1), rd.GetDecimal(2), rd.GetBoolean(3), rd.GetDecimal(4)));
                    }
                }

                if (itemsToProcess.Count == 0)
                {
                    throw new InvalidOperationException("El pedido no existe o ya no está pendiente.");
                }

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

                string numeroPedido = itemsToProcess[0].numPedido;

                // 2. Process stock and kardex for physical products
                foreach (var item in itemsToProcess)
                {
                    if (item.isServ) continue;

                    // Guarantee stock entry exists
                    const string guaranteeStockSql = @"
                        INSERT INTO operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido)
                        VALUES (@almacenId, @prodId, 0.0000, 0.0000)
                        ON CONFLICT (almacenid, productoid) DO NOTHING;";
                    using (var cmdGuar = new NpgsqlCommand(guaranteeStockSql, cn, tx))
                    {
                        cmdGuar.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                        cmdGuar.Parameters.AddWithValue("@prodId", item.prodId);
                        await cmdGuar.ExecuteNonQueryAsync();
                    }

                    // Deduct stock
                    const string deductStockSql = @"
                        UPDATE operaciones.stockalmacen
                        SET    stockactual = stockactual - @qty
                        WHERE  almacenid = @almacenId AND productoid = @prodId";
                    using (var cmdDeduct = new NpgsqlCommand(deductStockSql, cn, tx))
                    {
                        cmdDeduct.Parameters.AddWithValue("@qty", item.qty);
                        cmdDeduct.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                        cmdDeduct.Parameters.AddWithValue("@prodId", item.prodId);
                        await cmdDeduct.ExecuteNonQueryAsync();
                    }

                    // Log in Kardex
                    const string logKardexSql = @"
                        INSERT INTO operaciones.kardexmovimientos
                            (almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento)
                        VALUES
                            (@almacenId, @prodId, 'sal', 'Despacho Venta ' || @numPedido, @numPedido, @qty, @cost, NOW())";
                    using (var cmdKardex = new NpgsqlCommand(logKardexSql, cn, tx))
                    {
                        cmdKardex.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                        cmdKardex.Parameters.AddWithValue("@prodId", item.prodId);
                        cmdKardex.Parameters.AddWithValue("@numPedido", numeroPedido);
                        cmdKardex.Parameters.AddWithValue("@qty", item.qty);
                        cmdKardex.Parameters.AddWithValue("@cost", item.cost);
                        await cmdKardex.ExecuteNonQueryAsync();
                    }
                }

                // 3. Update order state to approved
                const string updateStateSql = "UPDATE operaciones.pedidosventa SET estado = 'aprobado' WHERE pedidoid = @id";
                using (var cmdUpdate = new NpgsqlCommand(updateStateSql, cn, tx))
                {
                    cmdUpdate.Parameters.AddWithValue("@id", id);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task CancelAsync(int id)
        {
            const string sql = "UPDATE operaciones.pedidosventa SET estado = 'cancelado' WHERE pedidoid = @id AND estado = 'pendiente'";
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DispatchAsync(int pedId, int vehId, int condId, string serie, string corr)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // Update Pedido status
                const string updatePedidoSql = "UPDATE operaciones.pedidosventa SET estado = 'despachado' WHERE pedidoid = @pedId AND estado = 'aprobado'";
                using var cmdUpdate = new NpgsqlCommand(updatePedidoSql, cn, tx);
                cmdUpdate.Parameters.AddWithValue("@pedId", pedId);
                int updated = await cmdUpdate.ExecuteNonQueryAsync();
                if (updated == 0)
                {
                    throw new InvalidOperationException("El pedido no se encuentra aprobado o no existe.");
                }

                // Create Guía Remisión
                const string insertGuiaSql = @"
                    INSERT INTO operaciones.guiasremision
                        (serie, correlativo, motivotraslado, fechaemision, almacenorigenid, vehiculoid, conductorid, pesototal, estadosunat)
                    VALUES
                        (@serie, @correlativo, '01', NOW(), 1, @vehId, @condId, 100.0, 'aceptado')";
                using var cmdGuia = new NpgsqlCommand(insertGuiaSql, cn, tx);
                cmdGuia.Parameters.AddWithValue("@serie", serie);
                cmdGuia.Parameters.AddWithValue("@correlativo", corr);
                cmdGuia.Parameters.AddWithValue("@vehId", vehId);
                cmdGuia.Parameters.AddWithValue("@condId", condId);
                await cmdGuia.ExecuteNonQueryAsync();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<List<PedidoVentaDetalle>> GetDetalleByPedidoIdAsync(int pedidoId)
        {
            var lista = new List<PedidoVentaDetalle>();
            const string sql = @"
                SELECT d.detalledid, d.pedidoid, d.productoid, d.cantidad, d.preciounitariocongiv,
                       d.descuento, d.totalfila, prod.descripcion, prod.codigosku, prod.unidadmedida
                FROM   operaciones.pedidosventadetalle d
                INNER JOIN comercial.productos prod ON d.productoid = prod.productoid
                WHERE  d.pedidoid = @pedidoId
                ORDER BY d.detalledid";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new PedidoVentaDetalle
                {
                    DetalleId = rd.GetInt32(0),
                    PedidoId = rd.GetInt32(1),
                    ProductoId = rd.GetInt32(2),
                    Cantidad = rd.GetDecimal(3),
                    PrecioUnitarioConGiv = rd.GetDecimal(4),
                    Descuento = rd.GetDecimal(5),
                    TotalFila = rd.GetDecimal(6),
                    ProductoDescripcion = rd.GetString(7),
                    ProductoSku = rd.GetString(8),
                    UnidadMedida = rd.GetString(9)
                });
            }
            return lista;
        }
    }
}
