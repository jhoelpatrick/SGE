namespace SGE.Models
{
    public class DetallePlanilla
    {
        public int Id { get; set; }
        public string CodigoPlanilla { get; set; } = "";
        public int EmpleadoId { get; set; }
        public string Periodo { get; set; } = "";

        public decimal SueldoBase { get; set; }
        public decimal AsignacionFamiliar { get; set; }
        public decimal HorasExtras { get; set; }
        public decimal Movilidad { get; set; }
        public decimal Refrigerio { get; set; }
        public decimal BonificacionDesempenio { get; set; }
        public decimal OtrosIngresos { get; set; }
        public decimal TotalBruto { get; set; }

        public decimal DescuentoAFP_ONP { get; set; }
        public decimal ComisionAFP { get; set; }
        public decimal SeguroAFP { get; set; }
        public decimal EssaludTrabajador { get; set; }
        public decimal Renta5taCategoria { get; set; }
        public decimal SCTR { get; set; }

        public decimal Prestamos { get; set; }
        public decimal Adelantos { get; set; }
        public decimal TardanzasFaltas { get; set; }
        public decimal OtrosDescuentos { get; set; }
        public decimal TotalDescuentos { get; set; }

        public decimal EssaludEmpleador { get; set; }
        public decimal SCTREmpleador { get; set; }

        public decimal TotalNeto { get; set; }

        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaCalculo { get; set; } = DateTime.Now;
        public string CalculadoPor { get; set; } = "Sistema";

        public string NombreEmpleado { get; set; } = "";
        public string DNIEmpleado { get; set; } = "";
        public string CargoEmpleado { get; set; } = "";
        public string SistemaPrevisional { get; set; } = "";
        public string BancoPago { get; set; } = "";
        public string NumeroCuenta { get; set; } = "";
    }

    public class DetallePlanillaViewModel
    {
        public Planilla Planilla { get; set; } = new();
        public List<DetallePlanilla> Detalles { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItems { get; set; }
        public string BuscarFiltro { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";

        public decimal TotalBrutoGeneral => Detalles.Sum(d => d.TotalBruto);
        public decimal TotalDescuentosGeneral => Detalles.Sum(d => d.TotalDescuentos);
        public decimal TotalNetoGeneral => Detalles.Sum(d => d.TotalNeto);
        public decimal TotalEssaludEmpresa => Detalles.Sum(d => d.EssaludEmpleador);
    }
}