namespace SGE.Models
{
    public enum EstadoUtilidad { Pendiente, EnCalculo, Aprobada, Pagada, Anulada }

    public class Utilidad
    {
        public int      Id                     { get; set; }
        public string   Codigo                 { get; set; } = "";
        public int      EjercicioFiscal        { get; set; }   // año: 2023, 2024, 2025
        public decimal  PorcentajeParticipacion { get; set; }  // 8, 10, etc.
        public decimal  UtilidadNetaDeclarada  { get; set; }   // monto S/
        public int      DiasComputables        { get; set; }   // 360
        public decimal  RemuneracionComputable { get; set; }   // total masa salarial
        public decimal? MontoDistribuido       { get; set; }   // null si aún no calculado
        public DateTime FechaPagoEstimada      { get; set; }
        public DateTime? FechaPagoReal         { get; set; }
        public EstadoUtilidad Estado           { get; set; }
        public string   EmpleadosAplica        { get; set; } = "Todos";
        public int      CantidadEmpleados      { get; set; }
        public string   Observacion            { get; set; } = "";
        public DateTime FechaCreacion          { get; set; } = DateTime.Today;
    }

    public class UtilidadesViewModel
    {
        public List<Utilidad> Utilidades    { get; set; } = new();
        public int  PaginaActual            { get; set; }
        public int  TotalPaginas            { get; set; }
        public int  TotalItems              { get; set; }
        public string Buscar                { get; set; } = "";
        public string EstadoFiltro          { get; set; } = "";
        public string AnioFiltro            { get; set; } = "";
    }
}
