namespace SGE.Models
{
    public class Planilla
    {
        public string Codigo { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public int Empleados { get; set; }
        public decimal TotalBruto { get; set; }
        public decimal Descuentos { get; set; }
        public decimal TotalNeto { get; set; }
        public string Estado { get; set; } = string.Empty;

        // Propiedades adicionales usadas en Planillas.cshtml
        public DateTime FechaCierre { get; set; }
        public decimal TotalDescuentos { get; set; }
        // Navegación hacia el detalle por empleado (se llena desde el controlador)
        public List<DetallePlanilla> Detalles { get; set; } = new();
    }
   
    }