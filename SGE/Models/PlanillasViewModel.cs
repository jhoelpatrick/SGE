namespace SGE.Models
{
    public class PlanillasViewModel
    {
        public List<Planilla> Planillas { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public string Buscar { get; set; } = string.Empty;
        public string EstadoFiltro { get; set; } = string.Empty;
        public bool TienePrev => PaginaActual > 1;
        public bool TieneNext => PaginaActual < TotalPaginas;
        public int DesdeItem => (PaginaActual - 1) * 8 + 1;
        public int HastaItem => Math.Min(PaginaActual * 8, TotalItems);
    }
}