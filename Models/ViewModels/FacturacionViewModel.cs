namespace SyS_ERP.Models.ViewModels
{
    public class Comprobante
    {
        public int     Id              { get; set; }
        public string  Serie           { get; set; } = string.Empty;
        public string  Numero          { get; set; } = string.Empty;
        public string  Tipo            { get; set; } = "01"; // 01=Factura, 03=Boleta
        public string  Cliente         { get; set; } = string.Empty;
        public string  TipoDocCliente  { get; set; } = "6"; // 6=RUC, 1=DNI
        public string  NumDocCliente   { get; set; } = string.Empty;
        public string  FechaEmision    { get; set; } = string.Empty;
        public decimal SubTotal        { get; set; }
        public decimal IGV             { get; set; }
        public decimal Total           { get; set; }
        public string  Estado          { get; set; } = "Emitida"; // Emitida | Anulada
        public string  CadenaQR        { get; set; } = string.Empty;
        public string  CodigoHash      { get; set; } = string.Empty;
        public string  SunatEstado     { get; set; } = "Enviado SUNAT"; // Enviado SUNAT | En Cola | Contingencia
    }

    public class FacturacionViewModel
    {
        public List<Comprobante> Comprobantes   { get; set; } = new();
        public int               TotalEmitidas  { get; set; }
        public int               TotalAnuladas  { get; set; }
        public decimal           MontoTotal     { get; set; }

        // ── RUC de la empresa (configurable) ──────────────────────────────
        public string RucEmpresa { get; set; } = "20123456789";
    }
}
