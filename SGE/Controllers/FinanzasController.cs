using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGE.Models;
using SGE.Services;
using System.Data;
using System.Globalization;

namespace SGE.Controllers
{
    public class FinanzasController : Controller
    {
        private readonly ISgeDbConnectionFactory _connectionFactory;
        private readonly ILogger<FinanzasController> _logger;

        public FinanzasController(ISgeDbConnectionFactory connectionFactory, ILogger<FinanzasController> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IActionResult Impuestos()
        {
            try
            {
                var data = CargarFinanzas();
                var model = new ImpuestosViewModel
                {
                    Impuestos = data.Impuestos,
                    DebitoFiscal = data.AsientosDetalle.Where(x => x.CuentaCodigo == "4011").Sum(x => x.Haber),
                    CreditoFiscal = data.AsientosDetalle.Where(x => x.CuentaCodigo == "4011").Sum(x => x.Debe),
                    Retenciones = data.Impuestos.Where(x => x.NombreImpuesto.Contains("Retencion", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Porcentaje) * 100,
                    Percepciones = data.Impuestos.Where(x => x.NombreImpuesto.Contains("Percepcion", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Porcentaje) * 70
                };

                model.Kpis = new List<FinanzasKpi>
                {
                    new() { Titulo = "Impuestos", Valor = model.Impuestos.Count.ToString(), Detalle = "Tabla finanzas.impuestos", Icono = "bi-file-earmark-text", Tono = "teal" },
                    new() { Titulo = "IGV neto", Valor = Money(model.IgvNeto), Detalle = "Debito - credito", Icono = "bi-percent", Tono = "indigo" },
                    new() { Titulo = "Creditos", Valor = Money(model.CreditosDeducibles), Detalle = "Retenciones y percepciones", Icono = "bi-shield-check", Tono = "green" },
                    new() { Titulo = "Pago SUNAT", Valor = Money(model.MontoEstimadoSunat), Detalle = "Estimado mensual", Icono = "bi-cash-coin", Tono = "amber" }
                };

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return ErrorPartial(nameof(Impuestos), new ImpuestosViewModel(), ex);
            }
        }

        public IActionResult Contabilidad()
        {
            try
            {
                var data = CargarFinanzas();
                var model = new ContabilidadFinanzasViewModel
                {
                    PlanCuentas = data.PlanCuentas,
                    Asientos = data.AsientosCabecera,
                    Detalles = data.AsientosDetalle,
                    LibroDiario = BuildLibroDiario(data)
                };

                model.Kpis = new List<FinanzasKpi>
                {
                    new() { Titulo = "Plan cuentas", Valor = model.PlanCuentas.Count.ToString(), Detalle = "finanzas.plancuentas", Icono = "bi-diagram-3", Tono = "teal" },
                    new() { Titulo = "Asientos", Valor = model.Asientos.Count.ToString(), Detalle = "finanzas.asientoscabecera", Icono = "bi-journal-text", Tono = "indigo" },
                    new() { Titulo = "Total debe", Valor = Money(model.TotalDebe), Detalle = "finanzas.asientosdetalle", Icono = "bi-arrow-down-left", Tono = "amber" },
                    new() { Titulo = "Diferencia", Valor = Money(model.Diferencia), Detalle = model.Diferencia == 0 ? "Partida doble OK" : "Revisar", Icono = "bi-balance-scale", Tono = model.Diferencia == 0 ? "green" : "red" }
                };

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return ErrorPartial(nameof(Contabilidad), new ContabilidadFinanzasViewModel(), ex);
            }
        }

        public IActionResult Caja_y_Bancos()
        {
            try
            {
                var data = CargarFinanzas();
                var model = new CajaBancosViewModel
                {
                    Cuentas = data.CuentasBancarias,
                    Movimientos = data.MovimientosTesoreria
                };

                model.Kpis = new List<FinanzasKpi>
                {
                    new() { Titulo = "Cuentas", Valor = model.Cuentas.Count.ToString(), Detalle = "finanzas.cuentasbancarias", Icono = "bi-bank", Tono = "teal" },
                    new() { Titulo = "Saldo total", Valor = Money(model.SaldoTotal), Detalle = "Disponible", Icono = "bi-wallet2", Tono = "green" },
                    new() { Titulo = "Ingresos", Valor = Money(model.TotalIngresos), Detalle = "tipoflujo ING", Icono = "bi-arrow-down-circle", Tono = "indigo" },
                    new() { Titulo = "Egresos", Valor = Money(model.TotalEgresos), Detalle = "tipoflujo EGR", Icono = "bi-arrow-up-circle", Tono = "amber" }
                };

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return ErrorPartial(nameof(Caja_y_Bancos), new CajaBancosViewModel(), ex);
            }
        }

        public IActionResult ActivosFijos()
        {
            try
            {
                var data = CargarFinanzas();
                var model = new ActivosFijosViewModel
                {
                    Activos = data.ActivosFijos
                };

                model.Kpis = new List<FinanzasKpi>
                {
                    new() { Titulo = "Activos", Valor = model.Activos.Count.ToString(), Detalle = "finanzas.activosfijos", Icono = "bi-box-seam", Tono = "indigo" },
                    new() { Titulo = "Valor total", Valor = Money(model.ValorTotal), Detalle = "Adquisicion", Icono = "bi-cash-stack", Tono = "green" },
                    new() { Titulo = "Depreciacion", Valor = Money(model.DepreciacionTotal), Detalle = "Acumulada", Icono = "bi-graph-down", Tono = "amber" },
                    new() { Titulo = "Valor neto", Valor = Money(model.ValorNetoTotal), Detalle = "Libros", Icono = "bi-journal-check", Tono = "teal" }
                };

                return PartialView(model);
            }
            catch (Exception ex)
            {
                return ErrorPartial(nameof(ActivosFijos), new ActivosFijosViewModel(), ex);
            }
        }

        [HttpPost]
        public IActionResult CrearImpuesto(ImpuestoFinanciero impuesto)
        {
            if (string.IsNullOrWhiteSpace(impuesto.CodigoImpuestoSunat) || impuesto.Porcentaje < 0)
            {
                TempData["Error"] = "Completa un codigo SUNAT valido y un porcentaje positivo.";
                return RedirectToAction(nameof(Impuestos));
            }

            try
            {
                ExecuteNonQuery(
                    "insert into finanzas.impuestos (codigoimpuestosunat, nombreimpuesto, porcentaje, estado) values (@codigo, @nombre, @porcentaje, 1)",
                    p =>
                    {
                        p.AddWithValue("@codigo", impuesto.CodigoImpuestoSunat.PadLeft(4, '0')[..4]);
                        p.AddWithValue("@nombre", string.IsNullOrWhiteSpace(impuesto.NombreImpuesto) ? "Nuevo impuesto" : impuesto.NombreImpuesto);
                        p.AddWithValue("@porcentaje", impuesto.Porcentaje);
                    });

                TempData["Ok"] = "Impuesto registrado en SQL Server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar el impuesto {CodigoImpuestoSunat}.", impuesto.CodigoImpuestoSunat);
                TempData["Error"] = "No se pudo registrar el impuesto. Verifica si el codigo SUNAT ya existe.";
            }

            return RedirectToAction(nameof(Impuestos));
        }

        [HttpPost]
        public IActionResult CrearAsiento(string glosa, string tipoLibroSunat, string documentoReferencia, string cuentaDebe, decimal montoDebe, string cuentaHaber, decimal montoHaber)
        {
            if (montoDebe <= 0 || montoDebe != montoHaber)
            {
                TempData["Error"] = "El asiento debe estar cuadrado y tener importes mayores a cero.";
                return RedirectToAction(nameof(Contabilidad));
            }

            try
            {
                using var cn = CrearConexion();
                cn.Open();
                using var tx = cn.BeginTransaction();
                var asientoId = 0L;
                using (var cmd = new SqlCommand(@"
insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
output inserted.asientoid
values (@numero, cast(getdate() as date), @libro, @glosa, @documento)", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@numero", $"AS-2026-{DateTime.Now:HHmmss}");
                    cmd.Parameters.AddWithValue("@libro", string.IsNullOrWhiteSpace(tipoLibroSunat) ? "01" : tipoLibroSunat);
                    cmd.Parameters.AddWithValue("@glosa", string.IsNullOrWhiteSpace(glosa) ? "Asiento contable manual" : glosa);
                    cmd.Parameters.AddWithValue("@documento", string.IsNullOrWhiteSpace(documentoReferencia) ? DBNull.Value : documentoReferencia);
                    asientoId = Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
                }

                InsertDetalle(cn, tx, asientoId, cuentaDebe, montoDebe, 0);
                InsertDetalle(cn, tx, asientoId, cuentaHaber, 0, montoHaber);
                tx.Commit();
                TempData["Ok"] = "Asiento registrado en SQL Server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar el asiento contable.");
                TempData["Error"] = "No se pudo registrar el asiento. Revisa cuentas y montos.";
            }

            return RedirectToAction(nameof(Contabilidad));
        }

        [HttpPost]
        public IActionResult RegistrarMovimiento(int cuentaBancariaId, string tipoFlujo, string medioPagoSunat, decimal monto, string glosaMovimiento)
        {
            if (monto <= 0)
            {
                TempData["Error"] = "El monto debe ser mayor a cero.";
                return RedirectToAction(nameof(Caja_y_Bancos));
            }

            try
            {
                var flujo = tipoFlujo == "egr" ? "egr" : "ing";
                ExecuteNonQuery(@"
insert into finanzas.movimientostesoreria (cuentabancariaid, tipoflujo, mediopagosunat, monto, glosamovimiento, fechamovimiento)
values (@cuenta, @flujo, @medio, @monto, @glosa, getdate());

update finanzas.cuentasbancarias
set saldoactual = saldoactual + case when @flujo = 'ing' then @monto else -@monto end
where cuentabancariaid = @cuenta;",
                p =>
                {
                    p.AddWithValue("@cuenta", cuentaBancariaId);
                    p.AddWithValue("@flujo", flujo);
                    p.AddWithValue("@medio", string.IsNullOrWhiteSpace(medioPagoSunat) ? "003" : medioPagoSunat);
                    p.AddWithValue("@monto", monto);
                    p.AddWithValue("@glosa", string.IsNullOrWhiteSpace(glosaMovimiento) ? "Movimiento de tesoreria" : glosaMovimiento);
                });

                TempData["Ok"] = "Movimiento registrado en SQL Server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar movimiento de tesoreria para la cuenta {CuentaBancariaId}.", cuentaBancariaId);
                TempData["Error"] = "No se pudo registrar el movimiento. Verifica la cuenta bancaria.";
            }

            return RedirectToAction(nameof(Caja_y_Bancos));
        }

