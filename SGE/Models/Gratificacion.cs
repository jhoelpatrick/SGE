namespace SGE.Models
{
    public enum TipoGratificacion { Obligatoria, Voluntaria }
    public enum FrecuenciaGratificacion { Mensual, Semestral, Anual, Unica, Variable }
    public enum BaseCalculo { RemuneracionBasica, RemuneracionComputable, SalarioNeto, Fijo, PorcentajeVariable }
    public enum EstadoGratificacion { Activa, Pendiente, Programada, Pagada, Borrador }

    public class Gratificacion
    {
        public int    Id           { get; set; }
        public string Codigo       { get; set; } = "";
        public string Nombre       { get; set; } = "";
        public TipoGratificacion   Tipo        { get; set; }
        public string              Periodo     { get; set; } = "";
        public FrecuenciaGratificacion Frecuencia { get; set; }
        public string              PorcentajeMonto { get; set; } = ""; // "50% salario", "S/ 1,000.00", etc.
        public decimal?            MontoFijo   { get; set; }
        public decimal?            Porcentaje  { get; set; }
        public BaseCalculo         BaseDeCalculo { get; set; }
        public DateTime?           FechaEstimada { get; set; }
        public DateTime?           FechaPago   { get; set; }
        public EstadoGratificacion Estado      { get; set; }
        public string              EmpleadosAplica { get; set; } = "Todos";
        public int                 CantidadEmpleados { get; set; }
        public string              CreadoPor   { get; set; } = "Admin";
        public DateTime            FechaCreacion { get; set; } = DateTime.Today;
    }

    public class GratificacionesViewModel
    {
        public List<Gratificacion> Gratificaciones { get; set; } = new();
        public int  PaginaActual  { get; set; }
        public int  TotalPaginas  { get; set; }
        public int  TotalItems    { get; set; }
        public string Buscar      { get; set; } = "";
        public string TipoFiltro  { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
    }
}
