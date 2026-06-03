using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGE.Models;
using System.Globalization;
using System.Text;

namespace SGE.Services;

public interface IFinanzasDataService
{
    FinanzasDataStore CargarFinanzas();
    void ExecuteNonQuery(string sql, Action<SqlParameterCollection> parameters);
    long CrearAsiento(string tipoLibroSunat, string glosa, string? documentoReferencia, string cuentaDebe, decimal montoDebe, string cuentaHaber, decimal montoHaber);
    void ActualizarMovimientoTesoreria(long movimientoTesoreriaId, int cuentaBancariaId, string tipoFlujo, string medioPagoSunat, decimal monto, string glosaMovimiento);
}

public class FinanzasDataService : IFinanzasDataService
{
    private readonly ISgeDbConnectionFactory _connectionFactory;

    public FinanzasDataService(ISgeDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public FinanzasDataStore CargarFinanzas()
    {
        var data = new FinanzasDataStore();
        using var cn = CrearConexion();
        cn.Open();

        data.Impuestos = Query(cn, "select impuestoid, codigoimpuestosunat, nombreimpuesto, porcentaje, estado from finanzas.impuestos order by impuestoid", r => new ImpuestoFinanciero
        {
            ImpuestoId = r.GetInt32(0),
            CodigoImpuestoSunat = r.GetString(1),
            NombreImpuesto = r.GetString(2),
            Porcentaje = r.GetDecimal(3),
            Estado = r.GetBoolean(4)
        });

        data.PlanCuentas = Query(cn, "select cuentacodigo, descripcion, tipocuenta, nivelint, aceptaasiento from finanzas.plancuentas order by cuentacodigo", r => new PlanCuentaFinanciero
        {
            CuentaCodigo = r.GetString(0),
            Descripcion = r.GetString(1),
            TipoCuenta = r.GetString(2),
            NivelInt = r.GetInt32(3),
            AceptaAsiento = r.GetBoolean(4)
        });

        data.AsientosCabecera = Query(cn, "select asientoid, numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia, fecharegistro from finanzas.asientoscabecera order by fechaasiento desc, asientoid desc", r => new AsientoCabeceraFinanciero
        {
            AsientoId = r.GetInt64(0),
            NumeroAsiento = r.GetString(1),
            FechaAsiento = r.GetDateTime(2),
            TipoLibroSunat = r.GetString(3),
            Glosa = r.GetString(4),
            DocumentoReferencia = r.IsDBNull(5) ? null : r.GetString(5),
            FechaRegistro = r.GetDateTime(6)
        });

        data.AsientosDetalle = Query(cn, "select asientodetalleid, asientoid, cuentacodigo, debe, haber from finanzas.asientosdetalle order by asientoid, asientodetalleid", r => new AsientoDetalleFinanciero
        {
            AsientoDetalleId = r.GetInt64(0),
            AsientoId = r.GetInt64(1),
            CuentaCodigo = r.GetString(2),
            Debe = r.GetDecimal(3),
            Haber = r.GetDecimal(4)
        });

        data.CuentasBancarias = Query(cn, "select cuentabancariaid, banconombre, numerocuenta, cuentacciexterno, tipocuenta, moneda, saldoactual, estado from finanzas.cuentasbancarias order by banconombre", r => new CuentaBancariaFinanciera
        {
            CuentaBancariaId = r.GetInt32(0),
            BancoNombre = r.GetString(1),
            NumeroCuenta = r.GetString(2),
            CuentaCciExterno = r.IsDBNull(3) ? null : r.GetString(3),
            TipoCuenta = r.GetString(4),
            Moneda = r.GetString(5),
            SaldoActual = r.GetDecimal(6),
            Estado = r.GetBoolean(7)
        });

        data.MovimientosTesoreria = Query(cn, "select movimientotesoreriaid, cuentabancariaid, tipoflujo, mediopagosunat, monto, comprobanteid, ordenid, glosamovimiento, fechamovimiento from finanzas.movimientostesoreria order by fechamovimiento desc", r => new MovimientoTesoreriaFinanciero
        {
            MovimientoTesoreriaId = r.GetInt64(0),
            CuentaBancariaId = r.GetInt32(1),
            TipoFlujo = r.GetString(2),
            MedioPagoSunat = r.GetString(3),
            Monto = r.GetDecimal(4),
            ComprobanteId = r.IsDBNull(5) ? null : r.GetInt32(5),
            OrdenId = r.IsDBNull(6) ? null : r.GetInt32(6),
            GlosaMovimiento = r.IsDBNull(7) ? null : r.GetString(7),
            FechaMovimiento = r.GetDateTime(8)
        });

        data.ActivosFijos = Query(cn, @"
select af.activoid,
       af.codigoactivo,
       af.descripcion,
       af.productoid,
       p.codigosku,
       p.descripcion as productodescripcion,
       af.fechadquisicion,
       af.valorinicial,
       af.tasadepreciacionanual,
       af.depreciacionacumulada,
       af.estado
from finanzas.activosfijos af
left join comercial.productos p on af.productoid = p.productoid
order by af.codigoactivo", r => new ActivoFijoFinanciero
        {
            ActivoId = r.GetInt32(0),
            CodigoActivo = r.GetString(1),
            Descripcion = r.GetString(2),
            ProductoId = r.IsDBNull(3) ? null : r.GetInt32(3),
            ProductoSku = r.IsDBNull(4) ? null : r.GetString(4),
            ProductoDescripcion = r.IsDBNull(5) ? null : r.GetString(5),
            FechaAdquisicion = r.GetDateTime(6),
            ValorInicial = r.GetDecimal(7),
            TasaDepreciacionAnual = r.GetDecimal(8),
            DepreciacionAcumulada = r.GetDecimal(9),
            Estado = r.GetString(10)
        });

        return data;
    }

    public long CrearAsiento(string tipoLibroSunat, string glosa, string? documentoReferencia, string cuentaDebe, decimal montoDebe, string cuentaHaber, decimal montoHaber)
    {
        using var cn = CrearConexion();
        cn.Open();
        using var tx = cn.BeginTransaction();

        long asientoId;
        using (var cmd = new SqlCommand(@"
insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
output inserted.asientoid
values (@numero, cast(getdate() as date), @libro, @glosa, @documento)", cn, tx))
        {
            cmd.Parameters.AddWithValue("@numero", $"AS-{DateTime.Now:yyyyMMddHHmmssfff}"[..20]);
            cmd.Parameters.AddWithValue("@libro", string.IsNullOrWhiteSpace(tipoLibroSunat) ? "01" : tipoLibroSunat.PadLeft(2, '0')[..2]);
            cmd.Parameters.AddWithValue("@glosa", string.IsNullOrWhiteSpace(glosa) ? "Asiento contable manual" : glosa.Trim());
            cmd.Parameters.AddWithValue("@documento", string.IsNullOrWhiteSpace(documentoReferencia) ? DBNull.Value : documentoReferencia.Trim());
            asientoId = Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }

        InsertDetalle(cn, tx, asientoId, cuentaDebe, montoDebe, 0);
        InsertDetalle(cn, tx, asientoId, cuentaHaber, 0, montoHaber);
        tx.Commit();

        return asientoId;
    }

    public void ActualizarMovimientoTesoreria(long movimientoTesoreriaId, int cuentaBancariaId, string tipoFlujo, string medioPagoSunat, decimal monto, string glosaMovimiento)
    {
        using var cn = CrearConexion();
        cn.Open();
        using var tx = cn.BeginTransaction();

        var oldCuenta = 0;
        var oldFlujo = "ing";
        var oldMonto = 0m;
        using (var read = new SqlCommand("select cuentabancariaid, tipoflujo, monto from finanzas.movimientostesoreria where movimientotesoreriaid = @id", cn, tx))
        {
            read.Parameters.AddWithValue("@id", movimientoTesoreriaId);
            using var reader = read.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("El movimiento no existe.");
            }

            oldCuenta = reader.GetInt32(0);
            oldFlujo = reader.GetString(1);
            oldMonto = reader.GetDecimal(2);
        }

        var flujo = string.Equals(tipoFlujo, "egr", StringComparison.OrdinalIgnoreCase) ? "egr" : "ing";
        AjustarSaldoCuenta(cn, tx, oldCuenta, -ImporteConSigno(oldFlujo, oldMonto));
        AjustarSaldoCuenta(cn, tx, cuentaBancariaId, ImporteConSigno(flujo, monto));

        using (var update = new SqlCommand(@"
update finanzas.movimientostesoreria
set cuentabancariaid = @cuenta,
    tipoflujo = @flujo,
    mediopagosunat = @medio,
    monto = @monto,
    glosamovimiento = @glosa
where movimientotesoreriaid = @id", cn, tx))
        {
            update.Parameters.AddWithValue("@id", movimientoTesoreriaId);
            update.Parameters.AddWithValue("@cuenta", cuentaBancariaId);
            update.Parameters.AddWithValue("@flujo", flujo);
            update.Parameters.AddWithValue("@medio", string.IsNullOrWhiteSpace(medioPagoSunat) ? "003" : medioPagoSunat.PadLeft(3, '0')[..3]);
            update.Parameters.AddWithValue("@monto", monto);
            update.Parameters.AddWithValue("@glosa", string.IsNullOrWhiteSpace(glosaMovimiento) ? "Movimiento de tesoreria" : glosaMovimiento.Trim());
            update.ExecuteNonQuery();
        }

        tx.Commit();
    }

    public void ExecuteNonQuery(string sql, Action<SqlParameterCollection> parameters)
    {
        using var cn = CrearConexion();
        using var cmd = new SqlCommand(sql, cn);
        parameters(cmd.Parameters);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private SqlConnection CrearConexion()
    {
        return _connectionFactory.CreateConnection();
    }

    private static List<T> Query<T>(SqlConnection cn, string sql, Func<SqlDataReader, T> map)
    {
        using var cmd = new SqlCommand(sql, cn);
        using var reader = cmd.ExecuteReader();
        var items = new List<T>();

        while (reader.Read())
        {
            items.Add(map(reader));
        }

        return items;
    }

    private static void InsertDetalle(SqlConnection cn, SqlTransaction tx, long asientoId, string cuentaCodigo, decimal debe, decimal haber)
    {
        using var cmd = new SqlCommand("insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber) values (@asiento, @cuenta, @debe, @haber)", cn, tx);
        cmd.Parameters.AddWithValue("@asiento", asientoId);
        cmd.Parameters.AddWithValue("@cuenta", cuentaCodigo);
        cmd.Parameters.AddWithValue("@debe", debe);
        cmd.Parameters.AddWithValue("@haber", haber);
        cmd.ExecuteNonQuery();
    }

    private static void AjustarSaldoCuenta(SqlConnection cn, SqlTransaction tx, int cuentaBancariaId, decimal ajuste)
    {
        using var cmd = new SqlCommand("update finanzas.cuentasbancarias set saldoactual = saldoactual + @ajuste where cuentabancariaid = @cuenta", cn, tx);
        cmd.Parameters.AddWithValue("@cuenta", cuentaBancariaId);
        cmd.Parameters.AddWithValue("@ajuste", ajuste);
        cmd.ExecuteNonQuery();
    }

    private static decimal ImporteConSigno(string flujo, decimal monto)
    {
        return string.Equals(flujo, "egr", StringComparison.OrdinalIgnoreCase) ? -monto : monto;
    }
}

public static class FinanzasFormat
{
    public static string Money(decimal value) => "S/ " + value.ToString("#,##0.00", CultureInfo.InvariantCulture);

    public static FileContentResult Csv(string fileName, IEnumerable<string[]> rows, ControllerBase controller)
    {
        var csv = new StringBuilder();
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",", row.Select(EscapeCsv)));
        }

        return controller.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", fileName);
    }

    private static string EscapeCsv(string? value)
    {
        value ??= "";
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
