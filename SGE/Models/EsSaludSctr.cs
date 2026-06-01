namespace SGE.Models
{
    // ─── Enumeraciones ────────────────────────────────────────────
    public enum EstadoDeclaracion { Pendiente, Enviada, Aceptada, Observada, Rechazada }
    public enum TipoDeclaracion { Mensual, Rectificatoria, Anual }
    public enum NivelRiesgoSCTR { Riesgo1, Riesgo2, Riesgo3, Riesgo4 }
    public enum EstadoEnvio { Enviado, PendienteEnvio, ConObservaciones, Aceptado }

    // ─── Declaración EsSalud ──────────────────────────────────────
    public class DeclaracionEsSalud
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Periodo { get; set; } = "";   // "Abril 2025"
        public int Trabajadores { get; set; }
        public decimal RemuneracionAsignable { get; set; }
        public decimal AporteEsSalud { get; set; }         // 9%
        public decimal Subsidios { get; set; }
        public decimal TotalPagar { get; set; }
        public DateTime FechaEnvio { get; set; }
        public EstadoDeclaracion Estado { get; set; }
        public TipoDeclaracion TipoDeclaracion { get; set; }
        public string NroOrdenSunat { get; set; } = "";
        public string? Observacion { get; set; }
    }

    // ─── Validación de declaración ────────────────────────────────
    public class ValidacionEsSalud
    {
        public string Nombre { get; set; } = "";
        public string Periodo { get; set; } = "";
        public bool Valido { get; set; }
        public string Severidad { get; set; } = "Ok"; // Ok | Advertencia | Error
        public string Detalle { get; set; } = "";
        public string DetalleLargo { get; set; } = "";   // descripción extendida para modal
        public string AfectadosJson { get; set; } = "[]"; // JSON: [{Nombre, Dato}]
    }

    // ─── Grupo SCTR ───────────────────────────────────────────────
    public class GrupoSctr
    {
        public int Id { get; set; }
        public NivelRiesgoSCTR NivelRiesgo { get; set; }
        public int Trabajadores { get; set; }
        public decimal SctrSalud { get; set; }
        public decimal SctrPension { get; set; }
        public string Aseguradora { get; set; } = "RIMAC Seguros";
        public bool Activo { get; set; } = true;
    }

    // ─── Historial de envíos ──────────────────────────────────────
    public class HistorialEnvioEsSalud
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Declaracion { get; set; } = "";
        public string Usuario { get; set; } = "";
        public EstadoEnvio Estado { get; set; }
        public string? Mensaje { get; set; }
    }

    // ─── ViewModels ───────────────────────────────────────────────
    public class EsSaludViewModel
    {
        // Sub-vista activa: Resumen | Declaraciones | Aportes | Sctr | Validaciones | Historial
        public string Vista { get; set; } = "Resumen";

        // Datos generales
        public List<DeclaracionEsSalud> Declaraciones { get; set; } = new();
        public List<GrupoSctr> GruposSctr { get; set; } = new();
        public List<ValidacionEsSalud> Validaciones { get; set; } = new();
        public List<HistorialEnvioEsSalud> Historial { get; set; } = new();

        // KPIs resumen
        public int TotalDeclaraciones { get; set; }
        public int Pendientes { get; set; }
        public int Enviadas { get; set; }
        public int Aceptadas { get; set; }
        public int Observadas { get; set; }
        public decimal AporteTotalPeriodo { get; set; }

        // SCTR resumen
        public decimal SctrSaludTotal { get; set; }
        public decimal SctrPensionTotal { get; set; }
        public decimal TotalSctr => SctrSaludTotal + SctrPensionTotal;

        // Empleados para tab Expuestos
        public List<Empleado> Empleados { get; set; } = new();

        // Filtros
        public string PeriodoFiltro { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
        public string TipoFiltro { get; set; } = "";
        public string Buscar { get; set; } = "";
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
    }
}