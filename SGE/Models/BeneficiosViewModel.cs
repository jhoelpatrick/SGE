namespace SGE.Models
{
    public class BeneficiosViewModel
    {
        public List<Beneficio> Beneficios    { get; set; } = new();
        public int  PaginaActual  { get; set; }
        public int  TotalPaginas  { get; set; }
        public int  TotalItems    { get; set; }
        public string Buscar      { get; set; } = "";
        public string CategoriaFiltro { get; set; } = "";
        public string EstadoFiltro    { get; set; } = "";
    }
}
