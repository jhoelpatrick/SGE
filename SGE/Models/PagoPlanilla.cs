namespace SGE.Models
{
    public enum EstadoPago { Pagado, Pendiente, Anulado, EnProceso }
    public enum MedioPago  { BCP, BBVA, Interbank, Scotiabank, Efectivo, Transferencia }

    public class PagoPlanilla
    {
        public int         Id             { get; set; }
        public string      Codigo         { get; set; } = "";
        public string      PlanillaConcepto{ get; set; } = "";
        public string      Periodo        { get; set; } = "";
        public DateTime    FechaPago      { get; set; }
        public MedioPago   Banco          { get; set; }
        public decimal     MontoPagado    { get; set; }
        public EstadoPago  Estado         { get; set; }
        public string      Observacion    { get; set; } = "";
        public int         Empleados      { get; set; }
    }
}
