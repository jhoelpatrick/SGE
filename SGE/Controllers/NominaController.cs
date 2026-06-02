using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Data.SqlClient;

namespace SGE.Controllers
{
    public class NominaController : Controller
    {
        private readonly string _conn;

        public NominaController(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public IActionResult Index()
        {
            var vm = new NominaViewModel();
            try
            {
                using var cn = new SqlConnection(_conn);
                cn.Open();
                
                // Get counts
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = 1", cn))
                {
                    vm.EmpleadosActivos = (int)cmd.ExecuteScalar();
                }
                
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados", cn))
                {
                    vm.TotalEmpleados = (int)cmd.ExecuteScalar();
                }

                vm.EmpleadosEnPlanilla = vm.EmpleadosActivos;

                // Masa salarial
                using (var cmd = new SqlCommand("SELECT SUM(sueldobase) FROM rrhh_recursos.contratos WHERE estaactivo = 1", cn))
                {
                    var val = cmd.ExecuteScalar();
                    vm.MasaSalarial = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                    vm.TotalPlanillaMesActual = vm.MasaSalarial;
                }

                // Recent employees preview — fetch all fields required by Index.cshtml
                using (var cmd = new SqlCommand(
                    @"SELECT TOP 10
                        e.empleadoid,
                        e.nombres,
                        e.apellidopaterno,
                        e.apellidomaterno,
                        e.numerodocumento,
                        ISNULL(e.cargo, 'Sin cargo') AS cargo,
                        ISNULL(c.sueldobase, 0) AS sueldobase,
                        ISNULL(c.tipocontrato, 'Indefinido') AS tipocontrato,
                        ISNULL(c.sistemaprevisional, 'ONP') AS sistemaprevisional,
                        CASE WHEN e.estaactivo = 1 THEN 0 ELSE 3 END AS estado
                    FROM rrhh_recursos.empleados e
                    LEFT JOIN rrhh_recursos.contratos c
                        ON c.empleadoid = e.empleadoid AND c.estaactivo = 1
                    ORDER BY e.empleadoid DESC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic item = new ExpandoObject();
                        item.Id               = rd.GetInt32(0);
                        item.Nombres          = rd.GetString(1);
                        item.ApellidoPaterno  = rd.GetString(2);
                        item.ApellidoMaterno  = rd.IsDBNull(3) ? "" : rd.GetString(3);
                        item.NumeroDocumento  = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        item.Cargo            = rd.GetString(5);
                        item.SueldoBase       = rd.GetDecimal(6);
                        item.TipoContrato     = (SGE.Models.TipoContrato)Enum.Parse(typeof(SGE.Models.TipoContrato), rd.GetString(7).Replace(" ", "_"), ignoreCase: true);
                        item.SistemaPrevisional = (SGE.Models.TipoAFP)Enum.Parse(typeof(SGE.Models.TipoAFP), rd.GetString(8).Replace(" ", "_"), ignoreCase: true);
                        item.Estado           = (SGE.Models.EstadoEmpleado)rd.GetInt32(9);
                        vm.EmpleadosPreview.Add(item);
                    }
                }
            }
            catch {}

            // Populate some dummy lists if they are empty
            if (vm.EmpleadosPreview.Count == 0)
            {
                dynamic e1 = new ExpandoObject();
                e1.Id = 1; e1.Nombres = "Luis Fernando"; e1.ApellidoPaterno = "Gomez"; e1.ApellidoMaterno = "Silva";
                e1.NumeroDocumento = "12345678"; e1.Cargo = "Analista"; e1.SueldoBase = 3500.00m;
                e1.TipoContrato = SGE.Models.TipoContrato.Indefinido;
                e1.SistemaPrevisional = SGE.Models.TipoAFP.AFP_Prima;
                e1.Estado = SGE.Models.EstadoEmpleado.Activo;
                vm.EmpleadosPreview.Add(e1);
                dynamic e2 = new ExpandoObject();
                e2.Id = 2; e2.Nombres = "Maria Elena"; e2.ApellidoPaterno = "Paz"; e2.ApellidoMaterno = "Torres";
                e2.NumeroDocumento = "87654321"; e2.Cargo = "Contadora"; e2.SueldoBase = 4200.00m;
                e2.TipoContrato = SGE.Models.TipoContrato.Indefinido;
                e2.SistemaPrevisional = SGE.Models.TipoAFP.ONP;
                e2.Estado = SGE.Models.EstadoEmpleado.Activo;
                vm.EmpleadosPreview.Add(e2);
            }

            // Populate dummy UltimasPlanillas
            dynamic p1 = new ExpandoObject();
            p1.Codigo = "PL-2026-05"; p1.Periodo = "Mayo 2026"; 
            p1.Empleados = 2; p1.TotalBruto = 7700m; p1.TotalDescuentos = 1000m;
            p1.TotalNeto = vm.TotalPlanillaMesActual > 0 ? vm.TotalPlanillaMesActual : 6700m; 
            p1.Estado = "Pagado";
            vm.UltimasPlanillas.Add(p1);

            return PartialView("~/Views/RRHH/Nomina/Index.cshtml", vm);
        }

        public IActionResult Planillas()
        {
            var vm = new PlanillasViewModel();
            return PartialView("~/Views/RRHH/Nomina/Planillas.cshtml", vm);
        }

        public IActionResult Empleados()
        {
            var vm = new EmpleadoViewModel();
            return PartialView("~/Views/RRHH/Nomina/Empleados.cshtml", vm);
        }

        public IActionResult Conceptos()
        {
            var vm = new ConceptosViewModel();
            return PartialView("~/Views/RRHH/Nomina/Conceptos.cshtml", vm);
        }

        public IActionResult Descuentos()
        {
            var vm = new DescuentosViewModel();
            return PartialView("~/Views/RRHH/Nomina/Descuentos.cshtml", vm);
        }

        public IActionResult Beneficios()
        {
            var vm = new BeneficiosViewModel();
            // populate some dummy beneficios
            vm.Beneficios.Add(new BeneficioItem { Id = 1, Codigo = "BEN-001", Nombre = "Asignación Familiar", Categoria = CategoriaBeneficio.Otros, Tipo = TipoBeneficio.Beneficio, Periodicidad = Periodicidad.Mensual, MontoCadena = "10% Remuneración Mínima", Activo = true });
            vm.Beneficios.Add(new BeneficioItem { Id = 2, Codigo = "BEN-002", Nombre = "Bono de Movilidad", Categoria = CategoriaBeneficio.Transporte, Tipo = TipoBeneficio.Bonificacion, Periodicidad = Periodicidad.Mensual, MontoCadena = "S/ 150.00", Activo = true });
            return PartialView("~/Views/RRHH/Nomina/Beneficios.cshtml", vm);
        }

        public IActionResult EsSalud()
        {
            var vm = new EsSaludViewModel();
            return PartialView("~/Views/RRHH/Nomina/EsSalud.cshtml", vm);
        }

        public IActionResult Gratificaciones()
        {
            var vm = new GratificacionesViewModel();
            // populate some dummy gratificaciones
            vm.Gratificaciones.Add(new GratificacionItem { Id = 1, Codigo = "GRA-001", Nombre = "Gratificación Fiestas Patrias", Tipo = TipoGratificacion.Obligatoria, Periodo = "Julio 2026", Frecuencia = FrecuenciaGratificacion.Semestral, PorcentajeMonto = "100% Sueldo", BaseDeCalculo = BaseCalculo.RemuneracionBasica, Estado = EstadoGratificacion.Pendiente, EmpleadosAplica = "Todos", CantidadEmpleados = 94, CreadoPor = "admin" });
            return PartialView("~/Views/RRHH/Nomina/Gratificaciones.cshtml", vm);
        }

        public IActionResult Utilidades()
        {
            var vm = new UtilidadesViewModel();
            return PartialView("~/Views/RRHH/Nomina/Utilidades.cshtml", vm);
        }

        public IActionResult SunatPdt()
        {
            var vm = new SunatPdtViewModel();
            return PartialView("~/Views/RRHH/Nomina/SunatPdt.cshtml", vm);
        }

        public IActionResult HistorialPagos()
        {
            var vm = new HistorialPagosViewModel();
            return PartialView("~/Views/RRHH/Nomina/HistorialPagos.cshtml", vm);
        }

        public IActionResult Reportes()
        {
            var vm = new ReportesViewModel();
            ViewBag.TotalReportes = 0;
            ViewBag.Completados = 0;
            ViewBag.EnProceso = 0;
            ViewBag.TotalDescargas = 0;
            return PartialView("~/Views/RRHH/Nomina/Reportes.cshtml", vm);
        }

        public IActionResult Configuracion()
        {
            ViewBag.Seccion = HttpContext.Request.Query["seccion"].ToString();
            if (string.IsNullOrEmpty(ViewBag.Seccion)) ViewBag.Seccion = "parametros";

            ViewBag.Params = new ParametrosGenerales { Empresa = "Mi Empresa SAC" };
            ViewBag.Rangos = new List<RangoRenta>();
            ViewBag.Bancos = new List<BancoConfig>();
            ViewBag.Feriados = new List<Feriado>();
            ViewBag.Centros = new List<CentroCosto>();
            ViewBag.UsuariosNom = new List<UsuarioNomina>();

            return PartialView("~/Views/RRHH/Nomina/Configuracion.cshtml");
        }

        public IActionResult DetallePlanilla()
        {
            var vm = new DetallePlanillaViewModel();
            return PartialView("~/Views/RRHH/Nomina/DetallePlanilla.cshtml", vm);
        }
        
        public IActionResult Boleta()
        {
            var vm = new BoletaPagoViewModel();
            return PartialView("~/Views/RRHH/Nomina/Boleta.cshtml", vm);
        }
    }
}
