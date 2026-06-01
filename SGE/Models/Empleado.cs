namespace SGE.Models
{
    // ── Enums de Empleado ──────────────────────────────────────────
    public enum TipoContrato { Indeterminado, PlazoFijo, ServiciosEspecificos, Practicante }
    public enum RegimeLaboralT { Regimen728, Regimen276, Mype, CAS }
    public enum TipoAFP { AFP_Integra, AFP_Habitat, AFP_Prima, AFP_Profuturo, ONP }
    public enum EstadoEmpleado { Activo, Inactivo, Vacaciones, Suspendido }
    public enum TipoDocumento { DNI, CE, Pasaporte }

    public class Empleado
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";       // EMP-001

        // ── Datos personales ───────────────────────────────────────
        public string Nombres { get; set; } = "";
        public string ApellidoPaterno { get; set; } = "";
        public string ApellidoMaterno { get; set; } = "";
        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.DNI;
        public string NumeroDocumento { get; set; } = "";       // DNI 8 dígitos
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = "M";      // M | F
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string Direccion { get; set; } = "";

        // ── Datos laborales ────────────────────────────────────────
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaCese { get; set; }             // null = activo
        public string Cargo { get; set; } = "";
        public string Departamento { get; set; } = "";
        public int CentroCostoId { get; set; }
        public TipoContrato TipoContrato { get; set; } = TipoContrato.Indeterminado;
        public RegimeLaboralT RegimeLaboral { get; set; } = RegimeLaboralT.Regimen728;
        public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

        // ── Datos remunerativos ────────────────────────────────────
        public decimal SueldoBase { get; set; }
        public decimal AsignacionFamiliar { get; set; } = 0m;       // S/ 102.50 si tiene hijos
        public bool TieneHijos { get; set; } = false;

        // ── Previsión social ───────────────────────────────────────
        public TipoAFP SistemaPrevisional { get; set; } = TipoAFP.ONP;
        public string? CodigoAFP { get; set; }             // CUSPP para AFP
        public string? CUSPP { get; set; }             // Código AFP

        // ── Datos bancarios ────────────────────────────────────────
        public MedioPago BancoPago { get; set; } = MedioPago.BCP;
        public string NumeroCuenta { get; set; } = "";
        public string TipoCuenta { get; set; } = "Ahorros";// Ahorros | Corriente
        public string CCI { get; set; } = "";       // Código de cuenta interbancario

        // ── SUNAT ─────────────────────────────────────────────────
        public bool AfectoRenta5ta { get; set; } = true;
        public bool AfectoEssalud { get; set; } = true;

        // ── Propiedades calculadas ─────────────────────────────────
        public string NombreCompleto =>
            $"{ApellidoPaterno} {ApellidoMaterno}, {Nombres}".Trim();

        public int AniosServicio =>
            FechaCese.HasValue
                ? (int)((FechaCese.Value - FechaIngreso).TotalDays / 365.25)
                : (int)((DateTime.Today - FechaIngreso).TotalDays / 365.25);

        public decimal RemuneracionComputable =>
            SueldoBase + (TieneHijos ? AsignacionFamiliar : 0m);
    }
}