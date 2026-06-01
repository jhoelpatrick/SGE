namespace SGE.Models
{
    public enum CategoriaBeneficio { Alimentacion, Transporte, Salud, Educacion, Otros }
    public enum TipoBeneficio     { Beneficio, Bonificacion, Subsidio }
    public enum Periodicidad      { Diario, Mensual, Trimestral, Anual, Unico, Variable }

    public class Beneficio
    {
        public int               Id           { get; set; }
        public string            Codigo       { get; set; } = "";
        public string            Nombre       { get; set; } = "";
        public CategoriaBeneficio Categoria   { get; set; }
        public TipoBeneficio     Tipo         { get; set; }
        public Periodicidad      Periodicidad { get; set; }
        public string            MontoCadena  { get; set; } = ""; // "S/ 250.00" o "Según Plan"
        public decimal?          MontoFijo    { get; set; }       // null = monto variable/texto
        public bool              Activo       { get; set; } = true;
        public DateTime          FechaCreacion{ get; set; } = DateTime.Today;
    }
}