        [HttpPost]
        public IActionResult CrearActivo(ActivoFijoFinanciero activo)
        {
            if (activo.ValorInicial <= 0 || activo.TasaDepreciacionAnual < 0 || activo.DepreciacionAcumulada < 0)
            {
                TempData["Error"] = "El valor inicial debe ser mayor a cero y la depreciacion no puede ser negativa.";
                return RedirectToAction(nameof(ActivosFijos));
            }

            try
            {
                ExecuteNonQuery(@"
insert into finanzas.activosfijos (codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada, estado)
values (@codigo, @descripcion, null, @fecha, @valor, @tasa, @depreciacion, @estado)",
                p =>
                {
                    p.AddWithValue("@codigo", string.IsNullOrWhiteSpace(activo.CodigoActivo) ? $"AF-2026-{DateTime.Now:HHmmss}" : activo.CodigoActivo);
                    p.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(activo.Descripcion) ? "Nuevo activo fijo" : activo.Descripcion);
                    p.AddWithValue("@fecha", activo.FechaAdquisicion == default ? DateTime.Today : activo.FechaAdquisicion);
                    p.AddWithValue("@valor", activo.ValorInicial);
                    p.AddWithValue("@tasa", activo.TasaDepreciacionAnual);
                    p.AddWithValue("@depreciacion", activo.DepreciacionAcumulada);
                    p.AddWithValue("@estado", string.IsNullOrWhiteSpace(activo.Estado) ? "activo" : activo.Estado);
                });

                TempData["Ok"] = "Activo registrado en SQL Server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar el activo fijo {CodigoActivo}.", activo.CodigoActivo);
                TempData["Error"] = "No se pudo registrar el activo. Verifica si el codigo ya existe.";
            }

