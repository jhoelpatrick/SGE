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
    public class ReportesController : Controller
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(IDbConnection connection, ILogger<ReportesController> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? moduloFiltro, bool? estadoFiltro)
        {
            var model = new ReportesIndexViewModel
            {
                ModuloFiltro = moduloFiltro,
                EstadoFiltro = estadoFiltro
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarModulosReportes(model);
                await CargarReportes(model);
                CalcularResumenReportes(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando reportes.");
                model.Error = "No se pudieron cargar los reportes.";
            }

            return View("~/Views/Sistema/Reportes.cshtml", model);
        }

        public async Task<IActionResult> Excel(string? moduloFiltro, bool? estadoFiltro)
        {
            var model = new ReportesIndexViewModel
            {
                ModuloFiltro = moduloFiltro,
                EstadoFiltro = estadoFiltro
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarReportes(model);
                CalcularResumenReportes(model);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Reportes");

                worksheet.Cell(1, 1).Value = "Código";
                worksheet.Cell(1, 2).Value = "Reporte";
                worksheet.Cell(1, 3).Value = "Descripción";
                worksheet.Cell(1, 4).Value = "Módulo";
                worksheet.Cell(1, 5).Value = "Procedimiento";
                worksheet.Cell(1, 6).Value = "Estado";
                worksheet.Cell(1, 7).Value = "Total registros";

                var header = worksheet.Range(1, 1, 1, 7);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#10b981");
                header.Style.Font.FontColor = XLColor.White;

                var row = 2;

                foreach (var item in model.Reportes)
                {
                    worksheet.Cell(row, 1).Value = item.Codigo;
                    worksheet.Cell(row, 2).Value = item.Nombre;
                    worksheet.Cell(row, 3).Value = item.Descripcion;
                    worksheet.Cell(row, 4).Value = item.ModuloOrigen;
                    worksheet.Cell(row, 5).Value = item.ProcedimientoNombre;
                    worksheet.Cell(row, 6).Value = item.EstaActivo ? "Activo" : "Inactivo";
                    worksheet.Cell(row, 7).Value = item.TotalRegistros;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"reportes_sistema_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando Excel.");
                TempData["Error"] = "No se pudo exportar el Excel.";
                return RedirectToAction(nameof(Index), new { moduloFiltro, estadoFiltro });
            }
        }

        public async Task<IActionResult> Pdf(string? moduloFiltro, bool? estadoFiltro)
        {
            var model = new ReportesIndexViewModel
            {
                ModuloFiltro = moduloFiltro,
                EstadoFiltro = estadoFiltro
            };

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                await CargarReportes(model);
                CalcularResumenReportes(model);

                QuestPDF.Settings.License = LicenseType.Community;

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Reportes del Sistema")
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
                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(10).Column(c =>
                                {
                                    c.Item().Text("Reportes totales").FontColor("#6b7280");
                                    c.Item().Text(model.TotalReportes.ToString()).Bold().FontSize(16);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(10).Column(c =>
                                {
                                    c.Item().Text("Reportes activos").FontColor("#6b7280");
                                    c.Item().Text(model.ReportesActivos.ToString()).Bold().FontSize(16);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(10).Column(c =>
                                {
                                    c.Item().Text("Registros analizados").FontColor("#6b7280");
                                    c.Item().Text(model.TotalRegistrosAnalizados.ToString("N0")).Bold().FontSize(16);
                                });

                                row.RelativeItem().Border(1).BorderColor("#e5e7eb").Padding(10).Column(c =>
                                {
                                    c.Item().Text("Módulos cubiertos").FontColor("#6b7280");
                                    c.Item().Text(model.TotalModulos.ToString()).Bold().FontSize(16);
                                });
                            });

                            col.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.ConstantColumn(90);
                                    columns.ConstantColumn(75);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#10b981").Padding(6).Text("Código").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(6).Text("Reporte").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(6).Text("Módulo").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(6).Text("Total").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#10b981").Padding(6).Text("Estado").FontColor(Colors.White).Bold();
                                });

                                foreach (var item in model.Reportes)
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(item.Codigo);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(item.Nombre);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(item.ModuloOrigen);
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(item.TotalRegistros.ToString("N0"));
                                    table.Cell().BorderBottom(1).BorderColor("#e5e7eb").Padding(6).Text(item.EstaActivo ? "Activo" : "Inactivo");
                                }
                            });
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

                var fileName = $"reportes_sistema_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando PDF.");
                TempData["Error"] = "No se pudo exportar el PDF.";
                return RedirectToAction(nameof(Index), new { moduloFiltro, estadoFiltro });
            }
        }

        private async Task CargarModulosReportes(ReportesIndexViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = @"
                select distinct moduloorigen
                from sistema.reportes_config
                order by moduloorigen;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.Modulos.Add(reader["moduloorigen"].ToString() ?? "");
            }
        }

        private async Task CargarReportes(ReportesIndexViewModel model)
        {
            using var command = ((NpgsqlConnection)_connection).CreateCommand();

            command.CommandText = @"
                select 
                    rc.reporteid,
                    rc.codigo,
                    rc.nombre,
                    rc.descripcion,
                    rc.moduloorigen,
                    rc.procedimientonombre,
                    rc.estaactivo,

                    case 
                        when lower(rc.codigo) like '%cli%' 
                          or lower(rc.nombre) like '%cliente%'
                          or lower(coalesce(rc.descripcion, '')) like '%cliente%'
                            then (select count(*) from comercial.clientes)

                        when lower(rc.codigo) like '%prov%' 
                          or lower(rc.nombre) like '%proveedor%'
                          or lower(coalesce(rc.descripcion, '')) like '%proveedor%'
                            then (select count(*) from comercial.proveedores)

                        when lower(rc.codigo) like '%prod%' 
                          or lower(rc.nombre) like '%producto%'
                          or lower(coalesce(rc.descripcion, '')) like '%producto%'
                            then (select count(*) from comercial.productos)

                        when lower(rc.codigo) like '%ped%' 
                          or lower(rc.nombre) like '%pedido%'
                          or lower(coalesce(rc.descripcion, '')) like '%pedido%'
                            then (select count(*) from operaciones.pedidosventa)

                        when lower(rc.codigo) like '%oc%' 
                          or lower(rc.nombre) like '%orden%'
                          or lower(rc.nombre) like '%compra%'
                          or lower(coalesce(rc.descripcion, '')) like '%orden%'
                            then (select count(*) from operaciones.ordenescompra)

                        when lower(rc.codigo) like '%comp%' 
                          or lower(rc.nombre) like '%comprobante%'
                          or lower(coalesce(rc.descripcion, '')) like '%comprobante%'
                            then (select count(*) from operaciones.comprobantesfacturacion)

                        when lower(rc.codigo) like '%proy%' 
                          or lower(rc.nombre) like '%proyecto%'
                          or lower(coalesce(rc.descripcion, '')) like '%proyecto%'
                            then (select count(*) from operaciones.proyectos)

                        when lower(rc.codigo) like '%emp%' 
                          or lower(rc.nombre) like '%empleado%'
                          or lower(coalesce(rc.descripcion, '')) like '%empleado%'
                            then (select count(*) from rrhh_recursos.empleados)

                        else 1
                    end as totalregistros

                from sistema.reportes_config rc
                where (@moduloFiltro is null or rc.moduloorigen = @moduloFiltro)
                  and (@estadoFiltro is null or rc.estaactivo = @estadoFiltro)
                order by rc.moduloorigen, rc.nombre;
            ";

            command.Parameters.AddWithValue("@moduloFiltro", string.IsNullOrWhiteSpace(model.ModuloFiltro) ? DBNull.Value : model.ModuloFiltro);
            command.Parameters.AddWithValue("@estadoFiltro", model.EstadoFiltro.HasValue ? model.EstadoFiltro.Value : DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                model.Reportes.Add(new ReporteViewModel
                {
                    ReporteId = Convert.ToInt32(reader["reporteid"]),
                    Codigo = reader["codigo"].ToString() ?? "",
                    Nombre = reader["nombre"].ToString() ?? "",
                    Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"].ToString(),
                    ModuloOrigen = reader["moduloorigen"].ToString() ?? "",
                    ProcedimientoNombre = reader["procedimientonombre"].ToString() ?? "",
                    EstaActivo = Convert.ToBoolean(reader["estaactivo"]),
                    TotalRegistros = Convert.ToInt32(reader["totalregistros"])
                });
            }
        }

        private void CalcularResumenReportes(ReportesIndexViewModel model)
        {
            model.TotalReportes = model.Reportes.Count;
            model.ReportesActivos = model.Reportes.Count(x => x.EstaActivo);
            model.TotalRegistrosAnalizados = model.Reportes.Sum(x => x.TotalRegistros);
            model.TotalModulos = model.Reportes.Select(x => x.ModuloOrigen).Distinct().Count();

            var reportesPorModulo = model.Reportes
                .GroupBy(x => x.ModuloOrigen)
                .Select(g => new
                {
                    Modulo = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            model.ChartModuloLabels = reportesPorModulo
                .Select(x => x.Modulo)
                .ToList();

            model.ChartModuloValues = reportesPorModulo
                .Select(x => x.Total)
                .ToList();

            var top = model.Reportes
                .OrderByDescending(x => x.TotalRegistros)
                .Take(6)
                .ToList();

            model.ChartTopLabels = top
                .Select(x => x.Nombre)
                .ToList();

            model.ChartTopValues = top
                .Select(x => x.TotalRegistros > 0 ? x.TotalRegistros : 1)
                .ToList();
        }
    }
}