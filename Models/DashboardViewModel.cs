namespace SyS_ERP.Models
{
    /// <summary>
    /// ViewModel tipado para los indicadores clave del Dashboard principal
    /// y los contadores rápidos de los 5 módulos de Operaciones.
    /// </summary>
    public class DashboardViewModel
    {
        // ── KPIs Globales ──────────────────────────────────────────────────────
        public string UsuariosActivos  { get; set; } = "0";
        public string VentasDelMes     { get; set; } = "0";
        public string FacturasEmitidas { get; set; } = "0";
        public string IngresosTotales  { get; set; } = "$0";

        // ── Contadores rápidos del Módulo de Operaciones ──────────────────────
        public int PedidosPendientes  { get; set; } = 0;
        public int OrdenesCompra      { get; set; } = 0;
        public int ComprobantesMes    { get; set; } = 0;
        public int AlertasStock       { get; set; } = 0;
        public int ProyectosActivos   { get; set; } = 0;
    }
}
