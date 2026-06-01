namespace SGE.Models
{
    // ── ViewModel principal para lista de empleados ────────────────
    public class EmpleadoViewModel
    {
        public List<Empleado> Empleados { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItems { get; set; }
        public string BuscarFiltro { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
        public string DeptFiltro { get; set; } = "";
        public string ContratoFiltro { get; set; } = "";

        // Contadores para tarjetas del dashboard
        public int TotalActivos => Empleados.Count(e => e.Estado == EstadoEmpleado.Activo);
        public int TotalInactivos => Empleados.Count(e => e.Estado == EstadoEmpleado.Inactivo);
        public int TotalVacaciones => Empleados.Count(e => e.Estado == EstadoEmpleado.Vacaciones);
        public decimal MassaSalarial => Empleados.Where(e => e.Estado == EstadoEmpleado.Activo)
                                                  .Sum(e => e.SueldoBase);
    }

    // ── ViewModel para boleta de pago individual ───────────────────
    public class BoletaPagoViewModel
    {
        public Empleado Empleado { get; set; } = new();
        public DetallePlanilla Detalle { get; set; } = new();
        public ParametrosGenerales Config { get; set; } = new();
        public string Periodo { get; set; } = "";

        // Datos de empresa para cabecera de boleta
        public string EmpresaRUC { get; set; } = "20123456789";
        public string EmpresaNombre { get; set; } = "Mi Empresa S.A.C.";
        public string EmpresaDireccion { get; set; } = "Lima, Perú";
    }
}