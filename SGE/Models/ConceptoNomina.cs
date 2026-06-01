namespace SGE.Models
{
    public enum TipoConcepto { Fijo, Variable }

    public class ConceptoNomina
    {
        public int    Id              { get; set; }
        public string Codigo          { get; set; } = "";
        public string Nombre          { get; set; } = "";
        public TipoConcepto Tipo      { get; set; } = TipoConcepto.Fijo;
        public bool   AfectaCalculo   { get; set; } = true;
        public bool   EsRemunerativo  { get; set; } = true;
        public bool   Activo          { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Today;
    }

    public class ConceptosViewModel
    {
        public List<ConceptoNomina> Conceptos   { get; set; } = new();
        public int  PaginaActual   { get; set; } = 1;
        public int  TotalPaginas   { get; set; } = 1;
        public int  TotalItems     { get; set; }
        public string Buscar       { get; set; } = "";
        public string TipoFiltro   { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
    }
}
