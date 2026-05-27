using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace SyS_ERP.Controllers
{
    /// <summary>
    /// Módulo de Facturación — Comprobantes SUNAT, generación de cadena QR fiscal,
    /// exportación real de PDF mínimo, asincronía de colas SUNAT y modo contingencia.
    /// </summary>
    public class FacturacionController : Controller
    {
        private readonly ILogger<FacturacionController> _logger;
        private const string RUC_EMPRESA = "20123456789";

        // Lista estática para simular persistencia
        private static readonly List<Comprobante> _comprobantes = new();

        static FacturacionController()
        {
            // Inicializar comprobantes mock en la lista estática
            var lista = new List<Comprobante>
            {
                new() { Id=1, Serie="F001", Numero="00000121", Tipo="01", Cliente="Distribuidora Norte SAC",   TipoDocCliente="6", NumDocCliente="20456789001", FechaEmision="2024-05-01", SubTotal=4110.59m, IGV=739.91m,  Total=4850.50m,  Estado="Emitida", SunatEstado="Enviado SUNAT" },
                new() { Id=2, Serie="B001", Numero="00000890", Tipo="03", Cliente="Juan Pérez García",         TipoDocCliente="1", NumDocCliente="47123456",    FechaEmision="2024-05-02", SubTotal=1016.95m, IGV=183.05m,  Total=1200.00m,  Estado="Emitida", SunatEstado="Enviado SUNAT" },
                new() { Id=3, Serie="F001", Numero="00000122", Tipo="01", Cliente="Tech Solutions Perú SAC",   TipoDocCliente="6", NumDocCliente="20315678902", FechaEmision="2024-05-05", SubTotal=6144.07m, IGV=1105.93m, Total=7250.00m,  Estado="Emitida", SunatEstado="Enviado SUNAT" },
                new() { Id=4, Serie="B001", Numero="00000891", Tipo="03", Cliente="María López Sánchez",       TipoDocCliente="1", NumDocCliente="72345678",    FechaEmision="2024-05-06", SubTotal= 423.73m, IGV=  76.27m, Total= 500.00m,  Estado="Anulada", SunatEstado="Enviado SUNAT" },
                new() { Id=5, Serie="F001", Numero="00000123", Tipo="01", Cliente="Grupo Andino Corp",         TipoDocCliente="6", NumDocCliente="20987654321", FechaEmision="2024-05-07", SubTotal=2627.12m, IGV= 472.88m, Total=3100.00m,  Estado="Emitida", SunatEstado="Enviado SUNAT" },
                new() { Id=6, Serie="B001", Numero="00000892", Tipo="03", Cliente="Carlos Mamani Quispe",      TipoDocCliente="1", NumDocCliente="29876543",    FechaEmision="2024-05-08", SubTotal= 847.46m, IGV= 152.54m, Total=1000.00m,  Estado="Emitida", SunatEstado="Enviado SUNAT" },
                new() { Id=7, Serie="F001", Numero="00000124", Tipo="01", Cliente="Importaciones Sur EIRL",    TipoDocCliente="6", NumDocCliente="20135792468", FechaEmision="2024-05-09", SubTotal=8305.08m, IGV=1494.92m, Total=9800.00m,  Estado="Emitida", SunatEstado="Contingencia" }, // offline
                new() { Id=8, Serie="F001", Numero="00000125", Tipo="01", Cliente="Megacom Distribuciones SAC",TipoDocCliente="6", NumDocCliente="20246813579", FechaEmision="2024-05-14", SubTotal=12711.86m,IGV=2288.14m, Total=15000.00m, Estado="Emitida", SunatEstado="Enviado SUNAT" },
            };

            foreach (var c in lista)
            {
                var seed = $"{RUC_EMPRESA}{c.Tipo}{c.Serie}{c.Numero}{c.Total}";
                c.CodigoHash = GenerarHash(seed);
                c.CadenaQR   = $"{RUC_EMPRESA}|{c.Tipo}|{c.Serie}-{c.Numero}|{c.IGV:F2}|{c.Total:F2}|{c.FechaEmision}|{c.TipoDocCliente}|{c.NumDocCliente}|{c.CodigoHash}|";
                _comprobantes.Add(c);
            }
        }

        public FacturacionController(ILogger<FacturacionController> logger)
            => _logger = logger;

        // ── GET /Facturacion ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new FacturacionViewModel
            {
                Comprobantes  = _comprobantes.OrderByDescending(c => c.Id).ToList(),
                TotalEmitidas = _comprobantes.Count(c => c.Estado == "Emitida"),
                TotalAnuladas = _comprobantes.Count(c => c.Estado == "Anulada"),
                MontoTotal    = _comprobantes.Where(c => c.Estado == "Emitida").Sum(c => c.Total),
                RucEmpresa    = RUC_EMPRESA
            };
            return View(vm);
        }

        // ── JsonResult: Lista de comprobantes (AJAX) ──────────────────────────
        [HttpGet]
        public IActionResult GetComprobantes()
        {
            var items = _comprobantes.OrderByDescending(c => c.Id).Select(c => new
            {
                c.Id, c.Serie, c.Numero,
                TipoDesc = c.Tipo == "01" ? "Factura" : "Boleta",
                c.Cliente, c.FechaEmision, c.Total, c.Estado, c.CadenaQR, c.SunatEstado
            });
            return Json(items);
        }

        // ── GET /Facturacion/MostrarBoleta/{id} ───────────────────────────────
        [HttpGet]
        public IActionResult MostrarBoleta(int id)
        {
            var comprobante = _comprobantes.FirstOrDefault(c => c.Id == id);
            if (comprobante is null) return NotFound();

            return View("MostrarBoleta", comprobante);
        }

        // ── POST: Emitir Comprobante ──────────────────────────────────────────
        [HttpPost]
        public IActionResult EmitirComprobante([FromBody] Comprobante nuevo, [FromQuery] bool contingencia = false)
        {
            if (nuevo == null || string.IsNullOrWhiteSpace(nuevo.Cliente) || nuevo.Total <= 0)
            {
                return Json(new { success = false, message = "Datos del comprobante inválidos." });
            }

            nuevo.Id = _comprobantes.Any() ? _comprobantes.Max(c => c.Id) + 1 : 1;
            nuevo.Serie = nuevo.Tipo == "01" ? "F001" : "B001";
            nuevo.Numero = _comprobantes.Where(c => c.Tipo == nuevo.Tipo).Any()
                ? (int.Parse(_comprobantes.Where(c => c.Tipo == nuevo.Tipo).Max(c => c.Numero) ?? "0") + 1).ToString("D8")
                : "00000001";
            nuevo.FechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
            nuevo.Estado = "Emitida";

            // Asignar estado SUNAT y encolar
            if (contingencia)
            {
                nuevo.SunatEstado = "Contingencia";
            }
            else
            {
                nuevo.SunatEstado = "En Cola"; // Irá a la cola RabbitMQ simulada
            }

            // Calcular código hash y QR
            var seed = $"{RUC_EMPRESA}{nuevo.Tipo}{nuevo.Serie}{nuevo.Numero}{nuevo.Total}";
            nuevo.CodigoHash = GenerarHash(seed);
            nuevo.CadenaQR = $"{RUC_EMPRESA}|{nuevo.Tipo}|{nuevo.Serie}-{nuevo.Numero}|{nuevo.IGV:F2}|{nuevo.Total:F2}|{nuevo.FechaEmision}|{nuevo.TipoDocCliente}|{nuevo.NumDocCliente}|{nuevo.CodigoHash}|";

            _comprobantes.Add(nuevo);

            return Json(new { 
                success = true, 
                message = contingencia 
                    ? "Comprobante emitido localmente bajo MODO CONTINGENCIA. Sincronizar después."
                    : "Comprobante encolado asíncronamente para procesamiento SUNAT.",
                comprobante = nuevo 
            });
        }

        // ── POST: Cambiar Estado Cola / Sincronizar SUNAT ──────────────────────
        [HttpPost]
        public IActionResult ProcesarColaSunat(int id)
        {
            var target = _comprobantes.FirstOrDefault(c => c.Id == id);
            if (target == null) return Json(new { success = false, message = "Comprobante no encontrado." });

            if (target.SunatEstado == "En Cola" || target.SunatEstado == "Contingencia")
            {
                target.SunatEstado = "Enviado SUNAT";
                return Json(new { success = true, message = $"Comprobante {target.Serie}-{target.Numero} enviado a SUNAT satisfactoriamente." });
            }

            return Json(new { success = false, message = "El comprobante ya fue procesado." });
        }

        // ── POST: Anular Comprobante ──────────────────────────────────────────
        [HttpPost]
        public IActionResult AnularComprobante(int id)
        {
            var target = _comprobantes.FirstOrDefault(c => c.Id == id);
            if (target == null) return Json(new { success = false, message = "Comprobante no encontrado." });

            target.Estado = "Anulada";
            return Json(new { success = true, message = "Comprobante anulado exitosamente." });
        }

        // ── GET /Facturacion/ExportarPDF/{id} ─────────────────────────────────
        [HttpGet]
        public IActionResult ExportarPDF(int id)
        {
            var comprobante = _comprobantes.FirstOrDefault(c => c.Id == id);
            if (comprobante is null) return NotFound();

            // Construir un PDF minimalista válido
            string pdfContent = 
                "%PDF-1.4\n" +
                "1 0 obj <</Type/Catalog/Pages 2 0 R>> endobj\n" +
                "2 0 obj <</Type/Pages/Kids[3 0 R]/Count 1>> endobj\n" +
                "3 0 obj <</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>> endobj\n" +
                "4 0 obj <</Length 180>> stream\n" +
                "BT\n" +
                "/F1 16 Tf\n" +
                "50 750 Td\n" +
                "(BUSINESSMANAGER CORP SAC) Tj\n" +
                "0 -25 Td\n" +
                $"(RUC: {RUC_EMPRESA}) Tj\n" +
                "0 -25 Td\n" +
                $"({(comprobante.Tipo == "01" ? "FACTURA ELECTRONICA" : "BOLETA DE VENTA")}: {comprobante.Serie}-{comprobante.Numero}) Tj\n" +
                "0 -25 Td\n" +
                $"(Cliente: {comprobante.Cliente}) Tj\n" +
                "0 -25 Td\n" +
                $"(Total: S/. {comprobante.Total:N2}) Tj\n" +
                "0 -25 Td\n" +
                $"(Codigo Hash: {comprobante.CodigoHash}) Tj\n" +
                "ET\n" +
                "endstream\n" +
                "endobj\n" +
                "5 0 obj <</Type/Font/Subtype/Type1/BaseFont/Helvetica>> endobj\n" +
                "xref\n" +
                "0 6\n" +
                "0000000000 65535 f\n" +
                "0000000009 00000 n\n" +
                "0000000056 00000 n\n" +
                "0000000111 00000 n\n" +
                "0000000223 00000 n\n" +
                "0000000453 00000 n\n" +
                "trailer <</Size 6/Root 1 0 R>>\n" +
                "startxref\n" +
                "528\n" +
                "%%EOF";

            byte[] bytes = Encoding.UTF8.GetBytes(pdfContent);
            return File(bytes, "application/pdf", $"{comprobante.Serie}-{comprobante.Numero}.pdf");
        }

        // ── GET /Facturacion/ObtenerCadenaQR/{id} ─────────────────────────────
        [HttpGet]
        public IActionResult ObtenerCadenaQR(int id)
        {
            var c = _comprobantes.FirstOrDefault(x => x.Id == id);
            if (c is null) return NotFound();

            return Json(new { cadenaQR = c.CadenaQR });
        }

        // ── GET /Facturacion/Emitir ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Emitir()
        {
            return View();
        }

        // ── POST /Facturacion/Emitir ──────────────────────────────────────────
        [HttpPost]
        public IActionResult Emitir([FromBody] FacturaCompletaRequest req)
        {
            if (req == null || req.Items == null || !req.Items.Any())
            {
                return Json(new { success = false, message = "Debe ingresar al menos un ítem al comprobante." });
            }

            // Crear el comprobante para persistir en memoria estática
            var nuevo = new Comprobante
            {
                Id = _comprobantes.Any() ? _comprobantes.Max(c => c.Id) + 1 : 1,
                Serie = req.Serie,
                Numero = req.Numero,
                Tipo = req.TipoComprobante,
                Cliente = req.ClienteNombre,
                TipoDocCliente = req.ClienteTipoDoc,
                NumDocCliente = req.ClienteNumero,
                FechaEmision = req.FechaEmision,
                SubTotal = req.TotalGravada + req.TotalExonerada + req.TotalInafecta,
                IGV = req.TotalIgv,
                Total = req.TotalNeto,
                Estado = "Emitida",
                SunatEstado = "En Cola"
            };

            // Calcular hash y QR
            var seed = $"{RUC_EMPRESA}{nuevo.Tipo}{nuevo.Serie}{nuevo.Numero}{nuevo.Total}";
            nuevo.CodigoHash = GenerarHash(seed);
            nuevo.CadenaQR = $"{RUC_EMPRESA}|{nuevo.Tipo}|{nuevo.Serie}-{nuevo.Numero}|{nuevo.IGV:F2}|{nuevo.Total:F2}|{nuevo.FechaEmision}|{nuevo.TipoDocCliente}|{nuevo.NumDocCliente}|{nuevo.CodigoHash}|";

            _comprobantes.Add(nuevo);

            string uuid = Guid.NewGuid().ToString();

            return Json(new { 
                success = true, 
                message = "Comprobante emitido con éxito y enviado a la cola OSE/SUNAT.",
                id = nuevo.Id,
                uuid = uuid,
                hash = nuevo.CodigoHash,
                xmlUrl = $"/Facturacion/ExportarPDF/{nuevo.Id}",
                cdrUrl = "#",
                qrData = nuevo.CadenaQR
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MÉTODOS PRIVADOS DE APOYO
        // ══════════════════════════════════════════════════════════════════════
        private static string GenerarHash(string contenido)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(contenido));
            return Convert.ToBase64String(bytes)[..20]; // primeros 20 chars
        }
    }

    // ── Estructuras SUNAT UBL 2.1 para Recibir Datos Complejos ───────────────
    public class FacturaCompletaRequest
    {
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string FechaEmision { get; set; } = string.Empty;
        public string Moneda { get; set; } = "PEN";
        public decimal TipoCambio { get; set; } = 1.00m;
        public string TipoOperacion { get; set; } = string.Empty;
        public string ReferenciaDoc { get; set; } = string.Empty;

        // Cliente
        public string ClienteTipoDoc { get; set; } = string.Empty;
        public string ClienteNumero { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteDireccion { get; set; } = string.Empty;
        public string ClienteUbigeo { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;

        // Totales
        public decimal TotalGravada { get; set; }
        public decimal TotalExonerada { get; set; }
        public decimal TotalInafecta { get; set; }
        public decimal TotalGratuita { get; set; }
        public decimal TotalIgv { get; set; }
        public decimal TotalIsc { get; set; }
        public decimal TotalIcbper { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalNeto { get; set; }
        public string MontoLetras { get; set; } = string.Empty;

        // Pago
        public string CondicionPago { get; set; } = "Contado"; // Contado | Credito
        public string MetodoPago { get; set; } = string.Empty;
        public string Banco { get; set; } = string.Empty;
        public string CuentaBancaria { get; set; } = string.Empty;
        public string NumeroOperacion { get; set; } = string.Empty;
        public List<CuotaPago> Cuotas { get; set; } = new();

        // Detalle
        public List<DetalleItemRequest> Items { get; set; } = new();
    }

    public class CuotaPago
    {
        public int Numero { get; set; }
        public string FechaVencimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

    public class DetalleItemRequest
    {
        public string Producto { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string CodigoSunat { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = "NIU";
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Descuento { get; set; }
        public string AfectacionIgv { get; set; } = "10";
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
    }
}
