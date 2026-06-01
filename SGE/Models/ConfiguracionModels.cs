namespace SGE.Models
{
    // ── Parámetros Generales ───────────────────────────────────────
    public class ParametrosGenerales
    {
        public string Empresa              { get; set; } = "Mi Empresa S.A.C.";
        public string Moneda               { get; set; } = "Soles (S/)";
        public int    DiaCierrePlanilla    { get; set; } = 20;
        public int    DiaPagoPlanilla      { get; set; } = 31;
        public bool   CalcHorasExtrasAuto  { get; set; } = true;
        public bool   InclFeriadosAsist    { get; set; } = true;
    }

    // ── Rango de Renta ─────────────────────────────────────────────
    public class RangoRenta
    {
        public int      Id         { get; set; }
        public decimal  Desde      { get; set; }
        public decimal? Hasta      { get; set; }   // null = "en adelante"
        public decimal  Tasa       { get; set; }   // porcentaje
        public decimal  MontoFijo  { get; set; }
        public bool     Activo     { get; set; } = true;
    }

    // ── Banco ──────────────────────────────────────────────────────
    public class BancoConfig
    {
        public int    Id               { get; set; }
        public string Nombre           { get; set; } = "";
        public string Codigo           { get; set; } = "";
        public string Moneda           { get; set; } = "Soles (S/)";
        public string CuentaPrincipal  { get; set; } = "";
        public bool   Activo           { get; set; } = true;
        public string Emoji            { get; set; } = "🏦";
    }

    // ── Feriado ────────────────────────────────────────────────────
    public class Feriado
    {
        public int      Id           { get; set; }
        public DateTime Fecha        { get; set; }
        public string   Nombre       { get; set; } = "";
        public string   Tipo         { get; set; } = "Nacional"; // Nacional | Personalizado
        public bool     Recuperable  { get; set; } = false;
        public bool     Activo       { get; set; } = true;
    }

    // ── Centro de Costo ────────────────────────────────────────────
    public class CentroCosto
    {
        public int    Id           { get; set; }
        public string Codigo       { get; set; } = "";
        public string Nombre       { get; set; } = "";
        public string Descripcion  { get; set; } = "";
        public string Responsable  { get; set; } = "";
        public bool   Activo       { get; set; } = true;
    }

    // ── Usuario Nómina ─────────────────────────────────────────────
    public class UsuarioNomina
    {
        public int    Id       { get; set; }
        public string Usuario  { get; set; } = "";
        public string Nombre   { get; set; } = "";
        public string Rol      { get; set; } = "";
        public string Email    { get; set; } = "";
        public bool   Activo   { get; set; } = true;
        public string Emoji    { get; set; } = "👤";
    }

    // ── Reporte ────────────────────────────────────────────────────
    public class Reporte
    {
        public int      Id              { get; set; }
        public string   Codigo          { get; set; } = "";
        public string   Nombre          { get; set; } = "";
        public string   Submodulo       { get; set; } = "";   // Planillas, Descuentos, Beneficios, etc.
        public string   Periodo         { get; set; } = "";
        public DateTime FechaGeneracion { get; set; }
        public string   GeneradoPor     { get; set; } = "Administrador";
        public string   Estado          { get; set; } = "Completado"; // Completado | En Proceso | Error
        public string   Formato         { get; set; } = "PDF";        // PDF | Excel | CSV
        public int      FilasGeneradas  { get; set; }
        public long     TamañoKb        { get; set; }
    }

    public class ReportesViewModel
    {
        public List<Reporte> Reportes        { get; set; } = new();
        public int           PaginaActual    { get; set; } = 1;
        public int           TotalPaginas    { get; set; } = 1;
        public int           TotalItems      { get; set; }
        public string        BuscarFiltro    { get; set; } = "";
        public string        SubmoduloFiltro { get; set; } = "";
        public string        EstadoFiltro    { get; set; } = "";
        public string        PeriodoFiltro   { get; set; } = "";
        public string        FormatoFiltro   { get; set; } = "";
    }
}
