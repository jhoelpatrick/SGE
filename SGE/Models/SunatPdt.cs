namespace SGE.Models
{
    public enum TipoPdt  { PLAME, PDT601, AFPNet }
    public enum EstadoPdt { Pendiente, Enviada, Aceptada, Observada, Rechazada }

    public class DeclaracionSunat
    {
        public int       Id               { get; set; }
        public string    Codigo           { get; set; } = "";
        public TipoPdt   Tipo             { get; set; }
        public string    Periodo          { get; set; } = "";
        public int       Ejercicio        { get; set; }
        public DateTime  FechaGeneracion  { get; set; }
        public DateTime? FechaEnvio       { get; set; }
        public EstadoPdt Estado           { get; set; }
        public string    NroOrden         { get; set; } = "";
        public bool      TieneConstancia  { get; set; }
        public string    Usuario          { get; set; } = "Admin";
        public string    Observacion      { get; set; } = "";
    }

    public class SunatPdtViewModel
    {
        public List<DeclaracionSunat> Declaraciones   { get; set; } = new();
        public int    PaginaActual                    { get; set; }
        public int    TotalPaginas                    { get; set; }
        public int    TotalItems                      { get; set; }
        public string Buscar                          { get; set; } = "";
        public string TipoFiltro                      { get; set; } = "";
        public string PeriodoFiltro                   { get; set; } = "";
        public string EstadoFiltro                    { get; set; } = "";
        public string EjercicioFiltro                 { get; set; } = "";
    }
}
