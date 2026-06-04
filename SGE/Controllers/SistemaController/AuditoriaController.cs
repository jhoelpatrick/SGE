using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGE.Models.SistemaModel;
using System.Data;

namespace SGE.Controllers.SistemaController
{
    [Route("Sistema/Auditoria")]
    public class AuditoriaController : Controller
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<AuditoriaController> _logger;

        public AuditoriaController(IDbConnection connection, ILogger<AuditoriaController> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? tablaAfectada,
            string? accion,
            string? usuarioFiltro,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1)
        {
            var model = new AuditoriaViewModel
            {
                TablaAfectada = tablaAfectada,
                Accion = accion,
                UsuarioFiltro = usuarioFiltro,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PaginaActual = pagina < 1 ? 1 : pagina
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarFiltrosAuditoria(model);
                await CargarMetricasAuditoria(model);
                await CargarGraficosAuditoria(model);
                await CargarRecientesAuditoria(model);
                await CargarAlertasAuditoria(model);
                await CargarTotalAuditoria(model);
                await CargarRegistrosAuditoria(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando auditoría.");
                model.Error = "No se pudo cargar la auditoría.";
            }

            return View("~/Views/Sistema/Auditoria.cshtml", model);
        }

        [HttpGet("Excel")]
        public async Task<IActionResult> Excel(
            string? tablaAfectada,
            string? accion,
            string? usuarioFiltro,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var model = new AuditoriaViewModel
            {
                TablaAfectada = tablaAfectada,
                Accion = accion,
                UsuarioFiltro = usuarioFiltro,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                RegistrosPorPagina = 100000,
                PaginaActual = 1
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarMetricasAuditoria(model);
                await CargarTotalAuditoria(model);
                await CargarRegistrosAuditoria(model);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Auditoria");

                worksheet.Cell(1, 1).Value = "Fecha";
                worksheet.Cell(1, 2).Value = "Usuario";
                worksheet.Cell(1, 3).Value = "Tabla afectada";
                worksheet.Cell(1, 4).Value = "Acción";
                worksheet.Cell(1, 5).Value = "ID afectado";
                worksheet.Cell(1, 6).Value = "Valor anterior";
                worksheet.Cell(1, 7).Value = "Valor nuevo";

                var header = worksheet.Range(1, 1, 1, 7);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#10b981");
                header.Style.Font.FontColor = XLColor.White;

                var row = 2;

                foreach (var item in model.Registros)
                {
                    worksheet.Cell(row, 1).Value = item.FechaRegistro;
                    worksheet.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";

                    worksheet.Cell(row, 2).Value = item.Usuario;
                    worksheet.Cell(row, 3).Value = item.TablaAfectada;
                    worksheet.Cell(row, 4).Value = item.Accion;
                    worksheet.Cell(row, 5).Value = item.IdRegistroAfectado;
                    worksheet.Cell(row, 6).Value = item.ValorAnterior;
                    worksheet.Cell(row, 7).Value = item.ValorNuevo;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"auditoria_sistema_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando auditoría a Excel.");
                TempData["Error"] = "No se pudo exportar auditoría a Excel.";

                return RedirectToAction(nameof(Index), new
                {
                    tablaAfectada,
                    accion,
                    usuarioFiltro,
                    fechaInicio,
                    fechaFin
                });
            }
        }

        [HttpGet("Pdf")]
        public async Task<IActionResult> Pdf(
            string? tablaAfectada,
            string? accion,
            string? usuarioFiltro,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var model = new AuditoriaViewModel
            {
                TablaAfectada = tablaAfectada,
                Accion = accion,
                UsuarioFiltro = usuarioFiltro,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                RegistrosPorPagina = 100000,
                PaginaActual = 1
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarMetricasAuditoria(model);
                await CargarTotalAuditoria(model);
                await CargarRegistrosAuditoria(model);

                QuestPDF.Settings.License = LicenseType.Community;

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(25);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Bitácora de Auditoría")
                                .FontSize(20)
                                .Bold()
                                .FontColor("#111827");

                            col.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor("#6b7280");
                        });

                        page.Content().PaddingVertical(15).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Eventos registrados").FontColor("#6b7280");
                                    c.Item().Text(model.TotalEventos.ToString("N0")).Bold().FontSize(14);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Usuarios únicos").FontColor("#6b7280");
                                    c.Item().Text(model.UsuariosUnicos.ToString()).Bold().FontSize(14);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Cambios críticos").FontColor("#6b7280");
                                    c.Item().Text(model.CambiosCriticos.ToString()).Bold().FontSize(14);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Nivel de riesgo").FontColor("#6b7280");
                                    c.Item().Text(model.NivelRiesgo).Bold().FontSize(14);
                                });
                            });

                            col.Item().PaddingTop(18).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(85);
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(1.4f);
                                    columns.ConstantColumn(55);
                                    columns.ConstantColumn(60);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#10b981").Padding(5).Text("Fecha").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("Usuario").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("Tabla").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("Acción").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("ID").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("Anterior").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(5).Text("Nuevo").FontColor(Colors.White).Bold();
                                });

                                foreach (var item in model.Registros.Take(200))
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.FechaRegistro.ToString("dd/MM/yyyy HH:mm"));
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.Usuario);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.TablaAfectada);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.Accion);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.IdRegistroAfectado);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.ValorAnterior ?? "-");
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.ValorNuevo ?? "-");
                                }
                            });

                            if (model.Registros.Count > 200)
                            {
                                col.Item().PaddingTop(10).Text("Nota: el PDF muestra los primeros 200 registros para mantener el archivo legible. El Excel contiene más registros.")
                                    .FontColor("#6b7280")
                                    .Italic();
                            }
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                }).GeneratePdf();

                var fileName = $"auditoria_sistema_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando auditoría a PDF.");
                TempData["Error"] = "No se pudo exportar auditoría a PDF.";

                return RedirectToAction(nameof(Index), new
                {
                    tablaAfectada,
                    accion,
                    usuarioFiltro,
                    fechaInicio,
                    fechaFin
                });
            }
        }

        private string ObtenerWhereAuditoria()
        {
            return @"
                where (@tablaAfectada is null or tablaafectada = @tablaAfectada)
                  and (@accion is null or accion = @accion)
                  and (@usuarioFiltro is null or usuario = @usuarioFiltro)
                  and (@fechaInicio is null or fecharegistro >= @fechaInicio)
                  and (@fechaFinMasUno is null or fecharegistro < @fechaFinMasUno)
            ";
        }

        private void AgregarParametrosAuditoria(NpgsqlCommand command, AuditoriaViewModel model)
        {
            command.Parameters.AddWithValue("@tablaAfectada", string.IsNullOrWhiteSpace(model.TablaAfectada) ? DBNull.Value : model.TablaAfectada);
            command.Parameters.AddWithValue("@accion", string.IsNullOrWhiteSpace(model.Accion) ? DBNull.Value : model.Accion);
            command.Parameters.AddWithValue("@usuarioFiltro", string.IsNullOrWhiteSpace(model.UsuarioFiltro) ? DBNull.Value : model.UsuarioFiltro);
            command.Parameters.AddWithValue("@fechaInicio", model.FechaInicio.HasValue ? model.FechaInicio.Value : DBNull.Value);
            command.Parameters.AddWithValue("@fechaFinMasUno", model.FechaFin.HasValue ? model.FechaFin.Value.AddDays(1) : DBNull.Value);
        }

        private async Task CargarFiltrosAuditoria(AuditoriaViewModel model)
        {
            using (var command = ((NpgsqlConnection)_connection).CreateCommand())
            {
                command.CommandText = @"
                    select distinct tablaafectada
                    from sistema.logs_auditoria_datos
                    order by tablaafectada;
                ";

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    model.Tablas.Add(reader["tablaafectada"].ToString() ?? "");
                }
            }

            using (var command = ((NpgsqlConnection)_connection).CreateCommand())
            {
                command.CommandText = @"
                    select distinct usuario
                    from sistema.logs_auditoria_datos
                    order by usuario;
                ";

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    model.Usuarios.Add(reader["usuario"].ToString() ?? "");
                }
            }
        }

        private async Task CargarMetricasAuditoria(AuditoriaViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = $@"
                select 
                    count(*) as totaleventos,
                    count(distinct usuario) as usuariosunicos,
                    sum(case when accion in ('update', 'delete') then 1 else 0 end) as cambioscriticos,
                    sum(case when cast(fecharegistro as date) = CURRENT_DATE then 1 else 0 end) as eventoshoy
                from sistema.logs_auditoria_datos
                {ObtenerWhereAuditoria()};
            ";

            AgregarParametrosAuditoria(command, model);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                model.TotalEventos = reader["totaleventos"] == DBNull.Value ? 0 : Convert.ToInt32(reader["totaleventos"]);
                model.UsuariosUnicos = reader["usuariosunicos"] == DBNull.Value ? 0 : Convert.ToInt32(reader["usuariosunicos"]);
                model.CambiosCriticos = reader["cambioscriticos"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cambioscriticos"]);
                model.EventosHoy = reader["eventoshoy"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eventoshoy"]);
            }

            if (model.TotalEventos == 0)
            {
                model.NivelRiesgo = "Bajo";
            }
            else
            {
                var ratio = (double)model.CambiosCriticos / model.TotalEventos;

                if (ratio >= 0.30)
                    model.NivelRiesgo = "Alto";
                else if (ratio >= 0.15)
                    model.NivelRiesgo = "Medio";
                else
                    model.NivelRiesgo = "Bajo";
            }
        }

        private async Task CargarGraficosAuditoria(AuditoriaViewModel model)
        {
            using (var command = ((NpgsqlConnection)_connection).CreateCommand())
            {
                command.CommandText = $@"
                    select accion, count(*) as total
                    from sistema.logs_auditoria_datos
                    {ObtenerWhereAuditoria()}
                    group by accion
                    order by total desc;
                ";

                AgregarParametrosAuditoria(command, model);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    model.ChartAccionLabels.Add(reader["accion"].ToString() ?? "");
                    model.ChartAccionValues.Add(Convert.ToInt32(reader["total"]));
                }
            }

            using (var command = ((NpgsqlConnection)_connection).CreateCommand())
            {
                command.CommandText = $@"
                    select tablaafectada, count(*) as total
                    from sistema.logs_auditoria_datos
                    {ObtenerWhereAuditoria()}
                    group by tablaafectada
                    order by total desc
                    LIMIT 5;
                ";

                AgregarParametrosAuditoria(command, model);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    model.ChartTablaLabels.Add(reader["tablaafectada"].ToString() ?? "");
                    model.ChartTablaValues.Add(Convert.ToInt32(reader["total"]));
                }
            }
        }

        private async Task CargarRecientesAuditoria(AuditoriaViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = $@"
                select logid, usuario, tablaafectada, accion, fecharegistro,
                       idregistroafectado, valoranterior, valornuevo
                from sistema.logs_auditoria_datos
                {ObtenerWhereAuditoria()}
                order by fecharegistro desc, logid desc
                LIMIT 5;
            ";

            AgregarParametrosAuditoria(command, model);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.Recientes.Add(MapAuditoria(reader));
            }
        }

        private async Task CargarAlertasAuditoria(AuditoriaViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = @"
                select usuario, tablaafectada, accion, fecharegistro
                from sistema.logs_auditoria_datos
                where accion in ('delete', 'update')
                order by fecharegistro desc, logid desc
                LIMIT 3;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var accion = reader["accion"].ToString() ?? "";
                var usuario = reader["usuario"].ToString() ?? "";
                var tabla = reader["tablaafectada"].ToString() ?? "";
                var fecha = Convert.ToDateTime(reader["fecharegistro"]);

                model.Alertas.Add(new AuditoriaAlertaViewModel
                {
                    Titulo = accion == "delete" ? "Eliminación de registro" : "Cambio crítico detectado",
                    Descripcion = $"{usuario} realizó {accion} en {tabla} ({fecha:dd/MM/yyyy HH:mm})",
                    Nivel = accion == "delete" ? "Alerta" : "Revisar"
                });
            }
        }

        private async Task CargarTotalAuditoria(AuditoriaViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = $@"
                select count(*)
                from sistema.logs_auditoria_datos
                {ObtenerWhereAuditoria()};
            ";

            AgregarParametrosAuditoria(command, model);

            var total = await command.ExecuteScalarAsync();
            model.TotalRegistros = Convert.ToInt32(total);
        }

        private async Task CargarRegistrosAuditoria(AuditoriaViewModel model)
        {
            var offset = (model.PaginaActual - 1) * model.RegistrosPorPagina;

            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = $@"
                select logid, usuario, tablaafectada, accion, fecharegistro,
                       idregistroafectado, valoranterior, valornuevo
                from sistema.logs_auditoria_datos
                {ObtenerWhereAuditoria()}
                order by fecharegistro desc, logid desc
                LIMIT @pageSize OFFSET @offset;
            ";

            AgregarParametrosAuditoria(command, model);
            command.Parameters.AddWithValue("@offset", offset);
            command.Parameters.AddWithValue("@pageSize", model.RegistrosPorPagina);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.Registros.Add(MapAuditoria(reader));
            }
        }

        private AuditoriaItemViewModel MapAuditoria(NpgsqlDataReader reader)
        {
            return new AuditoriaItemViewModel
            {
                LogId = Convert.ToInt64(reader["logid"]),
                Usuario = reader["usuario"].ToString() ?? "",
                TablaAfectada = reader["tablaafectada"].ToString() ?? "",
                Accion = reader["accion"].ToString() ?? "",
                FechaRegistro = Convert.ToDateTime(reader["fecharegistro"]),
                IdRegistroAfectado = reader["idregistroafectado"].ToString() ?? "",
                ValorAnterior = reader["valoranterior"] == DBNull.Value ? null : reader["valoranterior"].ToString(),
                ValorNuevo = reader["valornuevo"] == DBNull.Value ? null : reader["valornuevo"].ToString()
            };
        }
    }
}
