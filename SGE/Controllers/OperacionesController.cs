using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SGE.Models;
using SGE.Services;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SGE.Controllers
{
    public class OperacionesController : Controller
    {
        private readonly IProyectoRepository _proyectos;
        private readonly IVentaRepository _ventas;
        private readonly ICompraRepository _compras;
        private readonly IFacturacionRepository _facturacion;
        private readonly IInventarioRepository _inventario;
        private readonly IClienteRepository _clientes;
        private readonly IProveedorRepository _proveedores;
        private readonly IConfiguration _configuration;

        public OperacionesController(
            IProyectoRepository proyectos,
            IVentaRepository ventas,
            ICompraRepository compras,
            IFacturacionRepository facturacion,
            IInventarioRepository inventario,
            IClienteRepository clientes,
            IProveedorRepository proveedores,
            IConfiguration configuration)
        {
            _proyectos = proyectos;
            _ventas = ventas;
            _compras = compras;
            _facturacion = facturacion;
            _inventario = inventario;
            _clientes = clientes;
            _proveedores = proveedores;
            _configuration = configuration;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── ACCIONES PARA CARGAR LAS VISTAS PARCIALES ─────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        public IActionResult Ventas() => PartialView();
        public IActionResult Compras() => PartialView();
        public IActionResult Facturacion() => PartialView();
        public IActionResult Inventario() => PartialView();
        public IActionResult Proyectos() => PartialView();

        // ══════════════════════════════════════════════════════════════════════
        // ── API: PROYECTOS ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetProyectosJson()
        {
            try
            {
                var list = await _proyectos.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProyecto([FromBody] Proyecto model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreProyecto))
                return Json(new { ok = false, error = "El nombre del proyecto es obligatorio." });

            try
            {
                var newId = await _proyectos.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTareasJson(int id)
        {
            try
            {
                var list = await _proyectos.GetTareasByProyectoIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTarea([FromBody] ProyectoTarea model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreTarea))
                return Json(new { ok = false, error = "El nombre de la tarea es obligatorio." });

            try
            {
                var newId = await _proyectos.CreateTareaAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTareaEstado([FromBody] UpdateTareaEstadoRequest req)
        {
            try
            {
                await _proyectos.UpdateTareaEstadoAsync(req.TareaId, req.PorcentajeProgreso, req.Estado);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: VENTAS ───────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetVentasJson()
        {
            try
            {
                var list = await _ventas.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVentaDetalleJson(int id)
        {
            try
            {
                var list = await _ventas.GetDetalleByPedidoIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePedido([FromBody] PedidoVenta model)
        {
            try
            {
                var newId = await _ventas.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePedido([FromBody] int id)
        {
            try
            {
                await _ventas.ApproveAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPedido([FromBody] int id)
        {
            try
            {
                await _ventas.CancelAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispatchPedido([FromBody] DispatchPedidoRequest req)
        {
            try
            {
                await _ventas.DispatchAsync(req.PedId, req.VehId, req.CondId, req.Serie, req.Correlativo);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: COMPRAS ──────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetComprasJson()
        {
            try
            {
                var list = await _compras.GetAllAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompraDetalleJson(int id)
        {
            try
            {
                var list = await _compras.GetDetalleByOrdenIdAsync(id);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompra([FromBody] OrdenCompra model)
        {
            try
            {
                var newId = await _compras.CreateAsync(model);
                return Json(new { ok = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCompra([FromBody] int id)
        {
            try
            {
                await _compras.ApproveAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCompra([FromBody] int id)
        {
            try
            {
                await _compras.RejectAsync(id);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: FACTURACIÓN ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetComprobantesJson()
        {
            try
            {
                var list = await _facturacion.GetAllInvoicesAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGuiasJson()
        {
            try
            {
                var list = await _facturacion.GetAllGuidesAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest req)
        {
            try
            {
                var compId = await _facturacion.EmitirFacturaDesdePedidoAsync(req.PedidoId, req.TipoComprobante, req.Serie);
                return Json(new { ok = true, id = compId });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingBillingOrders()
        {
            try
            {
                var list = await _facturacion.GetPendingBillingOrdersAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var cn = new NpgsqlConnection(connectionString);
                await cn.OpenAsync();

                // 1. Fetch invoice info
                const string sqlInvoice = @"
                    SELECT cf.tipocomprobante, cf.serie, cf.correlativo, cf.fechaemision, 
                           cf.opgravada, cf.igv_total, cf.importetotalneto, cf.moneda,
                           c.razonsocial, c.numerodocumento, cf.pedidoid
                    FROM   operaciones.comprobantesfacturacion cf
                    INNER JOIN comercial.clientes c ON cf.clienteid = c.clienteid
                    WHERE  cf.comprobanteid = @id";

                string tipo = "01";
                string serie = "";
                string correlativo = "";
                DateTime fecha = DateTime.Now;
                decimal opGravada = 0;
                decimal igv = 0;
                decimal total = 0;
                string moneda = "PEN";
                string clienteNombre = "";
                string clienteRuc = "";
                int? pedidoId = null;

                using (var cmd = new NpgsqlCommand(sqlInvoice, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        tipo = rd.GetString(0);
                        serie = rd.GetString(1);
                        correlativo = rd.GetString(2);
                        fecha = rd.GetDateTime(3);
                        opGravada = rd.GetDecimal(4);
                        igv = rd.GetDecimal(5);
                        total = rd.GetDecimal(6);
                        moneda = rd.GetString(7);
                        clienteNombre = rd.GetString(8);
                        clienteRuc = rd.GetString(9);
                        pedidoId = rd.IsDBNull(10) ? null : rd.GetInt32(10);
                    }
                    else
                    {
                        return NotFound("El comprobante no existe.");
                    }
                }

                // 2. Fetch invoice details
                var items = new List<(string sku, string desc, decimal qty, decimal price, decimal rowTotal)>();
                if (pedidoId.HasValue)
                {
                    const string sqlDetails = @"
                        SELECT prod.codigosku, prod.descripcion, pd.cantidad, pd.preciounitariocongiv, (pd.cantidad * pd.preciounitariocongiv) as total
                        FROM   operaciones.pedidosventadetalle pd
                        INNER JOIN comercial.productos prod ON pd.productoid = prod.productoid
                        WHERE  pd.pedidoid = @pedidoId";
                    using var cmdDet = new NpgsqlCommand(sqlDetails, cn);
                    cmdDet.Parameters.AddWithValue("@pedidoId", pedidoId.Value);
                    using var rd = await cmdDet.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        items.Add((
                            rd.GetString(0),
                            rd.GetString(1),
                            rd.GetDecimal(2),
                            rd.GetDecimal(3),
                            rd.GetDecimal(4)
                        ));
                    }
                }

                // 3. Generate PDF using QuestPDF
                QuestPDF.Settings.License = LicenseType.Community;

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Header with company details and invoice box
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Modular ERP S.A.C.").FontSize(18).Bold().FontColor("#0d9488");
                                col.Item().Text("Av. Diagonal 450, Miraflores, Lima").FontSize(9).FontColor("#6b7280");
                                col.Item().Text("RUC: 20123456789").FontSize(9).FontColor("#6b7280");
                                col.Item().Text("Email: contacto@modular-erp.pe").FontSize(9).FontColor("#6b7280");
                            });

                            row.ConstantItem(180).Border(2).BorderColor("#0d9488").Padding(10).AlignCenter().Column(col =>
                            {
                                col.Item().Text("R.U.C. 20123456789").Bold().FontSize(12).AlignCenter();
                                col.Item().Text(tipo == "01" ? "FACTURA ELECTRÓNICA" : "BOLETA ELECTRÓNICA").Bold().FontSize(10).AlignCenter().FontColor("#0d9488");
                                col.Item().Text($"{serie}-{correlativo}").Bold().FontSize(12).AlignCenter();
                            });
                        });

                        // Content (Client Details and Table)
                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            // Client details box
                            col.Item().Background("#f8fafc").Border(1).BorderColor("#e2e8f0").Padding(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(t => { t.Span("Señor(es): ").Bold(); t.Span(clienteNombre); });
                                    c.Item().Text(t => { t.Span("R.U.C./D.N.I.: ").Bold(); t.Span(clienteRuc); });
                                    c.Item().Text(t => { t.Span("Fecha Emisión: ").Bold(); t.Span(fecha.ToString("dd/MM/yyyy")); });
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(t => { t.Span("Moneda: ").Bold(); t.Span(moneda == "PEN" ? "Soles (S/.)" : "Dólares ($)"); });
                                    c.Item().Text(t => { t.Span("Guía de Remisión: ").Bold(); t.Span("Ver Guías"); });
                                    c.Item().Text(t => { t.Span("Condición Pago: ").Bold(); t.Span("Contado"); });
                                });
                            });

                            // Spacer
                            col.Item().PaddingVertical(10);

                            // Items Table
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80); // SKU
                                    columns.RelativeColumn(3);  // Description
                                    columns.ConstantColumn(60); // Qty
                                    columns.ConstantColumn(80); // Price
                                    columns.ConstantColumn(80); // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background("#0d9488").Padding(5).Text("Código").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#0d9488").Padding(5).Text("Descripción").FontColor(Colors.White).Bold();
                                    header.Cell().Background("#0d9488").Padding(5).Text("Cant.").FontColor(Colors.White).Bold().AlignRight();
                                    header.Cell().Background("#0d9488").Padding(5).Text("P. Unit").FontColor(Colors.White).Bold().AlignRight();
                                    header.Cell().Background("#0d9488").Padding(5).Text("Total").FontColor(Colors.White).Bold().AlignRight();
                                });

                                foreach (var item in items)
                                {
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(item.sku);
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(item.desc);
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(item.qty.ToString("N2")).AlignRight();
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(item.price.ToString("N2")).AlignRight();
                                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(5).Text(item.rowTotal.ToString("N2")).AlignRight();
                                }
                            });

                            // Spacer
                            col.Item().PaddingVertical(10);

                            // Totals Row
                            col.Item().AlignRight().Width(200).Column(totalsCol =>
                            {
                                totalsCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Op. Gravada:").AlignRight();
                                    r.ConstantItem(80).Text($"S/. {opGravada:N2}").AlignRight();
                                });
                                totalsCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("I.G.V. (18%):").AlignRight();
                                    r.ConstantItem(80).Text($"S/. {igv:N2}").AlignRight();
                                });
                                totalsCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Importe Total:").AlignRight().Bold();
                                    r.ConstantItem(80).Text($"S/. {total:N2}").AlignRight().Bold().FontColor("#0d9488");
                                });
                            });
                        });

                        // Footer
                        page.Footer().AlignCenter().Column(col =>
                        {
                            col.Item().Text("Representación impresa de la factura electrónica").FontSize(8).FontColor("#6b7280").AlignCenter();
                            col.Item().Text("Autorizado mediante Resolución del SUNAT. Consulte su comprobante en: sunat.gob.pe").FontSize(8).FontColor("#6b7280").AlignCenter();
                        });
                    });
                }).GeneratePdf();

                var fileName = $"comprobante_{serie}_{correlativo}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar el PDF: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarXml(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using var cn = new NpgsqlConnection(connectionString);
                await cn.OpenAsync();

                // Fetch invoice info
                const string sqlInvoice = @"
                    SELECT cf.tipocomprobante, cf.serie, cf.correlativo, cf.fechaemision, 
                           cf.opgravada, cf.igv_total, cf.importetotalneto, cf.moneda,
                           c.razonsocial, c.numerodocumento
                    FROM   operaciones.comprobantesfacturacion cf
                    INNER JOIN comercial.clientes c ON cf.clienteid = c.clienteid
                    WHERE  cf.comprobanteid = @id";

                string tipo = "01";
                string serie = "";
                string correlativo = "";
                DateTime fecha = DateTime.Now;
                decimal opGravada = 0;
                decimal igv = 0;
                decimal total = 0;
                string moneda = "PEN";
                string clienteNombre = "";
                string clienteRuc = "";

                using (var cmd = new NpgsqlCommand(sqlInvoice, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        tipo = rd.GetString(0);
                        serie = rd.GetString(1);
                        correlativo = rd.GetString(2);
                        fecha = rd.GetDateTime(3);
                        opGravada = rd.GetDecimal(4);
                        igv = rd.GetDecimal(5);
                        total = rd.GetDecimal(6);
                        moneda = rd.GetString(7);
                        clienteNombre = rd.GetString(8);
                        clienteRuc = rd.GetString(9);
                    }
                    else
                    {
                        return NotFound("El comprobante no existe.");
                    }
                }

                // Generate UBL 2.1 SUNAT XML mock
                string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
         xmlns:ds=""http://www.w3.org/2000/08/xmldsig#""
         xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
    <ext:UBLExtensions>
        <ext:UBLExtension>
            <ext:ExtensionContent>
                <ds:Signature Id=""SignModularERP"">
                    <ds:SignedInfo>
                        <ds:CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/>
                        <ds:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
                    </ds:SignedInfo>
                    <ds:SignatureValue>MOCK_SIGNATURE_VALUE_FOR_DEMO_PURPOSES_ONLY_SUNAT_ACCEPTED</ds:SignatureValue>
                </ds:Signature>
            </ext:ExtensionContent>
        </ext:UBLExtension>
    </ext:UBLExtensions>
    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
    <cbc:CustomizationID>2.0</cbc:CustomizationID>
    <cbc:ID>{serie}-{correlativo}</cbc:ID>
    <cbc:IssueDate>{fecha:yyyy-MM-dd}</cbc:IssueDate>
    <cbc:IssueTime>{fecha:HH:mm:ss}</cbc:IssueTime>
    <cbc:InvoiceTypeCode listID=""0101"">{tipo}</cbc:InvoiceTypeCode>
    <cbc:DocumentCurrencyCode>{moneda}</cbc:DocumentCurrencyCode>
    <cac:Signature>
        <cbc:ID>SignModularERP</cbc:ID>
        <cac:SignatoryParty>
            <cac:PartyIdentification>
                <cbc:ID>20123456789</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[Modular ERP S.A.C.]]></cbc:Name>
            </cac:PartyName>
        </cac:SignatoryParty>
    </cac:Signature>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID=""6"">20123456789</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[Modular ERP S.A.C.]]></cbc:Name>
            </cac:PartyName>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:AccountingCustomerParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID=""6"">{clienteRuc}</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName><![CDATA[{clienteNombre}]]></cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingCustomerParty>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""{moneda}"">{igv:F2}</cbc:TaxAmount>
        <cac:TaxSubtotal>
            <cbc:TaxableAmount currencyID=""{moneda}"">{opGravada:F2}</cbc:TaxableAmount>
            <cbc:TaxAmount currencyID=""{moneda}"">{igv:F2}</cbc:TaxAmount>
            <cac:TaxCategory>
                <cac:TaxScheme>
                    <cbc:ID>1000</cbc:ID>
                    <cbc:Name>IGV</cbc:Name>
                    <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                </cac:TaxScheme>
            </cac:TaxCategory>
        </cac:TaxSubtotal>
    </cac:TaxTotal>
    <cac:LegalMonetaryTotal>
        <cbc:LineExtensionAmount currencyID=""{moneda}"">{opGravada:F2}</cbc:LineExtensionAmount>
        <cbc:TaxInclusiveAmount currencyID=""{moneda}"">{total:F2}</cbc:TaxInclusiveAmount>
        <cbc:PayableAmount currencyID=""{moneda}"">{total:F2}</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>
</Invoice>";

                var fileName = $"comprobante_{serie}_{correlativo}.xml";
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar el XML: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehiculosJson()
        {
            try
            {
                var list = await GetVehiculosAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConductoresJson()
        {
            try
            {
                var list = await GetConductoresAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── API: INVENTARIO ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetStockJson()
        {
            try
            {
                var list = await _inventario.GetStockSummaryAsync();
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKardexJson(int productoId)
        {
            try
            {
                var list = await _inventario.GetKardexByProductoIdAsync(productoId);
                return Json(new { ok = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMovimientoManual([FromBody] ManualMovementRequest req)
        {
            try
            {
                await _inventario.RegistrarMovimientoManualAsync(req.ProductoId, req.TipoMovimiento, req.Cantidad, req.Motivo);
                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── HELPERS INTERNOS ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private async Task<List<Vehiculo>> GetVehiculosAsync()
        {
            var list = new List<Vehiculo>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var cn = new NpgsqlConnection(connectionString);
            await cn.OpenAsync();
            const string sql = @"
                SELECT v.vehiculoid, v.proveedorid, v.placa, v.marca, v.modelo, v.tipovehiculo, v.estado, p.razonsocial
                FROM   comercial.vehiculosproveedores v
                INNER JOIN comercial.proveedores p ON v.proveedorid = p.proveedorid
                WHERE  v.estado = true";
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new Vehiculo
                {
                    VehiculoId = rd.GetInt32(0),
                    ProveedorId = rd.GetInt32(1),
                    Placa = rd.GetString(2),
                    Marca = rd.IsDBNull(3) ? "" : rd.GetString(3),
                    Modelo = rd.IsDBNull(4) ? "" : rd.GetString(4),
                    TipoVehiculo = rd.IsDBNull(5) ? "" : rd.GetString(5),
                    Estado = rd.GetBoolean(6),
                    ProveedorNombre = rd.GetString(7)
                });
            }
            return list;
        }

        private async Task<List<Conductor>> GetConductoresAsync()
        {
            var list = new List<Conductor>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var cn = new NpgsqlConnection(connectionString);
            await cn.OpenAsync();
            const string sql = @"
                SELECT conductorid, proveedorid, nombre, tipodocumento, numerodocumento, licenciaconducir, estado
                FROM   comercial.conductoresproveedores
                WHERE  estado = true";
            using var cmd = new NpgsqlCommand(sql, cn);
            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new Conductor
                {
                    ConductorId = rd.GetInt32(0),
                    ProveedorId = rd.GetInt32(1),
                    Nombre = rd.GetString(2),
                    NumeroDocumento = rd.GetString(4),
                    LicenciaConducir = rd.IsDBNull(5) ? "" : rd.GetString(5),
                    Estado = rd.GetBoolean(6)
                });
            }
            return list;
        }
    }

    // DTO classes for request body binding
    public class UpdateTareaEstadoRequest
    {
        public int TareaId { get; set; }
        public decimal PorcentajeProgreso { get; set; }
        public string Estado { get; set; } = "";
    }

    public class DispatchPedidoRequest
    {
        public int PedId { get; set; }
        public int VehId { get; set; }
        public int CondId { get; set; }
        public string Serie { get; set; } = "";
        public string Correlativo { get; set; } = "";
    }

    public class EmitirFacturaRequest
    {
        public int PedidoId { get; set; }
        public string TipoComprobante { get; set; } = "01";
        public string Serie { get; set; } = "F001";
    }

    public class ManualMovementRequest
    {
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string ContextoReferencia { get; set; } = "";
        public string Motivo { get; set; } = "";
    }
}
