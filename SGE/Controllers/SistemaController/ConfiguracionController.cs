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
    [Route("Sistema/Configuracion")]
    public class ConfiguracionController : Controller
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(IDbConnection connection, ILogger<ConfiguracionController> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = new ParametrosIndexViewModel();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarParametros(model);
                CalcularResumenConfiguracion(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando configuración.");
                model.Error = "No se pudo cargar la configuración.";
            }

            return View("~/Views/Sistema/Configuracion.cshtml", model);
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ParametrosIndexViewModel model)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using var transaction = ((NpgsqlConnection)_connection).BeginTransaction();

                try
                {
                    foreach (var item in model.Parametros)
                    {
                        using var command = ((NpgsqlConnection)_connection).CreateCommand();
                        command.Transaction = transaction;

                        command.CommandText = @"
                            update sistema.parametros
                            set valor = @valor,
                                fechamodificacion = NOW()
                            where parametroid = @parametroid;
                        ";

                        command.Parameters.AddWithValue("@valor", item.Valor ?? "");
                        command.Parameters.AddWithValue("@parametroid", item.ParametroId);

                        await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                model = new ParametrosIndexViewModel
                {
                    Mensaje = "Configuración actualizada correctamente."
                };

                await CargarParametros(model);
                CalcularResumenConfiguracion(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando configuración.");
                model.Error = "No se pudo guardar la configuración.";
            }

            return View("~/Views/Sistema/Configuracion.cshtml", model);
        }

        [HttpGet("Excel")]
        public async Task<IActionResult> Excel()
        {
            var model = new ParametrosIndexViewModel();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarParametros(model);
                CalcularResumenConfiguracion(model);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Configuracion");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Clave";
                worksheet.Cell(1, 3).Value = "Valor";
                worksheet.Cell(1, 4).Value = "Categoría";
                worksheet.Cell(1, 5).Value = "Descripción";
                worksheet.Cell(1, 6).Value = "Fecha modificación";

                var header = worksheet.Range(1, 1, 1, 6);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#10b981");
                header.Style.Font.FontColor = XLColor.White;

                var row = 2;

                foreach (var item in model.Parametros)
                {
                    worksheet.Cell(row, 1).Value = item.ParametroId;
                    worksheet.Cell(row, 2).Value = item.Clave;
                    worksheet.Cell(row, 3).Value = item.Valor;
                    worksheet.Cell(row, 4).Value = item.Categoria;
                    worksheet.Cell(row, 5).Value = item.Descripcion;
                    worksheet.Cell(row, 6).Value = item.FechaModificacion;
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"configuracion_sistema_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando configuración a Excel.");
                TempData["Error"] = "No se pudo exportar configuración a Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("Pdf")]
        public async Task<IActionResult> Pdf()
        {
            var model = new ParametrosIndexViewModel();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarParametros(model);
                CalcularResumenConfiguracion(model);

                QuestPDF.Settings.License = LicenseType.Community;

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Configuración del Sistema")
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
                                    c.Item().Text("Parámetros totales").FontColor("#6b7280");
                                    c.Item().Text(model.TotalParametros.ToString()).Bold().FontSize(14);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Categorías").FontColor("#6b7280");
                                    c.Item().Text(model.TotalCategorias.ToString()).Bold().FontSize(14);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(8).Column(c =>
                                {
                                    c.Item().Text("Actualizados hoy").FontColor("#6b7280");
                                    c.Item().Text(model.ActualizadosHoy.ToString()).Bold().FontSize(14);
                                });
                            });

                            foreach (var grupo in model.ParametrosPorCategoria)
                            {
                                col.Item().PaddingTop(18).Text(grupo.Key)
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor("#111827");

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background("#10b981").Padding(5).Text("Clave").FontColor(Colors.White).Bold();
                                        header.Cell().Background("#10b981").Padding(5).Text("Valor").FontColor(Colors.White).Bold();
                                        header.Cell().Background("#10b981").Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                                    });

                                    foreach (var item in grupo)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.Clave);
                                        table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.Valor);
                                        table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(5).Text(item.Descripcion ?? "-");
                                    }
                                });
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

                var fileName = $"configuracion_sistema_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando configuración a PDF.");
                TempData["Error"] = "No se pudo exportar configuración a PDF.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task CargarParametros(ParametrosIndexViewModel model)
        {
            model.Parametros.Clear();

            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = @"
                select parametroid, clave, valor, descripcion, categoria, fechamodificacion
                from sistema.parametros
                order by categoria, clave;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.Parametros.Add(new ParametroViewModel
                {
                    ParametroId = Convert.ToInt32(reader["parametroid"]),
                    Clave = reader["clave"].ToString() ?? "",
                    Valor = reader["valor"].ToString() ?? "",
                    Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"].ToString(),
                    Categoria = reader["categoria"].ToString() ?? "",
                    FechaModificacion = Convert.ToDateTime(reader["fechamodificacion"])
                });
            }
        }

        private void CalcularResumenConfiguracion(ParametrosIndexViewModel model)
        {
            model.TotalParametros = model.Parametros.Count;
            model.TotalCategorias = model.Parametros.Select(x => x.Categoria).Distinct().Count();
            model.ActualizadosHoy = model.Parametros.Count(x => x.FechaModificacion.Date == DateTime.Today);
            model.UltimaActualizacion = model.Parametros
                .OrderByDescending(x => x.FechaModificacion)
                .FirstOrDefault()
                ?.FechaModificacion;

            var grupos = model.Parametros
                .GroupBy(x => x.Categoria)
                .OrderBy(x => x.Key)
                .ToList();

            model.ChartCategoriaLabels = grupos.Select(g => g.Key).ToList();
            model.ChartCategoriaValues = grupos.Select(g => g.Count()).ToList();

            model.Recientes = model.Parametros
                .OrderByDescending(x => x.FechaModificacion)
                .Take(6)
                .ToList();
        }
    }
}