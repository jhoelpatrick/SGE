﻿using Npgsql;
using SGE.Models;

namespace SGE.Services
{
    public class FacturacionRepository : IFacturacionRepository
    {
        private readonly string _connectionString;

        public FacturacionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontrÃƒÂ³ la cadena de conexiÃƒÂ³n 'DefaultConnection'.");
        }

        public async Task<List<ComprobanteFacturacion>> GetAllInvoicesAsync()
        {
            var lista = new List<ComprobanteFacturacion>();
            const string sql = @"
                SELECT cf.comprobanteid, cf.pedidoid, cf.tipocomprobante, cf.serie, cf.correlativo,
                       cf.fechaemision, cf.tipooperacionsunat, cf.clienteid, cf.moneda,
                       cf.opgravada, cf.opinafecta, cf.opexonerada, cf.igv_total, cf.importetotalneto,
                       cf.tipoimpuestoespecial, cf.estadosunat,
                       c.razonsocial, c.numerodocumento, p.numeropedido
                FROM   operaciones.comprobantesfacturacion cf
                INNER JOIN comercial.clientes c ON cf.clienteid = c.clienteid
                LEFT JOIN  operaciones.pedidosventa p ON cf.pedidoid = p.pedidoid
                ORDER BY cf.comprobanteid DESC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new ComprobanteFacturacion
                {
                    ComprobanteId = rd.GetInt32(0),
                    PedidoId = rd.IsDBNull(1) ? null : rd.GetInt32(1),
                    TipoComprobante = rd.GetString(2),
                    Serie = rd.GetString(3),
                    Correlativo = rd.GetString(4),
                    FechaEmision = rd.GetDateTime(5),
                    TipoOperacionSunat = rd.GetString(6),
                    ClienteId = rd.GetInt32(7),
                    Moneda = rd.GetString(8),
                    OpGravada = rd.GetDecimal(9),
                    OpInafecta = rd.GetDecimal(10),
                    OpExonerada = rd.GetDecimal(11),
                    IgvTotal = rd.GetDecimal(12),
                    ImporteTotalNeto = rd.GetDecimal(13),
                    TipoImpuestoEspecial = rd.GetString(14),
                    EstadoSunat = rd.GetString(15),
                    ClienteNombre = rd.GetString(16),
                    ClienteRuc = rd.GetString(17),
                    PedidoNumero = rd.IsDBNull(18) ? "" : rd.GetString(18)
                });
            }
            return lista;
        }

        public async Task<List<GuiaRemision>> GetAllGuidesAsync()
        {
            var lista = new List<GuiaRemision>();
            const string sql = @"
                SELECT g.guiaid, g.serie, g.correlativo, g.fechaemision, g.motivotraslado,
                       g.almacenorigenid, g.almacendestinoid, g.proveedorid, g.vehiculoid, g.conductorid,
                       g.pesototal, g.unidadmedidapeso, g.estadosunat,
                       v.placa, v.marca, cond.nombre, cond.numerodocumento
                FROM   operaciones.guiasremision g
                LEFT JOIN comercial.vehiculosproveedores v ON g.vehiculoid = v.vehiculoid
                LEFT JOIN comercial.conductoresproveedores cond ON g.conductorid = cond.conductorid
                ORDER BY g.guiaid DESC";

            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                lista.Add(new GuiaRemision
                {
                    GuiaId = rd.GetInt32(0),
                    Serie = rd.GetString(1),
                    Correlativo = rd.GetString(2),
                    FechaEmision = rd.GetDateTime(3),
                    MotivoTraslado = rd.GetString(4),
                    AlmacenOrigenId = rd.GetInt32(5),
                    AlmacenDestinoId = rd.IsDBNull(6) ? null : rd.GetInt32(6),
                    ProveedorId = rd.IsDBNull(7) ? null : rd.GetInt32(7),
                    VehiculoId = rd.IsDBNull(8) ? null : rd.GetInt32(8),
                    ConductorId = rd.IsDBNull(9) ? null : rd.GetInt32(9),
                    PesoTotal = rd.GetDecimal(10),
                    UnidadMedidaPeso = rd.GetString(11),
                    EstadoSunat = rd.GetString(12),
                    VehiculoPlaca = rd.IsDBNull(13) ? "" : rd.GetString(13),
                    VehiculoMarca = rd.IsDBNull(14) ? "" : rd.GetString(14),
                    ConductorNombre = rd.IsDBNull(15) ? "" : rd.GetString(15),
                    ConductorDni = rd.IsDBNull(16) ? "" : rd.GetString(16),
                    MotivoTrasladoDesc = rd.GetString(4) == "01" ? "Venta" : rd.GetString(4) == "02" ? "Compra" : "Traslado"
                });
            }
            return lista;
        }

        public async Task<int> EmitirFacturaDesdePedidoAsync(int pedidoId, string tipoComprobante, string serie)
        {
            using var cn = new NpgsqlConnection(_connectionString);
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // 1. Get order information
                const string selectOrderSql = "SELECT clienteid, totalneto, moneda FROM operaciones.pedidosventa WHERE pedidoid = @pedidoId";
                int clienteId = 0;
                decimal totalNeto = 0;
                string moneda = "PEN";

                using (var cmdOrder = new NpgsqlCommand(selectOrderSql, cn, tx))
                {
                    cmdOrder.Parameters.AddWithValue("@pedidoId", pedidoId);
                    using var rd = await cmdOrder.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        clienteId = rd.GetInt32(0);
                        totalNeto = rd.GetDecimal(1);
                        moneda = rd.GetString(2);
                    }
                    else
                    {
                        throw new InvalidOperationException("El pedido no existe.");
                    }
                }

                // 2. Generate new comprobante ID and correlativo
                const string selectMaxIdSql = "SELECT COALESCE(MAX(comprobanteid), 0) + 1 FROM operaciones.comprobantesfacturacion";
                int nextId = 1;
                using (var cmdMax = new NpgsqlCommand(selectMaxIdSql, cn, tx))
                {
                    nextId = Convert.ToInt32(await cmdMax.ExecuteScalarAsync());
                }
                string correlativo = nextId.ToString().PadLeft(8, '0');

                // 3. Insert comprobante
                decimal opGravada = totalNeto / 1.18m;
                decimal igvTotal = totalNeto - opGravada;

                const string insertComprobanteSql = @"
                    INSERT INTO operaciones.comprobantesfacturacion
                        (pedidoid, tipocomprobante, serie, correlativo, fechaemision, tipooperacionsunat, clienteid, moneda, opgravada, opinafecta, opexonerada, igv_total, importetotalneto, tipoimpuestoespecial, estadosunat)
                    VALUES
                        (@pedidoId, @tipoComprobante, @serie, @correlativo, NOW(), '01', @clienteId, @moneda, @opGravada, 0.00, 0.00, @igvTotal, @totalNeto, 'ninguno', 'aceptado')
                    RETURNING comprobanteid;";

                using var cmdInsert = new NpgsqlCommand(insertComprobanteSql, cn, tx);
                cmdInsert.Parameters.AddWithValue("@pedidoId", pedidoId);
                cmdInsert.Parameters.AddWithValue("@tipoComprobante", tipoComprobante);
                cmdInsert.Parameters.AddWithValue("@serie", serie);
                cmdInsert.Parameters.AddWithValue("@correlativo", correlativo);
                cmdInsert.Parameters.AddWithValue("@clienteId", clienteId);
                cmdInsert.Parameters.AddWithValue("@moneda", moneda);
                cmdInsert.Parameters.AddWithValue("@opGravada", opGravada);
                cmdInsert.Parameters.AddWithValue("@igvTotal", igvTotal);
                cmdInsert.Parameters.AddWithValue("@totalNeto", totalNeto);

                int compId = Convert.ToInt32(await cmdInsert.ExecuteScalarAsync());

                tx.Commit();
                return compId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<List<PedidoVenta>> GetPendingBillingOrdersAsync()
        {
            var lista = new List<PedidoVenta>();
            const string sql = @"
                SELECT p.pedidoid, p.numeropedido, p.clienteid, p.proyectoid, p.fechaemision,
                       p.moneda, p.tipocambio, p.metodopago, p.cupondescuento,
                       p.montobruto, p.montodescuento, p.totalneto, p.estado,
                       c.razonsocial, c.numerodocumento
                FROM   operaciones.pedidosventa p
                INNER JOIN comercial.clientes c ON p.clienteid = c.clienteid
                WHERE  p.estado = 'despachado'
                       AND NOT EXISTS (
                           SELECT 1 FROM operaciones.comprobantesfacturacion cf WHERE cf.pedidoid = p.pedidoid
                       )
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
                    ClienteRuc = rd.GetString(14)
                });
            }
            return lista;
        }
    }
}