            return RedirectToAction(nameof(ActivosFijos));
        }

        private FinanzasDataStore CargarFinanzas()
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

            data.ActivosFijos = Query(cn, "select activoid, codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada, estado from finanzas.activosfijos order by codigoactivo", r => new ActivoFijoFinanciero
            {
                ActivoId = r.GetInt32(0),
                CodigoActivo = r.GetString(1),
                Descripcion = r.GetString(2),
                ProductoId = r.IsDBNull(3) ? null : r.GetInt32(3),
                FechaAdquisicion = r.GetDateTime(4),
                ValorInicial = r.GetDecimal(5),
                TasaDepreciacionAnual = r.GetDecimal(6),
                DepreciacionAcumulada = r.GetDecimal(7),
                Estado = r.GetString(8)
            });

            return data;
        }

        private static List<LibroDiarioFinanciero> BuildLibroDiario(FinanzasDataStore data)
        {
            return data.AsientosDetalle
                .Join(data.AsientosCabecera, d => d.AsientoId, a => a.AsientoId, (d, a) => new { d, a })
                .Join(data.PlanCuentas, x => x.d.CuentaCodigo, p => p.CuentaCodigo, (x, p) => new LibroDiarioFinanciero
                {
                    AsientoId = x.a.AsientoId,
                    NumeroAsiento = x.a.NumeroAsiento,
                    FechaAsiento = x.a.FechaAsiento,
                    TipoLibroSunat = x.a.TipoLibroSunat,
                    Glosa = x.a.Glosa,
                    DocumentoReferencia = x.a.DocumentoReferencia,
                    CuentaCodigo = x.d.CuentaCodigo,
                    NombreCuenta = p.Descripcion,
                    Debe = x.d.Debe,
                    Haber = x.d.Haber
                })
                .OrderByDescending(x => x.FechaAsiento)
                .ThenBy(x => x.NumeroAsiento)
                .ToList();
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

        private void ExecuteNonQuery(string sql, Action<SqlParameterCollection> parameters)
        {
            using var cn = CrearConexion();
            using var cmd = new SqlCommand(sql, cn);
            parameters(cmd.Parameters);
            cn.Open();
            cmd.ExecuteNonQuery();
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

        private IActionResult ErrorPartial<TModel>(string viewName, TModel model, Exception exception)
        {
            _logger.LogError(exception, "No se pudo cargar el modulo de finanzas {ViewName}.", viewName);
            TempData["Error"] = "No se pudo cargar la informacion de la base de datos. Revisa la conexion o intenta nuevamente.";
            return PartialView(viewName, model);
        }

        public static string Money(decimal value) => "S/ " + value.ToString("#,##0.00", CultureInfo.InvariantCulture);
    }
}
