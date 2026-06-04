using Npgsql;
using SGE.Models;

namespace SGE.Services
{
    public class CompraRepository : ICompraRepository
    {
        private readonly string _connectionString;

        public CompraRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontrÃƒÂ³ la cadena de conexiÃƒÂ³n 'DefaultConnection'.");
        }

        public async Task<List<OrdenCompra>> GetAllAsync()
        {
            var lista = new List<OrdenCompra>();
            const string sql = @"
                SELECT o.ordenid, o.numeroorden, o.proveedorid, o.proyectoid, o.solicitante,
                       o.fechaemision, o.moneda, o.monto_total, o.categoriagasto, o.estado,
                       p.razonsocial, p.numerodocumento, proj.nombreproyecto
                FROM   operaciones.ordenescompra o
                INNER JOIN comercial.proveedores p ON o.proveedorid = p.proveedorid
                LEFT JOIN  operaciones.proyectos proj ON o.proyectoid = proj.proyectoid
                ORDER BY o.ordenid DESC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new OrdenCompra
                {
                    OrdenId = rd.GetInt32(0),
                    NumeroOrden = rd.GetString(1),
                    ProveedorId = rd.GetInt32(2),
                    ProyectoId = rd.IsDBNull(3) ? null : rd.GetInt32(3),
                    Solicitante = rd.IsDBNull(4) ? null : rd.GetString(4),
                    FechaEmision = rd.GetDateTime(5),
                    Moneda = rd.GetString(6),
                    MontoTotal = rd.GetDecimal(7),
                    CategoriaGasto = rd.GetString(8),
                    Estado = rd.GetString(9),
                    ProveedorNombre = rd.GetString(10),
                    ProveedorRuc = rd.GetString(11),
                    ProyectoNombre = rd.IsDBNull(12) ? null : rd.GetString(12)
                });
            }
            return lista;
        }

        public async Task<OrdenCompra?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT o.ordenid, o.numeroorden, o.proveedorid, o.proyectoid, o.solicitante,
                       o.fechaemision, o.moneda, o.monto_total, o.categoriagasto, o.estado,
                       p.razonsocial, p.numerodocumento, proj.nombreproyecto
                FROM   operaciones.ordenescompra o
                INNER JOIN comercial.proveedores p ON o.proveedorid = p.proveedorid
                LEFT JOIN  operaciones.proyectos proj ON o.proyectoid = proj.proyectoid
                WHERE  o.ordenid = @id";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            using var rd = await cmd.ExecuteReaderAsync();

            if (await rd.ReadAsync())
            {
                var o = new OrdenCompra
                {
                    OrdenId = rd.GetInt32(0),
                    NumeroOrden = rd.GetString(1),
                    ProveedorId = rd.GetInt32(2),
                    ProyectoId = rd.IsDBNull(3) ? null : rd.GetInt32(3),
                    Solicitante = rd.IsDBNull(4) ? null : rd.GetString(4),
                    FechaEmision = rd.GetDateTime(5),
                    Moneda = rd.GetString(6),
                    MontoTotal = rd.GetDecimal(7),
                    CategoriaGasto = rd.GetString(8),
                    Estado = rd.GetString(9),
                    ProveedorNombre = rd.GetString(10),
                    ProveedorRuc = rd.GetString(11),
                    ProyectoNombre = rd.IsDBNull(12) ? null : rd.GetString(12)
                };
                rd.Close();
                o.Detalles = await GetDetalleByOrdenIdAsync(id);
                return o;
            }
            return null;
        }

        public async Task<int> CreateAsync(OrdenCompra o)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // 1. Insert header
                const string insertHeaderSql = @"
                    INSERT INTO operaciones.ordenescompra
                        (numeroorden, proveedorid, proyectoid, solicitante, fechaemision, moneda, monto_total, categoriagasto, estado)
                    VALUES
                        ('TEMP', @proveedorid, @proyectoid, @solicitante, NOW(), @moneda, @monto_total, @categoriagasto, 'pendiente')
                    RETURNING ordenid;";

                using var cmdHeader = new NpgsqlCommand(insertHeaderSql, cn, tx);
                cmdHeader.Parameters.AddWithValue("@proveedorid", o.ProveedorId);
                cmdHeader.Parameters.AddWithValue("@proyectoid", (object?)o.ProyectoId ?? DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@solicitante", (object?)o.Solicitante ?? DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@moneda", o.Moneda);
                cmdHeader.Parameters.AddWithValue("@monto_total", o.MontoTotal);
                cmdHeader.Parameters.AddWithValue("@categoriagasto", o.CategoriaGasto);

                int ordenId = Convert.ToInt32(await cmdHeader.ExecuteScalarAsync());

                // 2. Format and update number: OC-2026-XXX
                string num = $"OC-2026-{ordenId:D3}";
                const string updateNumSql = "UPDATE operaciones.ordenescompra SET numeroorden = @num WHERE ordenid = @ordenId";
                using var cmdUpdate = new NpgsqlCommand(updateNumSql, cn, tx);
                cmdUpdate.Parameters.AddWithValue("@num", num);
                cmdUpdate.Parameters.AddWithValue("@ordenId", ordenId);
                await cmdUpdate.ExecuteNonQueryAsync();

                // 3. Insert details
                foreach (var d in o.Detalles)
                {
                    const string insertDetailSql = @"
                        INSERT INTO operaciones.ordenescompradetalle
                            (ordenid, productoid, cantidad, costounitariocongiv, totalfila)
                        VALUES
                            (@ordenid, @productoid, @cantidad, @costo, @totalfila)";

                    using var cmdDetail = new NpgsqlCommand(insertDetailSql, cn, tx);
                    cmdDetail.Parameters.AddWithValue("@ordenid", ordenId);
                    cmdDetail.Parameters.AddWithValue("@productoid", d.ProductoId);
                    cmdDetail.Parameters.AddWithValue("@cantidad", d.Cantidad);
                    cmdDetail.Parameters.AddWithValue("@costo", d.CostoUnitarioConGiv);
                    cmdDetail.Parameters.AddWithValue("@totalfila", d.TotalFila);
                    await cmdDetail.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return ordenId;
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
                // 1. Get purchase order details and product info
                const string selectOrderSql = @"
                    SELECT o.numeroorden, d.productoid, d.cantidad, prod.esservicio, o.proyectoid, o.monto_total
                    FROM   operaciones.ordenescompra o
                    INNER JOIN operaciones.ordenescompradetalle d ON o.ordenid = d.ordenid
                    INNER JOIN comercial.productos prod ON d.productoid = prod.productoid
                    WHERE  o.ordenid = @id AND o.estado = 'pendiente'";

                var itemsToProcess = new List<(string numOrden, int prodId, decimal qty, bool isServ, int? projId, decimal amount)>();
                using (var cmdSelect = new NpgsqlCommand(selectOrderSql, cn, tx))
                {
                    cmdSelect.Parameters.AddWithValue("@id", id);
                    using var rd = await cmdSelect.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        itemsToProcess.Add((
                            rd.GetString(0),
                            rd.GetInt32(1),
                            rd.GetDecimal(2),
                            rd.GetBoolean(3),
                            rd.IsDBNull(4) ? null : rd.GetInt32(4),
                            rd.GetDecimal(5)
                        ));
                    }
                }

                if (itemsToProcess.Count == 0)
                {
                    throw new InvalidOperationException("La orden de compra no existe o ya no estÃƒÂ¡ pendiente.");
                }

                string numeroOrden = itemsToProcess[0].numOrden;
                int? proyectoId = itemsToProcess[0].projId;
                decimal montoTotal = itemsToProcess[0].amount;

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

                    // Add stock
                    const string addStockSql = @"
                        UPDATE operaciones.stockalmacen
                        SET    stockactual = stockactual + @qty
                        WHERE  almacenid = @almacenId AND productoid = @prodId";
                    using (var cmdAdd = new NpgsqlCommand(addStockSql, cn, tx))
                    {
                        cmdAdd.Parameters.AddWithValue("@qty", item.qty);
                        cmdAdd.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                        cmdAdd.Parameters.AddWithValue("@prodId", item.prodId);
                        await cmdAdd.ExecuteNonQueryAsync();
                    }

                    // Log in Kardex
                    // Costo unitario promedio is mapped from costopromedio, but here we can log costounitariomovimiento based on costounitariocongiv
                    // To fetch costounitariocongiv, we select it from detail
                    const string getDetailCostSql = @"
                        SELECT costounitariocongiv FROM operaciones.ordenescompradetalle
                        WHERE  ordenid = @id AND productoid = @prodId";
                    decimal costUnit = 0;
                    using (var cmdCost = new NpgsqlCommand(getDetailCostSql, cn, tx))
                    {
                        cmdCost.Parameters.AddWithValue("@id", id);
                        cmdCost.Parameters.AddWithValue("@prodId", item.prodId);
                        var costObj = await cmdCost.ExecuteScalarAsync();
                        costUnit = costObj != null ? (decimal)costObj : 0m;
                    }

                    const string logKardexSql = @"
                        INSERT INTO operaciones.kardexmovimientos
                            (almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento)
                        VALUES
                            (@almacenId, @prodId, 'ent', 'Compra según ' || @numOrden, @numOrden, @qty, @cost, NOW())";
                    using (var cmdKardex = new NpgsqlCommand(logKardexSql, cn, tx))
                    {
                        cmdKardex.Parameters.AddWithValue("@almacenId", targetAlmacenId);
                        cmdKardex.Parameters.AddWithValue("@prodId", item.prodId);
                        cmdKardex.Parameters.AddWithValue("@numOrden", numeroOrden);
                        cmdKardex.Parameters.AddWithValue("@qty", item.qty);
                        cmdKardex.Parameters.AddWithValue("@cost", costUnit);
                        await cmdKardex.ExecuteNonQueryAsync();
                    }
                }

                // 3. Update project cost
                if (proyectoId.HasValue)
                {
                    const string updateProjectSql = @"
                        UPDATE operaciones.proyectos
                        SET    costoreallogrado = costoreallogrado + @monto
                        WHERE  proyectoid = @proyectoId";
                    using var cmdProj = new NpgsqlCommand(updateProjectSql, cn, tx);
                    cmdProj.Parameters.AddWithValue("@monto", montoTotal);
                    cmdProj.Parameters.AddWithValue("@proyectoId", proyectoId.Value);
                    await cmdProj.ExecuteNonQueryAsync();
                }

                // 4. Update order state to approved
                const string updateStateSql = "UPDATE operaciones.ordenescompra SET estado = 'aprobado' WHERE ordenid = @id";
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

        public async Task RejectAsync(int id)
        {
            const string sql = "UPDATE operaciones.ordenescompra SET estado = 'rechazado' WHERE ordenid = @id AND estado = 'pendiente'";
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<OrdenCompraDetalle>> GetDetalleByOrdenIdAsync(int ordenId)
        {
            var lista = new List<OrdenCompraDetalle>();
            const string sql = @"
                SELECT d.detalledoc, d.ordenid, d.productoid, d.cantidad, d.costounitariocongiv,
                       d.totalfila, prod.descripcion, prod.codigosku
                FROM   operaciones.ordenescompradetalle d
                INNER JOIN comercial.productos prod ON d.productoid = prod.productoid
                WHERE  d.ordenid = @ordenId
                ORDER BY d.detalledoc";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@ordenId", ordenId);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new OrdenCompraDetalle
                {
                    DetalleDoc = rd.GetInt32(0),
                    OrdenId = rd.GetInt32(1),
                    ProductoId = rd.GetInt32(2),
                    Cantidad = rd.GetDecimal(3),
                    CostoUnitarioConGiv = rd.GetDecimal(4),
                    TotalFila = rd.GetDecimal(5),
                    ProductoDescripcion = rd.GetString(6),
                    ProductoSku = rd.GetString(7)
                });
            }
            return lista;
        }
    }
}
