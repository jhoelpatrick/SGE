namespace SGE.Models
{
    public class Descuento
    {
        public int     Id         { get; set; }
        public string  Codigo     { get; set; } = string.Empty;
        public string  Nombre     { get; set; } = string.Empty;
        public string  Tipo       { get; set; } = string.Empty;   // "Obligatorio" | "Voluntario"
        public bool    Obligatorio { get; set; }
        public bool    AfectaNeto  { get; set; }
        public decimal Porcentaje  { get; set; }
        public bool    Activo      { get; set; }
    }
}
