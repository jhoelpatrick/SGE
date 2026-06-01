namespace SGE.Models
{
    public class HistorialPagosViewModel
    {
        public List<PagoPlanilla> Pagos        { get; set; } = new();
        public int    PaginaActual   { get; set; }
        public int    TotalPaginas   { get; set; }
        public int    TotalItems     { get; set; }
        public string Buscar         { get; set; } = "";
        public string EstadoFiltro   { get; set; } = "";
        public string MedioFiltro    { get; set; } = "";
        public string PeriodoFiltro  { get; set; } = "";
    }
}
