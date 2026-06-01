namespace SGE.Models
{
    public class DescuentosViewModel
    {
        public List<Descuento> Descuentos    { get; set; } = new();
        public int             PaginaActual  { get; set; }
        public int             TotalPaginas  { get; set; }
        public int             TotalItems    { get; set; }
        public string          Buscar        { get; set; } = "";
        public string          TipoFiltro    { get; set; } = "";
        public string          EstadoFiltro  { get; set; } = "";
    }
}
