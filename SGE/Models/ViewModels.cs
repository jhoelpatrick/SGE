using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGE.Models
{
    public enum EstadoUsuario
    {
        Activo,
        Inactivo,
        Bloqueado
    }

    public class Permiso
    {
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        // Permission matrix fields (used by Gestion views)
        public string Modulo { get; set; } = "";
        public bool Ver { get; set; }
        public bool CrearEditar { get; set; }
        public bool Eliminar { get; set; }
        public bool Reportes { get; set; }
    }

    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
        public string Iniciales => $"{(Nombre.Length > 0 ? Nombre[0].ToString() : "")}{(Apellido.Length > 0 ? Apellido[0].ToString() : "")}".ToUpper();
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Contrasena { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int RolId { get; set; }
        // Rol as string for view compatibility (switch statements, display, Length checks)
        public string Rol { get; set; } = "";
        // Estado as enum — views compare u.Estado == EstadoUsuario.Activo
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
        public string RolNombre { get; set; } = "";
        public bool MfaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime UltimoAcceso { get; set; }
    }

    public class RolPermisosViewModel
    {
        public string Error { get; set; } = "";
        public List<string> Roles { get; set; } = new List<string>();
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
        public string RolSeleccionado { get; set; } = "Administrador";
    }

    public class KPI
    {
        public string Titulo { get; set; } = "";
        public string Icono { get; set; } = "";
        public string Valor { get; set; } = "";
        public string Detalle { get; set; } = "";
    }

    public class Impuesto
    {
        public int ImpuestoId { get; set; }
        public string CodigoImpuestoSunat { get; set; } = "";
        public string NombreImpuesto { get; set; } = "";
        public decimal Porcentaje { get; set; }
        public bool Estado { get; set; }
    }

    public class ImpuestosViewModel
    {
        public List<KPI> Kpis { get; set; } = new List<KPI>();
        public decimal IgvNeto { get; set; }
        public decimal MontoEstimadoSunat { get; set; }
        public decimal DebitoFiscal { get; set; }
        public decimal CreditoFiscal { get; set; }
        public decimal Retenciones { get; set; }
        public decimal Percepciones { get; set; }
        public List<Impuesto> Impuestos { get; set; } = new List<Impuesto>();
    }

    public class CuentaBancaria
    {
        public int CuentaBancariaId { get; set; }
        public string BancoNombre { get; set; } = "";
        public string NumeroCuenta { get; set; } = "";
        public string CuentaCciExterno { get; set; } = "";
        public string TipoCuenta { get; set; } = "";
        public string Moneda { get; set; } = "";
        public decimal SaldoActual { get; set; }
        public bool Estado { get; set; }
    }

    public class MovimientoTesoreria
    {
        public int MovimientoTesoreriaId { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public int CuentaBancariaId { get; set; }
        public string TipoFlujo { get; set; } = "";
        public string MedioPagoSunat { get; set; } = "";
        public int? ComprobanteId { get; set; }
        public int? OrdenId { get; set; }
        public string GlosaMovimiento { get; set; } = "";
        public decimal Monto { get; set; }
    }

    public class CajaBancosViewModel
    {
        public List<KPI> Kpis { get; set; } = new List<KPI>();
        public List<CuentaBancaria> Cuentas { get; set; } = new List<CuentaBancaria>();
        public List<MovimientoTesoreria> Movimientos { get; set; } = new List<MovimientoTesoreria>();
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
    }

    public class LibroDiarioItem
    {
        public DateTime FechaAsiento { get; set; }
        public string NumeroAsiento { get; set; } = "";
        public string TipoLibroSunat { get; set; } = "";
        public string CuentaCodigo { get; set; } = "";
        public string NombreCuenta { get; set; } = "";
        public string Glosa { get; set; } = "";
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    public class AsientoCabecera
    {
        public int AsientoId { get; set; }
        public string NumeroAsiento { get; set; } = "";
        public DateTime FechaAsiento { get; set; }
        public string TipoLibroSunat { get; set; } = "";
        public string Glosa { get; set; } = "";
        public string DocumentoReferencia { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
    }

    public class AsientoDetalle
    {
        public int AsientoDetalleId { get; set; }
        public int AsientoId { get; set; }
        public string CuentaCodigo { get; set; } = "";
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    public class PlanCuentasItem
    {
        public string CuentaCodigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string TipoCuenta { get; set; } = "";
        public int NivelInt { get; set; }
        public bool AceptaAsiento { get; set; }
    }

    public class ContabilidadFinanzasViewModel
    {
        public List<KPI> Kpis { get; set; } = new List<KPI>();
        public List<LibroDiarioItem> LibroDiario { get; set; } = new List<LibroDiarioItem>();
        public List<AsientoCabecera> Asientos { get; set; } = new List<AsientoCabecera>();
        public List<AsientoDetalle> Detalles { get; set; } = new List<AsientoDetalle>();
        public List<PlanCuentasItem> PlanCuentas { get; set; } = new List<PlanCuentasItem>();
    }

    public class ActivoFijo
    {
        public int ActivoId { get; set; }
        public string CodigoActivo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int? ProductoId { get; set; }
        public DateTime FechaAdquisicion { get; set; }
        public decimal ValorInicial { get; set; }
        public decimal TasaDepreciacionAnual { get; set; }
        public decimal DepreciacionAcumulada { get; set; }
        public decimal ValorNetoLibros { get; set; }
        public string Estado { get; set; } = "";
    }

    public class ActivosFijosViewModel
    {
        public List<KPI> Kpis { get; set; } = new List<KPI>();
        public List<ActivoFijo> Activos { get; set; } = new List<ActivoFijo>();
        public decimal DepreciacionTotal { get; set; }
        public decimal ValorNetoTotal { get; set; }
    }

    // RRHH ViewModels
    public enum EstadoEmpleado { Activo, Vacaciones, Suspendido, Inactivo }
    public enum TipoAFP { ONP, AFP_Prima, AFP_Integra, AFP_Habitat, AFP_ProFuturo }
    public enum TipoContrato { Indefinido, Plazo_Fijo, Practicante, Por_Obra, Temporal }
    public enum EstadoDeclaracion { Pendiente, Enviada, Aceptada, Observada, Rechazada }
    public enum NivelRiesgoSCTR { Riesgo1, Riesgo2, Riesgo3, Riesgo4 }
    public enum EstadoEnvio { PendienteEnvio, Enviado, Aceptado, ConObservaciones }

    public class EsSaludViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public int Anio { get; set; } = DateTime.Now.Year;
        public int Mes { get; set; } = DateTime.Now.Month;
        public List<dynamic> Declaraciones { get; set; } = new List<dynamic>();
        // View navigation / filter properties used by EsSalud.cshtml
        public string Vista { get; set; } = "Resumen";
        public List<dynamic> Empleados { get; set; } = new List<dynamic>();
        public int TotalDeclaraciones { get; set; } = 0;
        public int Pendientes { get; set; } = 0;
        public int Enviadas { get; set; } = 0;
        public int Aceptadas { get; set; } = 0;
        public decimal AporteTotalPeriodo { get; set; } = 0m;
        public List<dynamic> Validaciones { get; set; } = new List<dynamic>();
        public decimal SctrSaludTotal { get; set; } = 0m;
        public decimal SctrPensionTotal { get; set; } = 0m;
        public decimal TotalSctr { get; set; } = 0m;
        public List<dynamic> GruposSctr { get; set; } = new List<dynamic>();
        public List<dynamic> Historial { get; set; } = new List<dynamic>();
    }

    public class NominaViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Empleados { get; set; } = new List<dynamic>();
        public List<dynamic> Conceptos { get; set; } = new List<dynamic>();

        // Properties used in Nomina/Index.cshtml
        public decimal TotalPlanillaMesActual { get; set; } = 0m;
        public decimal PorcentajeCambio { get; set; } = 0m;
        public int EmpleadosEnPlanilla { get; set; } = 0;
        public int EmpleadosNuevosMes { get; set; } = 0;
        public DateTime ProximoPago { get; set; } = DateTime.Now.AddDays(10);
        public int DiasParaProximoPago { get; set; } = 10;
        public decimal DescuentosTotales { get; set; } = 0m;
        public decimal PorcentajeDescuentos { get; set; } = 0m;
        public int TotalEmpleados { get; set; } = 0;
        public int EmpleadosActivos { get; set; } = 0;
        public int EmpleadosEnVacaciones { get; set; } = 0;
        public decimal MasaSalarial { get; set; } = 0m;
        public List<dynamic> EmpleadosPreview { get; set; } = new List<dynamic>();
        public int TotalPlanillas { get; set; } = 0;
        public int PlanillasPagadas { get; set; } = 0;
        public int PlanillasEnProceso { get; set; } = 0;
        public int PlanillasPendientes { get; set; } = 0;
        public int PlanillasAnuladas { get; set; } = 0;
        public List<dynamic> UltimasPlanillas { get; set; } = new List<dynamic>();
    }

    public class BasePagedViewModel
    {
        public string Buscar { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
        public string TipoFiltro { get; set; } = "";
        public string PeriodoFiltro { get; set; } = "";
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int DesdeItem { get; set; } = 0;
        public int HastaItem { get; set; } = 0;
        public bool TienePrev => PaginaActual > 1;
        public bool TieneNext => PaginaActual < TotalPaginas;
    }

    public class ReportesViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public string BuscarFiltro { get; set; } = "";
        public string SubmoduloFiltro { get; set; } = "";
        public string FormatoFiltro { get; set; } = "";
        public List<dynamic> Reportes { get; set; } = new List<dynamic>();
    }

    public enum EstadoUtilidad { Pendiente, EnCalculo, Aprobada, Pagada }
    public class UtilidadesViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public string AnioFiltro { get; set; } = "";
        public List<dynamic> Utilidades { get; set; } = new List<dynamic>();
    }

    public enum TipoPdt { PLAME, T_REGISTRO, AFP_NET, PDT601 }
    public enum EstadoPdt { Pendiente, Enviada, Aceptada, Observada }
    public class SunatPdtViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public string EjercicioFiltro { get; set; } = "";
        public List<dynamic> Declaraciones { get; set; } = new List<dynamic>();
    }

    public class PlanillasViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Planillas { get; set; } = new List<dynamic>();
    }

    public enum EstadoPago { Pendiente, EnProceso, Pagado, Fallido }
    public class HistorialPagosViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public string MedioFiltro { get; set; } = "";
        public List<dynamic> Pagos { get; set; } = new List<dynamic>();
    }

    public enum TipoGratificacion { Obligatoria, Voluntaria }
    public enum FrecuenciaGratificacion { Mensual, Semestral, Anual, Unica, Variable }
    public enum EstadoGratificacion { Activa, Pendiente, Programada, Pagada, Borrador }
    public enum BaseCalculo { RemuneracionBasica, RemuneracionComputable, SalarioNeto, PorcentajeVariable, Fija }

    public class GratificacionItem
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public TipoGratificacion Tipo { get; set; } = TipoGratificacion.Obligatoria;
        public string Periodo { get; set; } = "";
        public FrecuenciaGratificacion Frecuencia { get; set; } = FrecuenciaGratificacion.Anual;
        public string PorcentajeMonto { get; set; } = "";
        public BaseCalculo BaseDeCalculo { get; set; } = BaseCalculo.RemuneracionBasica;
        public decimal? MontoFijo { get; set; }
        public decimal? Porcentaje { get; set; }
        public DateTime? FechaEstimada { get; set; }
        public DateTime? FechaPago { get; set; }
        public EstadoGratificacion Estado { get; set; } = EstadoGratificacion.Pendiente;
        public string EmpleadosAplica { get; set; } = "";
        public int CantidadEmpleados { get; set; }
        public string CreadoPor { get; set; } = "";
    }

    public class GratificacionesViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<GratificacionItem> Gratificaciones { get; set; } = new List<GratificacionItem>();
    }

    public class EmpleadoViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Empleados { get; set; } = new List<dynamic>();
        public string BuscarFiltro { get; set; } = "";
        public string DeptFiltro { get; set; } = "";
        public int TotalActivos { get; set; }
        public int TotalVacaciones { get; set; }
        public decimal MassaSalarial { get; set; }
    }

    public class DetallePlanillaViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Items { get; set; } = new List<dynamic>();
        public dynamic Planilla { get; set; } = null!;
        public decimal TotalBrutoGeneral { get; set; }
        public decimal TotalDescuentosGeneral { get; set; }
        public decimal TotalNetoGeneral { get; set; }
        public decimal TotalEssaludEmpresa { get; set; }
        public string BuscarFiltro { get; set; } = "";
        public List<dynamic> Detalles { get; set; } = new List<dynamic>();
    }

    public class DescuentosViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Descuentos { get; set; } = new List<dynamic>();
    }

    public enum TipoConcepto { Ingreso, Descuento, Aporte, Fijo, Variable }
    public class ConceptosViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public List<dynamic> Conceptos { get; set; } = new List<dynamic>();
    }

    public class BoletaPagoViewModel
    {
        public string Error { get; set; } = "";
        public dynamic? Boleta { get; set; }
        public dynamic? Empleado { get; set; }
        public dynamic? Detalle { get; set; }
        public string EmpresaNombre { get; set; } = "";
        public string EmpresaRUC { get; set; } = "";
        public string EmpresaDireccion { get; set; } = "";
        public string Periodo { get; set; } = "";
    }

    public enum CategoriaBeneficio { Alimentacion, Transporte, Salud, Educacion, Otros }
    public enum Periodicidad { Diario, Mensual, Trimestral, Anual, Unico, Variable }
    public enum TipoBeneficio { Bonificacion, Subsidio, Beneficio }

    public class BeneficioItem
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public CategoriaBeneficio Categoria { get; set; } = CategoriaBeneficio.Otros;
        public TipoBeneficio Tipo { get; set; } = TipoBeneficio.Beneficio;
        public Periodicidad Periodicidad { get; set; } = Periodicidad.Mensual;
        public decimal? MontoFijo { get; set; }
        public string MontoCadena { get; set; } = "";
        public bool Activo { get; set; } = true;
    }

    public class BeneficiosViewModel : BasePagedViewModel
    {
        public string Error { get; set; } = "";
        public string CategoriaFiltro { get; set; } = "";
        public List<BeneficioItem> Beneficios { get; set; } = new List<BeneficioItem>();
    }

    public class HRStatsViewModel
    {
        public int ActiveEmployees { get; set; }
        public int ActiveContracts { get; set; }
        public int PendingVacations { get; set; }
        public List<dynamic> RecentEmployees { get; set; } = new List<dynamic>();
    }

    public class ParametrosGenerales
    {
        public string Empresa { get; set; } = "";
        public string Moneda { get; set; } = "Soles (S/)";
        public int DiaCierrePlanilla { get; set; } = 30;
        public int DiaPagoPlanilla { get; set; } = 30;
        public bool CalcHorasExtrasAuto { get; set; } = true;
        public bool InclFeriadosAsist { get; set; } = false;
    }
    public class RangoRenta
    {
        public int Id { get; set; }
        public decimal Desde { get; set; }
        public decimal? Hasta { get; set; }
        public decimal Tasa { get; set; }
        public decimal MontoFijo { get; set; }
        public bool Activo { get; set; } = true;
    }
    public class BancoConfig
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Codigo { get; set; } = "";
        public string Moneda { get; set; } = "";
        public string CuentaPrincipal { get; set; } = "";
        public bool Activo { get; set; } = true;
        public string Emoji { get; set; } = "🏦";
    }
    public class Feriado
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "Nacional";
        public bool Recuperable { get; set; }
        public bool Activo { get; set; } = true;
    }
    public class CentroCosto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Responsable { get; set; } = "";
        public bool Activo { get; set; } = true;
    }
    public class UsuarioNomina
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Rol { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Activo { get; set; } = true;
        public string Emoji { get; set; } = "👤";
    }

    // ── Módulo Comercial — tablas de sge_crm ──────────────────────────────────

    public class Cliente
    {
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        [MaxLength(1)]
        public string TipoDocumento { get; set; } = "6"; // '1' DNI · '6' RUC · '4' CE

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [MaxLength(15)]
        public string NumeroDocumento { get; set; } = "";

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [MaxLength(250)]
        public string RazonSocial { get; set; } = "";

        [MaxLength(250)]
        public string? NombreComercial { get; set; }

        [MaxLength(500)]
        public string? DireccionFiscal { get; set; }

        [MaxLength(6)]
        public string? Ubigeo { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(20)]
        public string TipoCliente { get; set; } = "prospecto"; // 'cliente' | 'prospecto'

        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Campos calculados devueltos por la vista comercial.vw_crm_clientes_bandeja
        public string DireccionCompletaUI { get; set; } = "";
        public string TipoDocumentoDesc { get; set; } = "";
    }

    public class Producto
    {
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El código SKU es obligatorio.")]
        [MaxLength(50)]
        public string CodigoSku { get; set; } = "";

        [MaxLength(8)]
        public string? CodigoSunat { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(250)]
        public string Descripcion { get; set; } = "";

        [Required]
        [MaxLength(3)]
        public string UnidadMedida { get; set; } = "NIU"; // Unidad SUNAT: NIU, KGM, etc.

        [MaxLength(2)]
        public string TipoAfectacionIgv { get; set; } = "10"; // '10' Gravado, '20' Exonerado

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
        public decimal PrecioVentaSugerido { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostoPromedio { get; set; }

        public bool EsServicio { get; set; }
        public bool SeVende { get; set; } = true;
        public bool NoSeVende { get; set; }
        public bool SeFabrica { get; set; }
        public bool Estado { get; set; } = true;

        // Propiedades auxiliares para compatibilidad con la UI
        public decimal StockActual { get; set; } = 0;
        public decimal StockMinimo { get; set; } = 10;
    }

    public class Proveedor
    {
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        [MaxLength(1)]
        public string TipoDocumento { get; set; } = "6";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [MaxLength(15)]
        public string NumeroDocumento { get; set; } = "";

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [MaxLength(250)]
        public string RazonSocial { get; set; } = "";

        [MaxLength(500)]
        public string? DireccionFiscal { get; set; }

        [MaxLength(6)]
        public string? Ubigeo { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        public bool Estado { get; set; } = true;
    }

    // ── Módulo de Operaciones ──────────────────────────────────────────

    public class Proyecto
    {
        public int ProyectoId { get; set; }
        public int ClienteId { get; set; }
        public string NombreProyecto { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal PresupuestoTotal { get; set; }
        public decimal CostoRealLogrado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = "planificado"; // planificado, en progreso, suspendido, terminado
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Campos auxiliares para la UI
        public string ClienteNombre { get; set; } = "";
        public string ClienteRuc { get; set; } = "";
        public decimal ProgresoPromedio { get; set; }
        public int TotalTareas { get; set; }
    }

    public class ProyectoTarea
    {
        public int TareaId { get; set; }
        public int ProyectoId { get; set; }
        public string NombreTarea { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal PorcentajeProgreso { get; set; }
        public decimal CostoEstimado { get; set; }
        public string Estado { get; set; } = "pendiente"; // pendiente, en ejecucion, completada, bloqueada
    }

    public class Almacen
    {
        public int AlmacenId { get; set; }
        public string CodigoAlmacen { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Direccion { get; set; }
        public string? Ubigeo { get; set; }
        public bool Estado { get; set; } = true;
    }

    public class StockAlmacen
    {
        public int AlmacenId { get; set; }
        public int ProductoId { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockComprometido { get; set; }
    }

    public class PedidoVenta
    {
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = "";
        public int ClienteId { get; set; }
        public int? ProyectoId { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string Moneda { get; set; } = "PEN";
        public decimal TipoCambio { get; set; } = 1.0000m;
        public string? MetodoPago { get; set; }
        public string? CuponDescuento { get; set; }
        public decimal MontoBruto { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal TotalNeto { get; set; }
        public string Estado { get; set; } = "pendiente"; // pendiente, aprobado, despachado, cancelado

        // Campos auxiliares para la UI
        public string ClienteNombre { get; set; } = "";
        public string ClienteRuc { get; set; } = "";
        public string? ProyectoNombre { get; set; }
        public List<PedidoVentaDetalle> Detalles { get; set; } = new List<PedidoVentaDetalle>();
    }

    public class PedidoVentaDetalle
    {
        public int DetalleId { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitarioConGiv { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalFila { get; set; }

        // Campos auxiliares para la UI
        public string ProductoDescripcion { get; set; } = "";
        public string ProductoSku { get; set; } = "";
        public string UnidadMedida { get; set; } = "";
    }

    public class OrdenCompra
    {
        public int OrdenId { get; set; }
        public string NumeroOrden { get; set; } = "";
        public int ProveedorId { get; set; }
        public int? ProyectoId { get; set; }
        public string? Solicitante { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string Moneda { get; set; } = "PEN";
        public decimal MontoTotal { get; set; }
        public string CategoriaGasto { get; set; } = "materiales"; // materiales, servicios, equipos, logistica
        public string Estado { get; set; } = "pendiente"; // pendiente, aprobado, bloqueado, rechazado

        // Campos auxiliares para la UI
        public string ProveedorNombre { get; set; } = "";
        public string ProveedorRuc { get; set; } = "";
        public string? ProyectoNombre { get; set; }
        public List<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();
    }

    public class OrdenCompraDetalle
    {
        public int DetalleDoc { get; set; }
        public int OrdenId { get; set; }
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitarioConGiv { get; set; }
        public decimal TotalFila { get; set; }

        // Campos auxiliares para la UI
        public string ProductoDescripcion { get; set; } = "";
        public string ProductoSku { get; set; } = "";
    }

    public class ComprobanteFacturacion
    {
        public int ComprobanteId { get; set; }
        public int? PedidoId { get; set; }
        public string TipoComprobante { get; set; } = "01"; // '01' factura, '03' boleta
        public string Serie { get; set; } = "";
        public string Correlativo { get; set; } = "";
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string TipoOperacionSunat { get; set; } = "01";
        public int ClienteId { get; set; }
        public string Moneda { get; set; } = "PEN";
        public decimal OpGravada { get; set; }
        public decimal OpInafecta { get; set; }
        public decimal OpExonerada { get; set; }
        public decimal IgvTotal { get; set; }
        public decimal ImporteTotalNeto { get; set; }
        public string TipoImpuestoEspecial { get; set; } = "ninguno";
        public string EstadoSunat { get; set; } = "enviado sunat"; // enviado sunat, contingencia, anulado

        // Campos auxiliares para la UI
        public string ClienteNombre { get; set; } = "";
        public string ClienteRuc { get; set; } = "";
        public string PedidoNumero { get; set; } = "";
    }

    public class GuiaRemision
    {
        public int GuiaId { get; set; }
        public string Serie { get; set; } = "";
        public string Correlativo { get; set; } = "";
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string MotivoTraslado { get; set; } = "01";
        public int AlmacenOrigenId { get; set; }
        public int? AlmacenDestinoId { get; set; }
        public int? ProveedorId { get; set; }
        public int? VehiculoId { get; set; }
        public int? ConductorId { get; set; }
        public decimal PesoTotal { get; set; }
        public string UnidadMedidaPeso { get; set; } = "KG";
        public string EstadoSunat { get; set; } = "aceptado";

        // Campos auxiliares para la UI
        public string VehiculoPlaca { get; set; } = "";
        public string VehiculoMarca { get; set; } = "";
        public string ConductorNombre { get; set; } = "";
        public string ConductorDni { get; set; } = "";
        public string MotivoTrasladoDesc { get; set; } = "";
    }

    public class KardexMovimiento
    {
        public long MovimientoId { get; set; }
        public int AlmacenId { get; set; }
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } = ""; // 'ent' (entrada), 'sal' (salida)
        public string ConceptoMovimiento { get; set; } = "";
        public string? DocumentoReferencia { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitarioMovimiento { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        // Campos auxiliares para la UI
        public string ProductoSku { get; set; } = "";
        public string ProductoDescripcion { get; set; } = "";
        public decimal SaldoPosterior { get; set; }
    }

    public class Vehiculo
    {
        public int VehiculoId { get; set; }
        public int ProveedorId { get; set; }
        public string Placa { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string TipoVehiculo { get; set; } = "";
        public string ProveedorNombre { get; set; } = "";
        public bool Estado { get; set; } = true;
    }

    public class Conductor
    {
        public int ConductorId { get; set; }
        public int ProveedorId { get; set; }
        public string Nombre { get; set; } = "";
        public string NumeroDocumento { get; set; } = "";
        public string LicenciaConducir { get; set; } = "";
        public bool Estado { get; set; } = true;
    }
}

namespace Reportes.Models
{
    public class ReportesIndexViewModel
    {
        public string Error { get; set; } = "";
        public int TotalReportes { get; set; }
        public int ReportesActivos { get; set; }
        public int TotalRegistrosAnalizados { get; set; }
        public int TotalModulos { get; set; }
        public List<string> Modulos { get; set; } = new List<string>();
        public string ModuloFiltro { get; set; } = "";
        public string EstadoFiltro { get; set; } = "";
        public List<string> ChartModuloLabels { get; set; } = new List<string>();
        public List<int> ChartModuloValues { get; set; } = new List<int>();
        public List<string> ChartTopLabels { get; set; } = new List<string>();
        public List<int> ChartTopValues { get; set; } = new List<int>();
        public List<dynamic> Reportes { get; set; } = new List<dynamic>();
    }

    public class ParametrosIndexViewModel
    {
        public string Error { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public int TotalParametros { get; set; }
        public int TotalCategorias { get; set; }
        public int ActualizadosHoy { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public List<dynamic> Recientes { get; set; } = new List<dynamic>();
        public List<dynamic> ParametrosPorCategoria { get; set; } = new List<dynamic>();
        public List<string> ChartCategoriaLabels { get; set; } = new List<string>();
        public List<int> ChartCategoriaValues { get; set; } = new List<int>();
        public List<dynamic> Parametros { get; set; } = new List<dynamic>();
    }

    public class RegistroAuditoria
    {
        public int LogId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Usuario { get; set; } = "";
        public string TablaAfectada { get; set; } = "";
        public string Accion { get; set; } = "";
        public string IdRegistroAfectado { get; set; } = "";
        public string ValorAnterior { get; set; } = "";
        public string ValorNuevo { get; set; } = "";
    }

    public class AlertaAuditoria
    {
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Nivel { get; set; } = "";
    }

    public class AuditoriaViewModel
    {
        public string Error { get; set; } = "";
        public int TotalEventos { get; set; }
        public int UsuariosUnicos { get; set; }
        public int CambiosCriticos { get; set; }
        public int EventosHoy { get; set; }
        public string NivelRiesgo { get; set; } = "";
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string UsuarioFiltro { get; set; } = "";
        public string TablaAfectada { get; set; } = "";
        public string Accion { get; set; } = "";
        public List<string> Usuarios { get; set; } = new List<string>();
        public List<string> Tablas { get; set; } = new List<string>();
        public List<RegistroAuditoria> Recientes { get; set; } = new List<RegistroAuditoria>();
        public List<AlertaAuditoria> Alertas { get; set; } = new List<AlertaAuditoria>();
        public List<RegistroAuditoria> Registros { get; set; } = new List<RegistroAuditoria>();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public List<string> ChartAccionLabels { get; set; } = new List<string>();
        public List<int> ChartAccionValues { get; set; } = new List<int>();
    }
}
