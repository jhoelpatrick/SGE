namespace SyS_ERP.Models.ViewModels
{
    public class OrdenCompra
    {
        public int     Id          { get; set; }
        public string  NroOrden    { get; set; } = string.Empty;
        public string  Proveedor   { get; set; } = string.Empty;
        public string  Fecha       { get; set; } = string.Empty;
        public decimal Monto       { get; set; }
        public string  Estado      { get; set; } = "Pendiente"; // Pendiente | Aprobado | Rechazado
        public string  Solicitante { get; set; } = string.Empty;
    }

    public class ComprasViewModel
    {
        public List<OrdenCompra> Ordenes              { get; set; } = new();
        public int               PendientesAprobacion { get; set; }
        public decimal           GastoMes             { get; set; }
    }
}
