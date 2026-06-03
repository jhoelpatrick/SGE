namespace SGE.Models.SistemaModel
{
    public class ReporteViewModel
    {
        public int ReporteId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string ModuloOrigen { get; set; } = string.Empty;
        public string ProcedimientoNombre { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public int TotalRegistros { get; set; }
    }

    public class ReportesIndexViewModel
    {
        public List<ReporteViewModel> Reportes { get; set; } = new();
        public List<string> Modulos { get; set; } = new();

        public string? ModuloFiltro { get; set; }
        public bool? EstadoFiltro { get; set; }

        public int TotalReportes { get; set; }
        public int ReportesActivos { get; set; }
        public int TotalRegistrosAnalizados { get; set; }
        public int TotalModulos { get; set; }

        public List<string> ChartModuloLabels { get; set; } = new();
        public List<int> ChartModuloValues { get; set; } = new();

        public List<string> ChartTopLabels { get; set; } = new();
        public List<int> ChartTopValues { get; set; } = new();

        public string? Error { get; set; }
    }
}