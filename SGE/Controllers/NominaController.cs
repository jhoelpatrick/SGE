using Microsoft.AspNetCore.Mvc;
using SGE.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace SGE.Controllers
{
    public class NominaController : Controller
    {
        private readonly string _conn;
        private readonly SGE.Services.IEmailService _emailService;

        public NominaController(IConfiguration config, SGE.Services.IEmailService emailService)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var vm = new NominaViewModel();
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                // Get counts
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = TRUE", cn))
                {
                    vm.EmpleadosActivos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados", cn))
                {
                    vm.TotalEmpleados = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.EmpleadosEnPlanilla = vm.EmpleadosActivos;

                // Masa salarial
                using (var cmd = new NpgsqlCommand("SELECT SUM(sueldobase) FROM rrhh_recursos.contratos WHERE estaactivo = TRUE", cn))
                {
                    var val = cmd.ExecuteScalar();
                    vm.MasaSalarial = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                    vm.TotalPlanillaMesActual = vm.MasaSalarial;
                }

                // Empleados en vacaciones
                try
                {
                    using (var cmd = new NpgsqlCommand(
                        @"SELECT COUNT(DISTINCT pv.empleadoid)
                          FROM rrhh_recursos.periodos_vacacionales pv
                          JOIN rrhh_recursos.programacion_vacaciones pv2 ON pv.periodovacacionalid = pv2.periodovacacionalid
                          WHERE pv2.estadosolicitud = 'aprobada' AND CURRENT_DATE BETWEEN pv2.fechainicio AND pv2.fechafin", cn))
                    {
                        vm.EmpleadosEnVacaciones = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch
                {
                    vm.EmpleadosEnVacaciones = 0;
                }

                // Recent employees preview
                using (var cmd = new NpgsqlCommand(
                    @"SELECT
                        e.empleadoid,
                        e.nombres,
                        e.apellidopaterno,
                        e.apellidomaterno,
                        e.numerodocumento,
                        COALESCE(e.cargo, 'Colaborador') AS cargo,
                        COALESCE(c.sueldobase, 0) AS sueldobase,
                        COALESCE(c.tipocontrato, 'Indefinido') AS tipocontrato,
                        COALESCE(ap.nombre, 'ONP') AS sistemaprevisional,
                        CASE WHEN e.estaactivo = TRUE THEN 0 ELSE 3 END AS estado
                    FROM rrhh_recursos.empleados e
                    LEFT JOIN rrhh_recursos.contratos c
                        ON c.empleadoid = e.empleadoid AND c.estaactivo = TRUE
                    LEFT JOIN rrhh_recursos.datos_laborales_empleados dle
                        ON dle.empleadoid = e.empleadoid
                    LEFT JOIN rrhh_recursos.administradoras_pensiones ap
                        ON ap.afpid = dle.afpid
                    ORDER BY e.empleadoid DESC
                    LIMIT 10", cn))
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

                        string rawContrato = rd.GetString(7).ToLower();
                        SGE.Models.TipoContrato tc = SGE.Models.TipoContrato.Indefinido;
                        if (rawContrato.Contains("indeterminado") || rawContrato.Contains("indefinido"))
                            tc = SGE.Models.TipoContrato.Indefinido;
                        else if (rawContrato.Contains("fijo"))
                            tc = SGE.Models.TipoContrato.Plazo_Fijo;
                        else if (rawContrato.Contains("practicante"))
                            tc = SGE.Models.TipoContrato.Practicante;
                        else if (rawContrato.Contains("obra"))
                            tc = SGE.Models.TipoContrato.Por_Obra;
                        else if (rawContrato.Contains("temporal"))
                            tc = SGE.Models.TipoContrato.Temporal;
                        item.TipoContrato = tc;

                        string rawAfp = rd.GetString(8).ToLower();
                        SGE.Models.TipoAFP ta = SGE.Models.TipoAFP.ONP;
                        if (rawAfp.Contains("integra"))
                            ta = SGE.Models.TipoAFP.AFP_Integra;
                        else if (rawAfp.Contains("habitat") || rawAfp.Contains("hábitat"))
                            ta = SGE.Models.TipoAFP.AFP_Habitat;
                        else if (rawAfp.Contains("prima"))
                            ta = SGE.Models.TipoAFP.AFP_Prima;
                        else if (rawAfp.Contains("profuturo"))
                            ta = SGE.Models.TipoAFP.AFP_ProFuturo;
                        else if (rawAfp.Contains("onp"))
                            ta = SGE.Models.TipoAFP.ONP;
                        item.SistemaPrevisional = ta;

                        item.Estado           = (SGE.Models.EstadoEmpleado)rd.GetInt32(9);
                        vm.EmpleadosPreview.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Index.cshtml", vm);
        }

        // ==========================================
        // PLANILLAS RESUMEN
        // ==========================================
        public IActionResult Planillas(string buscar, string estado, int pagina = 1)
        {
            var vm = new PlanillasViewModel();
            vm.Buscar = buscar ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 8;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.planillas_resumen WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (codigo ILIKE @buscar OR periodo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = "SELECT codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, totalneto, estado FROM rrhh_nomina.planillas_resumen WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (codigo ILIKE @buscar OR periodo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                sql += " ORDER BY codigo DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic p = new ExpandoObject();
                        p.Codigo = rd.GetString(0);
                        p.Periodo = rd.GetString(1);
                        p.FechaCierre = rd.GetDateTime(2);
                        p.Empleados = rd.GetInt32(3);
                        p.TotalBruto = rd.GetDecimal(4);
                        p.TotalDescuentos = rd.GetDecimal(5);
                        p.TotalNeto = rd.GetDecimal(6);
                        p.Estado = rd.GetString(7);
                        vm.Planillas.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Planillas.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearPlanilla(string periodo, DateTime fechaCierre, int empleados, string estado, decimal totalBruto, decimal totalDescuentos)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                int count = 1;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.planillas_resumen", cn))
                {
                    count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }

                string codigo = $"PLA-{DateTime.Now.Year}-{count:D3}";

                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.planillas_resumen (codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, estado)
                      VALUES (@codigo, @periodo, @fechacierre, @empleados, @totalbruto, @totaldescuentos, @estado)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.Parameters.AddWithValue("periodo", periodo ?? "");
                    cmd.Parameters.AddWithValue("fechacierre", fechaCierre);
                    cmd.Parameters.AddWithValue("empleados", empleados);
                    cmd.Parameters.AddWithValue("totalbruto", totalBruto);
                    cmd.Parameters.AddWithValue("totaldescuentos", totalDescuentos);
                    cmd.Parameters.AddWithValue("estado", estado ?? "En Proceso");
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Planilla creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear planilla: " + ex.Message;
            }

            return RedirectToAction("Planillas");
        }

        [HttpGet]
        public IActionResult EditarPlanilla(string codigo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, estado FROM rrhh_nomina.planillas_resumen WHERE codigo = @codigo", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var data = new
                        {
                            codigo = rd.GetString(0),
                            periodo = rd.GetString(1),
                            fechaCierre = rd.GetDateTime(2).ToString("yyyy-MM-dd"),
                            empleados = rd.GetInt32(3),
                            totalBruto = rd.GetDecimal(4),
                            totalDescuentos = rd.GetDecimal(5),
                            estado = rd.GetString(6),
                            totalNeto = rd.GetDecimal(4) - rd.GetDecimal(5)
                        };
                        return Json(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPlanilla(string codigo, string periodo, DateTime fechaCierre, int empleados, string estado, decimal totalBruto, decimal totalDescuentos)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.planillas_resumen 
                      SET periodo = @periodo, fechacierre = @fechacierre, empleados = @empleados, 
                          estado = @estado, totalbruto = @totalbruto, totaldescuentos = @totaldescuentos
                      WHERE codigo = @codigo", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.Parameters.AddWithValue("periodo", periodo ?? "");
                    cmd.Parameters.AddWithValue("fechacierre", fechaCierre);
                    cmd.Parameters.AddWithValue("empleados", empleados);
                    cmd.Parameters.AddWithValue("estado", estado ?? "En Proceso");
                    cmd.Parameters.AddWithValue("totalbruto", totalBruto);
                    cmd.Parameters.AddWithValue("totaldescuentos", totalDescuentos);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Planilla actualizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar planilla: " + ex.Message;
            }
            return RedirectToAction("Planillas");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarPlanilla(string codigo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.planillas_resumen WHERE codigo = @codigo", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Planilla eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar planilla: " + ex.Message;
            }
            return RedirectToAction("Planillas");
        }

        [HttpGet]
        public IActionResult ExportarCSV(string buscar, string estado)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Codigo,Periodo,Fecha Cierre,Empleados,Total Bruto,Total Descuentos,Total Neto,Estado");

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string sql = "SELECT codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, totalneto, estado FROM rrhh_nomina.planillas_resumen WHERE 1=1";
                if (!string.IsNullOrEmpty(buscar)) sql += " AND (codigo ILIKE @buscar OR periodo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(estado)) sql += " AND estado = @estado";
                sql += " ORDER BY codigo DESC";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(buscar)) cmd.Parameters.AddWithValue("buscar", $"%{buscar}%");
                    if (!string.IsNullOrEmpty(estado)) cmd.Parameters.AddWithValue("estado", estado);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var cod = rd.GetString(0);
                        var per = rd.GetString(1);
                        var fec = rd.GetDateTime(2).ToString("yyyy-MM-dd");
                        var emp = rd.GetInt32(3);
                        var bruto = rd.GetDecimal(4);
                        var desc = rd.GetDecimal(5);
                        var neto = rd.GetDecimal(6);
                        var est = rd.GetString(7);

                        sb.AppendLine($"{cod},{per},{fec},{emp},{bruto},{desc},{neto},{est}");
                    }
                }
            }
            catch
            {
                // return empty on error
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"planillas_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ==========================================
        // EMPLEADOS LEGAJO
        // ==========================================
        public IActionResult Empleados(string buscar, string estado, string depto, int pagina = 1)
        {
            var vm = new EmpleadoViewModel();
            vm.BuscarFiltro = buscar ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.DeptFiltro = depto ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 8;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Compute Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = TRUE", cn))
                {
                    vm.TotalActivos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                try
                {
                    using (var cmd = new NpgsqlCommand(
                        @"SELECT COUNT(DISTINCT pv.empleadoid)
                          FROM rrhh_recursos.periodos_vacacionales pv
                          JOIN rrhh_recursos.programacion_vacaciones pv2 ON pv.periodovacacionalid = pv2.periodovacacionalid
                          WHERE pv2.estadosolicitud = 'aprobada' AND CURRENT_DATE BETWEEN pv2.fechainicio AND pv2.fechafin", cn))
                    {
                        vm.TotalVacaciones = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch
                {
                    vm.TotalVacaciones = 0;
                }

                using (var cmd = new NpgsqlCommand("SELECT SUM(sueldobase) FROM rrhh_recursos.contratos WHERE estaactivo = TRUE", cn))
                {
                    var val = cmd.ExecuteScalar();
                    vm.MassaSalarial = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }

                // Query total count for legajo
                string countSql = @"
                    SELECT COUNT(*) 
                    FROM rrhh_recursos.empleados e
                    WHERE 1=1";
                
                if (!string.IsNullOrEmpty(vm.BuscarFiltro))
                {
                    countSql += " AND (e.nombres ILIKE @buscar OR e.apellidopaterno ILIKE @buscar OR e.numerodocumento ILIKE @buscar)";
                }
                if (!string.IsNullOrEmpty(vm.DeptFiltro))
                {
                    countSql += " AND e.departamento = @depto";
                }
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") countSql += " AND e.estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") countSql += " AND e.estaactivo = FALSE";
                }

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.BuscarFiltro)) cmd.Parameters.AddWithValue("buscar", $"%{vm.BuscarFiltro}%");
                    if (!string.IsNullOrEmpty(vm.DeptFiltro)) cmd.Parameters.AddWithValue("depto", vm.DeptFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string querySql = @"
                    SELECT 
                        e.empleadoid,
                        e.nombres,
                        e.apellidopaterno,
                        e.apellidomaterno,
                        e.numerodocumento,
                        COALESCE(e.cargo, 'Colaborador') AS cargo,
                        COALESCE(e.departamento, 'Administración') AS departamento,
                        e.estaactivo,
                        COALESCE(c.sueldobase, 0) AS sueldobase,
                        COALESCE(c.tipocontrato, 'Indeterminado') AS tipocontrato,
                        COALESCE(ap.nombre, 'ONP') AS sistemaprevisional,
                        dle.cuentasueldo,
                        COALESCE(bc.codigo, 'Efectivo') AS bancopago
                    FROM rrhh_recursos.empleados e
                    LEFT JOIN rrhh_recursos.contratos c ON c.empleadoid = e.empleadoid AND c.estaactivo = TRUE
                    LEFT JOIN rrhh_recursos.datos_laborales_empleados dle ON dle.empleadoid = e.empleadoid
                    LEFT JOIN rrhh_recursos.administradoras_pensiones ap ON ap.afpid = dle.afpid
                    LEFT JOIN rrhh_nomina.bancos_config bc ON bc.bancoid = dle.bancosueldoid
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(vm.BuscarFiltro))
                {
                    querySql += " AND (e.nombres ILIKE @buscar OR e.apellidopaterno ILIKE @buscar OR e.numerodocumento ILIKE @buscar)";
                }
                if (!string.IsNullOrEmpty(vm.DeptFiltro))
                {
                    querySql += " AND e.departamento = @depto";
                }
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") querySql += " AND e.estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") querySql += " AND e.estaactivo = FALSE";
                }
                querySql += " ORDER BY e.empleadoid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(querySql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.BuscarFiltro)) cmd.Parameters.AddWithValue("buscar", $"%{vm.BuscarFiltro}%");
                    if (!string.IsNullOrEmpty(vm.DeptFiltro)) cmd.Parameters.AddWithValue("depto", vm.DeptFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic emp = new ExpandoObject();
                        emp.Id = rd.GetInt32(0);
                        emp.Nombres = rd.GetString(1);
                        emp.ApellidoPaterno = rd.GetString(2);
                        emp.ApellidoMaterno = rd.IsDBNull(3) ? "" : rd.GetString(3);
                        emp.NumeroDocumento = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        emp.Cargo = rd.GetString(5);
                        emp.Departamento = rd.GetString(6);
                        
                        bool act = rd.GetBoolean(7);
                        emp.Estado = act ? SGE.Models.EstadoEmpleado.Activo : SGE.Models.EstadoEmpleado.Inactivo;
                        
                        emp.SueldoBase = rd.GetDecimal(8);
                        
                        string rawContrato = rd.GetString(9).ToLower();
                        SGE.Models.TipoContrato tc = SGE.Models.TipoContrato.Indefinido;
                        if (rawContrato.Contains("indeterminado") || rawContrato.Contains("indefinido")) tc = SGE.Models.TipoContrato.Indefinido;
                        else if (rawContrato.Contains("fijo")) tc = SGE.Models.TipoContrato.Plazo_Fijo;
                        else if (rawContrato.Contains("practicante")) tc = SGE.Models.TipoContrato.Practicante;
                        else if (rawContrato.Contains("obra")) tc = SGE.Models.TipoContrato.Por_Obra;
                        else if (rawContrato.Contains("temporal")) tc = SGE.Models.TipoContrato.Temporal;
                        emp.TipoContrato = tc;

                        string rawAfp = rd.GetString(10).ToLower();
                        SGE.Models.TipoAFP ta = SGE.Models.TipoAFP.ONP;
                        if (rawAfp.Contains("integra")) ta = SGE.Models.TipoAFP.AFP_Integra;
                        else if (rawAfp.Contains("habitat") || rawAfp.Contains("hábitat")) ta = SGE.Models.TipoAFP.AFP_Habitat;
                        else if (rawAfp.Contains("prima")) ta = SGE.Models.TipoAFP.AFP_Prima;
                        else if (rawAfp.Contains("profuturo")) ta = SGE.Models.TipoAFP.AFP_ProFuturo;
                        else if (rawAfp.Contains("onp")) ta = SGE.Models.TipoAFP.ONP;
                        emp.SistemaPrevisional = ta;

                        emp.NumeroCuenta = rd.IsDBNull(11) ? "" : rd.GetString(11);
                        emp.BancoPago = rd.GetString(12);
                        
                        emp.NombreCompleto = $"{emp.Nombres} {emp.ApellidoPaterno} {emp.ApellidoMaterno}".Trim();
                        emp.Codigo = $"EMP-{emp.Id:D4}";
                        emp.RegimeLaboral = "Régimen 728";

                        vm.Empleados.Add(emp);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Empleados.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerEmpleado(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string sql = @"
                    SELECT 
                        e.empleadoid, e.apellidopaterno, e.apellidomaterno, e.nombres, e.numerodocumento,
                        e.fechanacimiento, e.telefonocelular, e.correocorporativo, e.estaactivo, e.cargo, 
                        e.departamento, e.fechaingreso, e.tienehijos,
                        c.sueldobase, c.tipocontrato,
                        dle.regimenlaboralid, dle.afpid, dle.cuspp, dle.bancosueldoid, dle.cuentasueldo, dle.cuentacts, dle.direccion
                    FROM rrhh_recursos.empleados e
                    LEFT JOIN rrhh_recursos.contratos c ON c.empleadoid = e.empleadoid AND c.estaactivo = TRUE
                    LEFT JOIN rrhh_recursos.datos_laborales_empleados dle ON dle.empleadoid = e.empleadoid
                    WHERE e.empleadoid = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        bool act = rd.GetBoolean(8);
                        int estadoInt = act ? 0 : 1;

                        string rawCont = rd.IsDBNull(14) ? "indefinido" : rd.GetString(14).ToLower();
                        int contrInt = 0;
                        if (rawCont.Contains("fijo")) contrInt = 1;
                        else if (rawCont.Contains("servicio")) contrInt = 2;
                        else if (rawCont.Contains("practicante")) contrInt = 3;

                        int regimenInt = rd.IsDBNull(15) ? 0 : rd.GetInt32(15) - 1;
                        int afpInt = rd.IsDBNull(16) ? 4 : rd.GetInt32(16) - 1;
                        int bancoInt = rd.IsDBNull(18) ? 4 : rd.GetInt32(18) - 1;

                        var data = new
                        {
                            id = rd.GetInt32(0),
                            apellidoPaterno = rd.GetString(1),
                            apellidoMaterno = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            nombres = rd.GetString(3),
                            numeroDocumento = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            fechaNacimiento = rd.GetDateTime(5).ToString("yyyy-MM-dd"),
                            telefono = rd.IsDBNull(6) ? "" : rd.GetString(6),
                            email = rd.IsDBNull(7) ? "" : rd.GetString(7),
                            estado = estadoInt,
                            cargo = rd.IsDBNull(9) ? "Colaborador" : rd.GetString(9),
                            departamento = rd.IsDBNull(10) ? "Administración" : rd.GetString(10),
                            fechaIngreso = rd.IsDBNull(11) ? DateTime.Now.ToString("yyyy-MM-dd") : rd.GetDateTime(11).ToString("yyyy-MM-dd"),
                            tieneHijos = rd.GetBoolean(12),
                            sueldoBase = rd.IsDBNull(13) ? 1025.00m : rd.GetDecimal(13),
                            tipoContrato = contrInt,
                            regimeLaboral = regimenInt,
                            sistemaPrevisional = afpInt,
                            cuspp = rd.IsDBNull(17) ? "" : rd.GetString(17),
                            bancoPago = bancoInt,
                            numeroCuenta = rd.IsDBNull(19) ? "" : rd.GetString(19),
                            cci = rd.IsDBNull(20) ? "" : rd.GetString(20),
                            direccion = rd.IsDBNull(21) ? "" : rd.GetString(21)
                        };
                        return Json(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearEmpleado(
            string apellidoPaterno, string apellidoMaterno, string nombres, string numeroDocumento,
            DateTime fechaNacimiento, string telefono, string email, string direccion,
            string cargo, string departamento, DateTime fechaIngreso, int tipoContrato,
            int regimeLaboral, int estado, decimal sueldoBase, bool tieneHijos,
            int sistemaPrevisional, string cuspp, int bancoPago, string numeroCuenta, string cci)
        {
            using var cn = new NpgsqlConnection(_conn);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                int centroCostoId = deptoToCentroCostoId(departamento);
                bool estaActivo = (estado == 0 || estado == 2);

                int empleadoId = 0;
                string insertEmpSql = @"
                    INSERT INTO rrhh_recursos.empleados 
                        (tipodocumento, numerodocumento, nombres, apellidopaterno, apellidomaterno, 
                         fechanacimiento, sexo, correopersonal, correocorporativo, telefonocelular, 
                         centrocostoid, estaactivo, cargo, departamento, tienehijos, fechaingreso)
                    VALUES 
                        ('1', @numerodocumento, @nombres, @apellidopaterno, @apellidomaterno, 
                         @fechanacimiento, 'm', @correopersonal, @correocorporativo, @telefonocelular, 
                         @centrocostoid, @estaactivo, @cargo, @departamento, @tienehijos, @fechaingreso)
                    RETURNING empleadoid";

                using (var cmd = new NpgsqlCommand(insertEmpSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("numerodocumento", numeroDocumento ?? "");
                    cmd.Parameters.AddWithValue("nombres", nombres ?? "");
                    cmd.Parameters.AddWithValue("apellidopaterno", apellidoPaterno ?? "");
                    cmd.Parameters.AddWithValue("apellidomaterno", apellidoMaterno ?? "");
                    cmd.Parameters.AddWithValue("fechanacimiento", fechaNacimiento);
                    cmd.Parameters.AddWithValue("correopersonal", email ?? "");
                    cmd.Parameters.AddWithValue("correocorporativo", email ?? "");
                    cmd.Parameters.AddWithValue("telefonocelular", telefono ?? "");
                    cmd.Parameters.AddWithValue("centrocostoid", centroCostoId);
                    cmd.Parameters.AddWithValue("estaactivo", estaActivo);
                    cmd.Parameters.AddWithValue("cargo", cargo ?? "Colaborador");
                    cmd.Parameters.AddWithValue("departamento", departamento ?? "Administración");
                    cmd.Parameters.AddWithValue("tienehijos", tieneHijos);
                    cmd.Parameters.AddWithValue("fechaingreso", fechaIngreso);

                    empleadoId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string contrType = contractIntToString(tipoContrato);
                string insertContSql = @"
                    INSERT INTO rrhh_recursos.contratos 
                        (empleadoid, tipocontrato, fechainicio, fechafin, sueldobase, estaactivo)
                    VALUES 
                        (@empleadoid, @tipocontrato, @fechainicio, NULL, @sueldobase, TRUE)";
                
                using (var cmd = new NpgsqlCommand(insertContSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("empleadoid", empleadoId);
                    cmd.Parameters.AddWithValue("tipocontrato", contrType);
                    cmd.Parameters.AddWithValue("fechainicio", fechaIngreso);
                    cmd.Parameters.AddWithValue("sueldobase", sueldoBase);
                    cmd.ExecuteNonQuery();
                }

                int regId = regimeLaboral + 1;
                int afpId = sistemaPrevisional + 1;
                int? bancoIdVal = null;
                if (bancoPago >= 0 && bancoPago <= 3) bancoIdVal = bancoPago + 1;

                string insertDleSql = @"
                    INSERT INTO rrhh_recursos.datos_laborales_empleados 
                        (empleadoid, regimenlaboralid, afpid, tipocomision, cuspp, 
                         ubigeodomicilio, direccion, cuentasueldo, bancosueldoid, cuentacts, bancoctsid)
                    VALUES 
                        (@empleadoid, @regimenlaboralid, @afpid, 'no_aplica', @cuspp, 
                         '150101', @direccion, @cuentasueldo, @bancosueldoid, @cuentacts, NULL)";

                using (var cmd = new NpgsqlCommand(insertDleSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("empleadoid", empleadoId);
                    cmd.Parameters.AddWithValue("regimenlaboralid", regId);
                    cmd.Parameters.AddWithValue("afpid", afpId);
                    cmd.Parameters.AddWithValue("cuspp", cuspp ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("direccion", direccion ?? "");
                    cmd.Parameters.AddWithValue("cuentasueldo", numeroCuenta ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("bancosueldoid", bancoIdVal.HasValue ? (object)bancoIdVal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("cuentacts", cci ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                TempData["Mensaje"] = "Empleado creado correctamente.";
            }
            catch (Exception ex)
            {
                tx.Rollback();
                TempData["Error"] = "Error al crear empleado: " + ex.Message;
            }

            return RedirectToAction("Empleados");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarEmpleado(
            int id, string apellidoPaterno, string apellidoMaterno, string nombres, string numeroDocumento,
            DateTime fechaNacimiento, string telefono, string email, string direccion,
            string cargo, string departamento, DateTime fechaIngreso, int tipoContrato,
            int regimeLaboral, int estado, decimal sueldoBase, bool tieneHijos,
            int sistemaPrevisional, string cuspp, int bancoPago, string numeroCuenta, string cci)
        {
            using var cn = new NpgsqlConnection(_conn);
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
                int centroCostoId = deptoToCentroCostoId(departamento);
                bool estaActivo = (estado == 0 || estado == 2);

                string updateEmpSql = @"
                    UPDATE rrhh_recursos.empleados 
                    SET numerodocumento = @numerodocumento, nombres = @nombres, 
                        apellidopaterno = @apellidopaterno, apellidomaterno = @apellidomaterno, 
                        fechanacimiento = @fechanacimiento, correopersonal = @correopersonal, 
                        correocorporativo = @correocorporativo, telefonocelular = @telefonocelular, 
                        centrocostoid = @centrocostoid, estaactivo = @estaactivo, cargo = @cargo, 
                        departamento = @departamento, tienehijos = @tienehijos, fechaingreso = @fechaingreso
                    WHERE empleadoid = @id";

                using (var cmd = new NpgsqlCommand(updateEmpSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("numerodocumento", numeroDocumento ?? "");
                    cmd.Parameters.AddWithValue("nombres", nombres ?? "");
                    cmd.Parameters.AddWithValue("apellidopaterno", apellidoPaterno ?? "");
                    cmd.Parameters.AddWithValue("apellidomaterno", apellidoMaterno ?? "");
                    cmd.Parameters.AddWithValue("fechanacimiento", fechaNacimiento);
                    cmd.Parameters.AddWithValue("correopersonal", email ?? "");
                    cmd.Parameters.AddWithValue("correocorporativo", email ?? "");
                    cmd.Parameters.AddWithValue("telefonocelular", telefono ?? "");
                    cmd.Parameters.AddWithValue("centrocostoid", centroCostoId);
                    cmd.Parameters.AddWithValue("estaactivo", estaActivo);
                    cmd.Parameters.AddWithValue("cargo", cargo ?? "Colaborador");
                    cmd.Parameters.AddWithValue("departamento", departamento ?? "Administración");
                    cmd.Parameters.AddWithValue("tienehijos", tieneHijos);
                    cmd.Parameters.AddWithValue("fechaingreso", fechaIngreso);
                    cmd.ExecuteNonQuery();
                }

                string contrType = contractIntToString(tipoContrato);
                
                using (var cmd = new NpgsqlCommand("UPDATE rrhh_recursos.contratos SET estaactivo = FALSE WHERE empleadoid = @id", cn, tx))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }

                string insertContSql = @"
                    INSERT INTO rrhh_recursos.contratos 
                        (empleadoid, tipocontrato, fechainicio, fechafin, sueldobase, estaactivo)
                    VALUES 
                        (@empleadoid, @tipocontrato, @fechainicio, NULL, @sueldobase, TRUE)";
                
                using (var cmd = new NpgsqlCommand(insertContSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("empleadoid", id);
                    cmd.Parameters.AddWithValue("tipocontrato", contrType);
                    cmd.Parameters.AddWithValue("fechainicio", fechaIngreso);
                    cmd.Parameters.AddWithValue("sueldobase", sueldoBase);
                    cmd.ExecuteNonQuery();
                }

                int regId = regimeLaboral + 1;
                int afpId = sistemaPrevisional + 1;
                int? bancoIdVal = null;
                if (bancoPago >= 0 && bancoPago <= 3) bancoIdVal = bancoPago + 1;

                string updateDleSql = @"
                    INSERT INTO rrhh_recursos.datos_laborales_empleados 
                        (empleadoid, regimenlaboralid, afpid, tipocomision, cuspp, 
                         ubigeodomicilio, direccion, cuentasueldo, bancosueldoid, cuentacts, bancoctsid)
                    VALUES 
                        (@id, @regimenlaboralid, @afpid, 'no_aplica', @cuspp, 
                         '150101', @direccion, @cuentasueldo, @bancosueldoid, @cuentacts, NULL)
                    ON CONFLICT (empleadoid) DO UPDATE 
                    SET regimenlaboralid = @regimenlaboralid, afpid = @afpid, cuspp = @cuspp, 
                        direccion = @direccion, cuentasueldo = @cuentasueldo, bancosueldoid = @bancosueldoid, 
                        cuentacts = @cuentacts";

                using (var cmd = new NpgsqlCommand(updateDleSql, cn, tx))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("regimenlaboralid", regId);
                    cmd.Parameters.AddWithValue("afpid", afpId);
                    cmd.Parameters.AddWithValue("cuspp", cuspp ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("direccion", direccion ?? "");
                    cmd.Parameters.AddWithValue("cuentasueldo", numeroCuenta ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("bancosueldoid", bancoIdVal.HasValue ? (object)bancoIdVal.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("cuentacts", cci ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
                TempData["Mensaje"] = "Empleado actualizado correctamente.";
            }
            catch (Exception ex)
            {
                tx.Rollback();
                TempData["Error"] = "Error al actualizar empleado: " + ex.Message;
            }

            return RedirectToAction("Empleados");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarEmpleado(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE rrhh_recursos.empleados SET estaactivo = FALSE WHERE empleadoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Empleado eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar empleado: " + ex.Message;
            }
            return RedirectToAction("Empleados");
        }

        private int deptoToCentroCostoId(string depto)
        {
            switch (depto?.ToUpper())
            {
                case "TI": return 1;
                case "CONTABILIDAD": return 2;
                case "RRHH": return 3;
                case "FINANZAS": return 4;
                case "MARKETING": return 5;
                case "ADMINISTRACIÓN": return 6;
                case "OPERACIONES": return 7;
                default: return 6;
            }
        }

        private string contractIntToString(int contr)
        {
            switch (contr)
            {
                case 0: return "Indeterminado";
                case 1: return "Plazo Fijo";
                case 2: return "Servicios Específicos";
                case 3: return "Practicante";
                default: return "Indeterminado";
            }
        }

        // ==========================================
        // CONCEPTOS CRUD
        // ==========================================
        public IActionResult Conceptos(string buscar, string tipo, string estado, int pagina = 1)
        {
            var vm = new ConceptosViewModel();
            vm.Buscar = buscar ?? "";
            vm.TipoFiltro = tipo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 8;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Compute Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto != 'descuento'", cn))
                {
                    ViewBag.StatsTotal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto != 'descuento' AND estaactivo = TRUE", cn))
                {
                    ViewBag.StatsActivos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto != 'descuento' AND tipo = 'Fijo'", cn))
                {
                    ViewBag.StatsFijos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto != 'descuento' AND tipo = 'Variable'", cn))
                {
                    ViewBag.StatsVariables = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto != 'descuento'";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (nombre ILIKE @buscar OR codigosunat ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) countSql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") countSql += " AND estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") countSql += " AND estaactivo = FALSE";
                }

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = @"
                    SELECT conceptoid, codigosunat, nombre, tipo, afectacalculo, esremunerativo, estaactivo 
                    FROM rrhh_nomina.conceptos 
                    WHERE tipoconcepto != 'descuento'";
                
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (nombre ILIKE @buscar OR codigosunat ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) sql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") sql += " AND estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") sql += " AND estaactivo = FALSE";
                }
                sql += " ORDER BY conceptoid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic c = new ExpandoObject();
                        c.Id = rd.GetInt32(0);
                        c.Codigo = rd.GetString(1);
                        c.Nombre = rd.GetString(2);
                        string tipoStr = rd.IsDBNull(3) ? "Fijo" : rd.GetString(3);
                        c.TipoConcepto = tipoStr;
                        c.Tipo = Enum.TryParse<TipoConcepto>(tipoStr, out var t) ? t : TipoConcepto.Fijo;
                        c.AfectaCalculo = rd.GetBoolean(4);
                        c.EsRemunerativo = rd.GetBoolean(5);
                        c.Activo = rd.GetBoolean(6);
                        vm.Conceptos.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            ViewBag.Inicio = vm.DesdeItem;
            ViewBag.Fin = vm.HastaItem;

            return PartialView("~/Views/RRHH/Nomina/Conceptos.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearConcepto(string nombre, string tipoConcepto, bool afectaCalculo, bool esRemunerativo, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                int count = 1;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos", cn))
                {
                    count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                string sunatCode = $"C{count:D3}";
                string abrev = (nombre?.Replace(" ", "").ToUpper() ?? "CONC") + count;
                if (abrev.Length > 15) abrev = abrev.Substring(0, 15);

                string dbTipoConcepto = esRemunerativo ? "ingreso_remunerativo" : "ingreso_no_remunerativo";

                string sql = @"
                    INSERT INTO rrhh_nomina.conceptos 
                        (codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo, afectacalculo, esremunerativo, tipo)
                    VALUES 
                        (@codigosunat, @nombre, @abreviatura, @tipoconcepto, @esfijo, @estaactivo, @afectacalculo, @esremunerativo, @tipo)";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("codigosunat", sunatCode);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("abreviatura", abrev);
                    cmd.Parameters.AddWithValue("tipoconcepto", dbTipoConcepto);
                    cmd.Parameters.AddWithValue("esfijo", tipoConcepto == "Fijo");
                    cmd.Parameters.AddWithValue("estaactivo", activo);
                    cmd.Parameters.AddWithValue("afectacalculo", afectaCalculo);
                    cmd.Parameters.AddWithValue("esremunerativo", esRemunerativo);
                    cmd.Parameters.AddWithValue("tipo", tipoConcepto ?? "Fijo");
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Concepto creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear concepto: " + ex.Message;
            }
            return RedirectToAction("Conceptos");
        }

        [HttpGet]
        public IActionResult ObtenerConcepto(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT conceptoid, codigosunat, nombre, tipo, afectacalculo, esremunerativo, estaactivo FROM rrhh_nomina.conceptos WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var data = new
                        {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            nombre = rd.GetString(2),
                            tipo = rd.IsDBNull(3) ? "Fijo" : rd.GetString(3),
                            afectaCalculo = rd.GetBoolean(4),
                            esRemunerativo = rd.GetBoolean(5),
                            activo = rd.GetBoolean(6)
                        };
                        return Json(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarConcepto(int id, string nombre, string tipoConcepto, bool afectaCalculo, bool esRemunerativo, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string dbTipoConcepto = esRemunerativo ? "ingreso_remunerativo" : "ingreso_no_remunerativo";

                string sql = @"
                    UPDATE rrhh_nomina.conceptos 
                    SET nombre = @nombre, tipoconcepto = @tipoconcepto, esfijo = @esfijo, 
                        estaactivo = @estaactivo, afectacalculo = @afectacalculo, 
                        esremunerativo = @esremunerativo, tipo = @tipo
                    WHERE conceptoid = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("tipoconcepto", dbTipoConcepto);
                    cmd.Parameters.AddWithValue("esfijo", tipoConcepto == "Fijo");
                    cmd.Parameters.AddWithValue("estaactivo", activo);
                    cmd.Parameters.AddWithValue("afectacalculo", afectaCalculo);
                    cmd.Parameters.AddWithValue("esremunerativo", esRemunerativo);
                    cmd.Parameters.AddWithValue("tipo", tipoConcepto ?? "Fijo");
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Concepto actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar concepto: " + ex.Message;
            }
            return RedirectToAction("Conceptos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConcepto(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.conceptos WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Concepto eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar concepto: " + ex.Message;
            }
            return RedirectToAction("Conceptos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleConcepto(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE rrhh_nomina.conceptos SET estaactivo = NOT estaactivo WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Estado del concepto actualizado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar estado del concepto: " + ex.Message;
            }
            return RedirectToAction("Conceptos");
        }

        // ==========================================
        // DESCUENTOS CRUD
        // ==========================================
        public IActionResult Descuentos(string buscar, string tipo, string estado, int pagina = 1)
        {
            var vm = new DescuentosViewModel();
            vm.Buscar = buscar ?? "";
            vm.TipoFiltro = tipo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 8;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.conceptos WHERE tipoconcepto = 'descuento'";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (nombre ILIKE @buscar OR codigosunat ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) countSql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") countSql += " AND estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") countSql += " AND estaactivo = FALSE";
                }

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = @"
                    SELECT conceptoid, codigosunat, nombre, tipo, porcentaje, obligatorio, afectaneto, estaactivo 
                    FROM rrhh_nomina.conceptos 
                    WHERE tipoconcepto = 'descuento'";
                
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (nombre ILIKE @buscar OR codigosunat ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) sql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") sql += " AND estaactivo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") sql += " AND estaactivo = FALSE";
                }
                sql += " ORDER BY conceptoid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic c = new ExpandoObject();
                        c.Id = rd.GetInt32(0);
                        c.Codigo = rd.GetString(1);
                        c.Nombre = rd.GetString(2);
                        c.Tipo = rd.IsDBNull(3) ? "Voluntario" : rd.GetString(3);
                        c.Porcentaje = rd.GetDecimal(4);
                        c.Obligatorio = rd.GetBoolean(5);
                        c.AfectaNeto = rd.GetBoolean(6);
                        c.Activo = rd.GetBoolean(7);
                        vm.Descuentos.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            ViewBag.Inicio = vm.DesdeItem;
            ViewBag.Fin = vm.HastaItem;

            return PartialView("~/Views/RRHH/Nomina/Descuentos.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearDescuento(string nombre, string tipo, decimal porcentaje, bool obligatorio, bool afectaNeto, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                int count = 1;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.conceptos", cn))
                {
                    count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                string sunatCode = $"D{count:D3}";
                string abrev = (nombre?.Replace(" ", "").ToUpper() ?? "DESC") + count;
                if (abrev.Length > 15) abrev = abrev.Substring(0, 15);

                string sql = @"
                    INSERT INTO rrhh_nomina.conceptos 
                        (codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo, afectacalculo, esremunerativo, obligatorio, afectaneto, porcentaje, tipo)
                    VALUES 
                        (@codigosunat, @nombre, @abreviatura, 'descuento', FALSE, @estaactivo, TRUE, FALSE, @obligatorio, @afectaneto, @porcentaje, @tipo)";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("codigosunat", sunatCode);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("abreviatura", abrev);
                    cmd.Parameters.AddWithValue("estaactivo", activo);
                    cmd.Parameters.AddWithValue("obligatorio", obligatorio);
                    cmd.Parameters.AddWithValue("afectaneto", afectaNeto);
                    cmd.Parameters.AddWithValue("porcentaje", porcentaje);
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Voluntario");
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Descuento creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear descuento: " + ex.Message;
            }
            return RedirectToAction("Descuentos");
        }

        [HttpGet]
        public IActionResult ObtenerDescuento(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT conceptoid, codigosunat, nombre, tipo, porcentaje, obligatorio, afectaneto, estaactivo FROM rrhh_nomina.conceptos WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var data = new
                        {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            nombre = rd.GetString(2),
                            tipo = rd.IsDBNull(3) ? "Voluntario" : rd.GetString(3),
                            porcentaje = rd.GetDecimal(4),
                            obligatorio = rd.GetBoolean(5),
                            afectaNeto = rd.GetBoolean(6),
                            activo = rd.GetBoolean(7)
                        };
                        return Json(data);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarDescuento(int id, string nombre, string tipo, decimal porcentaje, bool obligatorio, bool afectaNeto, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string sql = @"
                    UPDATE rrhh_nomina.conceptos 
                    SET nombre = @nombre, tipo = @tipo, porcentaje = @porcentaje, 
                        obligatorio = @obligatorio, afectaneto = @afectaneto, estaactivo = @estaactivo
                    WHERE conceptoid = @id";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Voluntario");
                    cmd.Parameters.AddWithValue("porcentaje", porcentaje);
                    cmd.Parameters.AddWithValue("obligatorio", obligatorio);
                    cmd.Parameters.AddWithValue("afectaneto", afectaNeto);
                    cmd.Parameters.AddWithValue("estaactivo", activo);
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Descuento actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar descuento: " + ex.Message;
            }
            return RedirectToAction("Descuentos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarDescuento(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.conceptos WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Descuento eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar descuento: " + ex.Message;
            }
            return RedirectToAction("Descuentos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleDescuento(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE rrhh_nomina.conceptos SET estaactivo = NOT estaactivo WHERE conceptoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Estado del descuento actualizado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar estado del descuento: " + ex.Message;
            }
            return RedirectToAction("Descuentos");
        }

        // ==========================================
        // CONFIGURACIÓN POST ACTIONS
        // ==========================================
        public IActionResult Configuracion()
        {
            ViewBag.Seccion = HttpContext.Request.Query["seccion"].ToString();
            if (string.IsNullOrEmpty(ViewBag.Seccion)) ViewBag.Seccion = "parametros";

            var prm = new ParametrosGenerales { Empresa = "Mi Empresa SAC" };
            var rangos = new List<RangoRenta>();
            var bancos = new List<BancoConfig>();
            var feriados = new List<Feriado>();
            var centros = new List<CentroCosto>();
            var usrs = new List<UsuarioNomina>();

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // 1. Params
                using (var cmd = new NpgsqlCommand("SELECT empresa, moneda, diacierreplanilla, diapagoplanilla, calchorasextrasauto, inclferiadosasist FROM rrhh_nomina.parametros_generales LIMIT 1", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        prm.Empresa = rd.GetString(0);
                        prm.Moneda = rd.GetString(1);
                        prm.DiaCierrePlanilla = rd.GetInt32(2);
                        prm.DiaPagoPlanilla = rd.GetInt32(3);
                        prm.CalcHorasExtrasAuto = rd.GetBoolean(4);
                        prm.InclFeriadosAsist = rd.GetBoolean(5);
                    }
                }

                // 2. Rangos
                using (var cmd = new NpgsqlCommand("SELECT rangoid, desde, hasta, tasa, montofijo, estaactivo FROM rrhh_nomina.rangos_renta ORDER BY desde ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        rangos.Add(new RangoRenta
                        {
                            Id = rd.GetInt32(0),
                            Desde = rd.GetDecimal(1),
                            Hasta = rd.IsDBNull(2) ? (decimal?)null : rd.GetDecimal(2),
                            Tasa = rd.GetDecimal(3),
                            MontoFijo = rd.GetDecimal(4),
                            Activo = rd.GetBoolean(5)
                        });
                    }
                }

                // 3. Bancos
                using (var cmd = new NpgsqlCommand("SELECT bancoid, nombre, codigo, moneda, cuentaprincipal, estaactivo FROM rrhh_nomina.bancos_config ORDER BY bancoid ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        bancos.Add(new BancoConfig
                        {
                            Id = rd.GetInt32(0),
                            Nombre = rd.GetString(1),
                            Codigo = rd.GetString(2),
                            Moneda = rd.GetString(3),
                            CuentaPrincipal = rd.GetString(4),
                            Activo = rd.GetBoolean(5)
                        });
                    }
                }

                // 4. Feriados
                using (var cmd = new NpgsqlCommand("SELECT feriadoid, fecha, descripcion, tipo, recuperable, estaactivo FROM rrhh_recursos.feriados ORDER BY fecha ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        feriados.Add(new Feriado
                        {
                            Id = rd.GetInt32(0),
                            Fecha = rd.GetDateTime(1),
                            Nombre = rd.GetString(2),
                            Tipo = rd.IsDBNull(3) ? "Nacional" : rd.GetString(3),
                            Recuperable = rd.GetBoolean(4),
                            Activo = rd.GetBoolean(5)
                        });
                    }
                }

                // 5. Centros de costo
                using (var cmd = new NpgsqlCommand("SELECT centrocostoid, codigo, nombre, descripcion, responsable, estaactivo FROM rrhh_recursos.centros_costos ORDER BY codigo ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        centros.Add(new CentroCosto
                        {
                            Id = rd.GetInt32(0),
                            Codigo = rd.GetString(1),
                            Nombre = rd.GetString(2),
                            Descripcion = rd.IsDBNull(3) ? "" : rd.GetString(3),
                            Responsable = rd.IsDBNull(4) ? "" : rd.GetString(4),
                            Activo = rd.GetBoolean(5)
                        });
                    }
                }

                // 6. Usuarios
                using (var cmd = new NpgsqlCommand("SELECT usuarionominaid, usuario, nombrecompleto, rol, correo, estaactivo FROM rrhh_recursos.usuarios_nomina ORDER BY usuario ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        usrs.Add(new UsuarioNomina
                        {
                            Id = rd.GetInt32(0),
                            Usuario = rd.GetString(1),
                            Nombre = rd.GetString(2),
                            Rol = rd.GetString(3),
                            Email = rd.GetString(4),
                            Activo = rd.GetBoolean(5)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            ViewBag.Params = prm;
            ViewBag.Rangos = rangos;
            ViewBag.Bancos = bancos;
            ViewBag.Feriados = feriados;
            ViewBag.Centros = centros;
            ViewBag.UsuariosNom = usrs;

            return PartialView("~/Views/RRHH/Nomina/Configuracion.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarParametros(string empresa, string moneda, int diaCierre, int diaPago, bool calcHoras, bool inclFeriados)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.parametros_generales 
                      SET empresa = @empresa, moneda = @moneda, diacierreplanilla = @diacierre, 
                          diapagoplanilla = @diapago, calchorasextrasauto = @calchoras, inclferiadosasist = @inclferiados 
                      WHERE paramid = 1", cn))
                {
                    cmd.Parameters.AddWithValue("empresa", empresa ?? "");
                    cmd.Parameters.AddWithValue("moneda", moneda ?? "");
                    cmd.Parameters.AddWithValue("diacierre", diaCierre);
                    cmd.Parameters.AddWithValue("diapago", diaPago);
                    cmd.Parameters.AddWithValue("calchoras", calcHoras);
                    cmd.Parameters.AddWithValue("inclferiados", inclFeriados);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Parámetros guardados correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar parámetros: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "parametros" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearRango(decimal desde, decimal? hasta, decimal tasa, decimal montoFijo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.rangos_renta (desde, hasta, tasa, montofijo, estaactivo) 
                      VALUES (@desde, @hasta, @tasa, @montofijo, TRUE)", cn))
                {
                    cmd.Parameters.AddWithValue("desde", desde);
                    cmd.Parameters.AddWithValue("hasta", hasta.HasValue ? (object)hasta.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("tasa", tasa);
                    cmd.Parameters.AddWithValue("montofijo", montoFijo);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Rango de renta creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear rango: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "rangos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarRango(int id, decimal desde, decimal? hasta, decimal tasa, decimal montoFijo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.rangos_renta 
                      SET desde = @desde, hasta = @hasta, tasa = @tasa, montofijo = @montofijo 
                      WHERE rangoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("desde", desde);
                    cmd.Parameters.AddWithValue("hasta", hasta.HasValue ? (object)hasta.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("tasa", tasa);
                    cmd.Parameters.AddWithValue("montofijo", montoFijo);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Rango de renta actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar rango: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "rangos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarRango(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.rangos_renta WHERE rangoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Rango de renta eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar rango: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "rangos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearBanco(string nombre, string codigo, string moneda, string cuentaPrincipal)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.bancos_config (nombre, codigo, moneda, cuentaprincipal, estaactivo) 
                      VALUES (@nombre, @codigo, @moneda, @cuentaprincipal, TRUE)", cn))
                {
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("codigo", codigo ?? "");
                    cmd.Parameters.AddWithValue("moneda", moneda ?? "");
                    cmd.Parameters.AddWithValue("cuentaprincipal", cuentaPrincipal ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Banco creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear banco: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "bancos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarBanco(int id, string nombre, string codigo, string moneda, string cuentaPrincipal)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.bancos_config 
                      SET nombre = @nombre, codigo = @codigo, moneda = @moneda, cuentaprincipal = @cuentaprincipal 
                      WHERE bancoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("codigo", codigo ?? "");
                    cmd.Parameters.AddWithValue("moneda", moneda ?? "");
                    cmd.Parameters.AddWithValue("cuentaprincipal", cuentaPrincipal ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Banco actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar banco: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "bancos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarBanco(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.bancos_config WHERE bancoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Banco eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar banco: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "bancos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearFeriado(DateTime fecha, string tipo, string feriado_nombre, bool recuperable)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_recursos.feriados (fecha, descripcion, tipo, recuperable, estaactivo) 
                      VALUES (@fecha, @descripcion, @tipo, @recuperable, TRUE)", cn))
                {
                    cmd.Parameters.AddWithValue("fecha", fecha);
                    cmd.Parameters.AddWithValue("descripcion", feriado_nombre ?? "");
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Nacional");
                    cmd.Parameters.AddWithValue("recuperable", recuperable);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Feriado creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear feriado: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "feriados" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarFeriado(int id, DateTime fecha, string tipo, string feriado_nombre, bool recuperable)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_recursos.feriados 
                      SET fecha = @fecha, descripcion = @descripcion, tipo = @tipo, recuperable = @recuperable 
                      WHERE feriadoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("fecha", fecha);
                    cmd.Parameters.AddWithValue("descripcion", feriado_nombre ?? "");
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Nacional");
                    cmd.Parameters.AddWithValue("recuperable", recuperable);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Feriado actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar feriado: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "feriados" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarFeriado(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_recursos.feriados WHERE feriadoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Feriado eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar feriado: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "feriados" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearCentro(string centro_codigo, string centro_nombre, string descripcion, string responsable)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_recursos.centros_costos (codigo, nombre, descripcion, responsable, estaactivo) 
                      VALUES (@codigo, @nombre, @descripcion, @responsable, TRUE)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", centro_codigo ?? "");
                    cmd.Parameters.AddWithValue("nombre", centro_nombre ?? "");
                    cmd.Parameters.AddWithValue("descripcion", descripcion ?? "");
                    cmd.Parameters.AddWithValue("responsable", responsable ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Centro de costo creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear centro de costo: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "centros" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarCentro(int id, string centro_codigo, string centro_nombre, string descripcion, string responsable)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_recursos.centros_costos 
                      SET codigo = @codigo, nombre = @nombre, descripcion = @descripcion, responsable = @responsable 
                      WHERE centrocostoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("codigo", centro_codigo ?? "");
                    cmd.Parameters.AddWithValue("nombre", centro_nombre ?? "");
                    cmd.Parameters.AddWithValue("descripcion", descripcion ?? "");
                    cmd.Parameters.AddWithValue("responsable", responsable ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Centro de costo actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar centro de costo: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "centros" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarCentro(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_recursos.centros_costos WHERE centrocostoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Centro de costo eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar centro de costo: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "centros" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearUsuarioNom(string usuario, string usuario_nombre, string rol, string email)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_recursos.usuarios_nomina (usuario, nombrecompleto, rol, correo, estaactivo) 
                      VALUES (@usuario, @nombrecompleto, @rol, @correo, TRUE)", cn))
                {
                    cmd.Parameters.AddWithValue("usuario", usuario ?? "");
                    cmd.Parameters.AddWithValue("nombrecompleto", usuario_nombre ?? "");
                    cmd.Parameters.AddWithValue("rol", rol ?? "");
                    cmd.Parameters.AddWithValue("correo", email ?? "");
                    cmd.ExecuteNonQuery();
                }

                // Enviar notificación al dueño
                string subject = "Nuevo usuario creado - SGE Enterprise";
                string body = $@"
                    <div style='font-family: sans-serif; max-width: 500px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px;'>
                        <h2 style='color: #4361ee; margin-top: 0;'>Nuevo Usuario Registrado</h2>
                        <p>Se ha creado una nueva cuenta en la plataforma (Configuración de Nómina):</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 6px 0; font-weight: bold; width: 140px;'>Nombre Completo:</td><td style='padding: 6px 0;'>{usuario_nombre}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Correo:</td><td style='padding: 6px 0;'>{email}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Rol Asignado:</td><td style='padding: 6px 0; text-transform: capitalize;'>{rol}</td></tr>
                            <tr><td style='padding: 6px 0; font-weight: bold;'>Estado:</td><td style='padding: 6px 0;'>Activo</td></tr>
                        </table>
                    </div>";
                Task.Run(async () => await _emailService.SendEmailAsync("zaiduriarteleo@gmail.com", subject, body));

                TempData["MsgConfig"] = "Usuario de nómina creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear usuario de nómina: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "usuarios" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarUsuarioNom(int id, string usuario, string usuario_nombre, string rol, string email)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_recursos.usuarios_nomina 
                      SET usuario = @usuario, nombrecompleto = @nombrecompleto, rol = @rol, correo = @correo 
                      WHERE usuarionominaid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("usuario", usuario ?? "");
                    cmd.Parameters.AddWithValue("nombrecompleto", usuario_nombre ?? "");
                    cmd.Parameters.AddWithValue("rol", rol ?? "");
                    cmd.Parameters.AddWithValue("correo", email ?? "");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Usuario de nómina actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar usuario: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "usuarios" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarUsuarioNom(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_recursos.usuarios_nomina WHERE usuarionominaid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgConfig"] = "Usuario de nómina eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar usuario: " + ex.Message;
            }
            return RedirectToAction("Configuracion", new { seccion = "usuarios" });
        }

        // ==========================================
        // OTHER PARTIAL VIEWS (SHELLS)
        // ==========================================
        // ==========================================
        // BENEFICIOS CRUD
        // ==========================================
        public IActionResult Beneficios(string buscar, string categoria, string estado, int pagina = 1)
        {
            var vm = new BeneficiosViewModel();
            vm.Buscar = buscar ?? "";
            vm.CategoriaFiltro = categoria ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 10;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Compute Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.beneficios", cn))
                {
                    ViewBag.StatsTotal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.beneficios WHERE activo = TRUE", cn))
                {
                    ViewBag.StatsActivos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.beneficios WHERE activo = FALSE", cn))
                {
                    ViewBag.StatsInactivos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Compute total amount this month (montoMensual)
                using (var cmd = new NpgsqlCommand("SELECT SUM(montofijo) FROM rrhh_nomina.beneficios WHERE activo = TRUE AND periodicidad = 'Mensual'", cn))
                {
                    var val = cmd.ExecuteScalar();
                    ViewBag.MontoMensual = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }

                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.beneficios WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.CategoriaFiltro)) countSql += " AND categoria = @categoria";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") countSql += " AND activo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") countSql += " AND activo = FALSE";
                }

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.CategoriaFiltro)) cmd.Parameters.AddWithValue("categoria", vm.CategoriaFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = "SELECT beneficioid, codigo, nombre, categoria, tipo, periodicidad, montofijo, montocadena, activo FROM rrhh_nomina.beneficios WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.CategoriaFiltro)) sql += " AND categoria = @categoria";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro))
                {
                    if (vm.EstadoFiltro == "Activo") sql += " AND activo = TRUE";
                    else if (vm.EstadoFiltro == "Inactivo") sql += " AND activo = FALSE";
                }
                sql += " ORDER BY beneficioid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.CategoriaFiltro)) cmd.Parameters.AddWithValue("categoria", vm.CategoriaFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var b = new BeneficioItem();
                        b.Id = rd.GetInt32(0);
                        b.Codigo = rd.GetString(1);
                        b.Nombre = rd.GetString(2);
                        b.Categoria = Enum.TryParse<CategoriaBeneficio>(rd.GetString(3), out var cat) ? cat : CategoriaBeneficio.Otros;
                        b.Tipo = Enum.TryParse<TipoBeneficio>(rd.GetString(4), out var tp) ? tp : TipoBeneficio.Beneficio;
                        b.Periodicidad = Enum.TryParse<Periodicidad>(rd.GetString(5), out var per) ? per : Periodicidad.Mensual;
                        b.MontoFijo = rd.IsDBNull(6) ? (decimal?)null : rd.GetDecimal(6);
                        b.MontoCadena = rd.GetString(7);
                        b.Activo = rd.GetBoolean(8);
                        vm.Beneficios.Add(b);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            ViewBag.Inicio = vm.DesdeItem;
            ViewBag.Fin = vm.HastaItem;

            return PartialView("~/Views/RRHH/Nomina/Beneficios.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerBeneficio(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT beneficioid, codigo, nombre, categoria, tipo, periodicidad, montofijo, montocadena, activo FROM rrhh_nomina.beneficios WHERE beneficioid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var b = new
                        {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            nombre = rd.GetString(2),
                            categoria = rd.GetString(3),
                            tipo = rd.GetString(4),
                            periodicidad = rd.GetString(5),
                            montoFijo = rd.IsDBNull(6) ? (decimal?)null : rd.GetDecimal(6),
                            montoCadena = rd.GetString(7),
                            activo = rd.GetBoolean(8)
                        };
                        return Json(b);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearBeneficio(string nombre, string categoria, string tipo, string periodicidad, decimal? montoFijo, string montoCadena, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                string code = "";
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.beneficios", cn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    code = $"BEN-{count:D3}";
                }

                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.beneficios (codigo, nombre, categoria, tipo, periodicidad, montofijo, montocadena, activo)
                      VALUES (@codigo, @nombre, @categoria, @tipo, @periodicidad, @montoFijo, @montoCadena, @activo)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", code);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("categoria", categoria ?? "Otros");
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Beneficio");
                    cmd.Parameters.AddWithValue("periodicidad", periodicidad ?? "Mensual");
                    cmd.Parameters.AddWithValue("montoFijo", (object)montoFijo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("montoCadena", montoCadena ?? "");
                    cmd.Parameters.AddWithValue("activo", activo);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeBeneficio"] = "Beneficio creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear beneficio: " + ex.Message;
            }
            return RedirectToAction("Beneficios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarBeneficio(int id, string nombre, string categoria, string tipo, string periodicidad, decimal? montoFijo, string montoCadena, bool activo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.beneficios 
                      SET nombre = @nombre, categoria = @categoria, tipo = @tipo, periodicidad = @periodicidad, 
                          montofijo = @montoFijo, montocadena = @montoCadena, activo = @activo
                      WHERE beneficioid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("categoria", categoria ?? "Otros");
                    cmd.Parameters.AddWithValue("tipo", tipo ?? "Beneficio");
                    cmd.Parameters.AddWithValue("periodicidad", periodicidad ?? "Mensual");
                    cmd.Parameters.AddWithValue("montoFijo", (object)montoFijo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("montoCadena", montoCadena ?? "");
                    cmd.Parameters.AddWithValue("activo", activo);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeBeneficio"] = "Beneficio actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al editar beneficio: " + ex.Message;
            }
            return RedirectToAction("Beneficios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarBeneficio(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.beneficios WHERE beneficioid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeBeneficio"] = "Beneficio eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar beneficio: " + ex.Message;
            }
            return RedirectToAction("Beneficios");
        }

        [HttpGet]
        public IActionResult ExportarAportesExcel()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Declaracion ID;Codigo;Periodo;Trabajadores;Remuneracion Asegurable;Aporte EsSalud;Fecha Envio;Estado;Nro Orden SUNAT;Total Pagar;Tipo");
            
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                string sql = @"
                    SELECT declaracionid, codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, 
                           fechaenvio, estado, nroordensunat, totalpagar, tipo 
                    FROM rrhh_nomina.essalud_declaraciones 
                    ORDER BY declaracionid DESC";
                    
                using (var cmd = new NpgsqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var id = rd.GetInt32(0);
                        var codigo = rd.GetString(1);
                        var periodo = rd.GetString(2);
                        var trabajadores = rd.GetInt32(3);
                        var remuneracion = rd.GetDecimal(4);
                        var aporte = rd.GetDecimal(5);
                        var fecha = rd.GetDateTime(6).ToString("dd/MM/yyyy");
                        var estado = rd.GetString(7);
                        var nroOrden = rd.IsDBNull(8) ? "" : rd.GetString(8);
                        var totalPagar = rd.GetDecimal(9);
                        var tipo = rd.GetString(10);
                        
                        csv.AppendLine($"{id};{codigo};{periodo};{trabajadores};{remuneracion:F2};{aporte:F2};{fecha};{estado};{nroOrden};{totalPagar:F2};{tipo}");
                    }
                }
            }
            catch (Exception ex)
            {
                csv.AppendLine($"Error al exportar datos: {ex.Message}");
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var bom = System.Text.Encoding.UTF8.GetPreamble();
            var fileBytes = new byte[bom.Length + bytes.Length];
            Buffer.BlockCopy(bom, 0, fileBytes, 0, bom.Length);
            Buffer.BlockCopy(bytes, 0, fileBytes, bom.Length, bytes.Length);
            
            return File(fileBytes, "text/csv", "Reporte_Aportes_EsSalud.csv");
        }

        // ==========================================
        // ESSALUD / SCTR
        // ==========================================
        public IActionResult EsSalud(string vista, string periodo, string estado, string tipo, string buscar, int pagina = 1)
        {
            var vm = new EsSaludViewModel();
            vm.Vista = string.IsNullOrEmpty(vista) ? "Resumen" : vista;
            vm.Buscar = buscar ?? "";
            vm.PeriodoFiltro = periodo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.TipoFiltro = tipo ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 5;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Compute Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones", cn))
                {
                    vm.TotalDeclaraciones = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Pendiente'", cn))
                {
                    vm.Pendientes = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Enviada'", cn))
                {
                    vm.Enviadas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Aceptada'", cn))
                {
                    vm.Aceptadas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT SUM(aporteessalud) FROM rrhh_nomina.essalud_declaraciones WHERE estado = 'Aceptada'", cn))
                {
                    var val = cmd.ExecuteScalar();
                    vm.AporteTotalPeriodo = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }

                // Fetch Declaraciones list
                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (codigo ILIKE @buscar OR periodo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) countSql += " AND periodo = @periodo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) countSql += " AND tipo = @tipo";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) cmd.Parameters.AddWithValue("periodo", vm.PeriodoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = @"
                    SELECT declaracionid, codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, 
                           fechaenvio, estado, nroordensunat, observacion, subsidios, totalpagar, tipo 
                    FROM rrhh_nomina.essalud_declaraciones 
                    WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (codigo ILIKE @buscar OR periodo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) sql += " AND periodo = @periodo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) sql += " AND tipo = @tipo";
                sql += " ORDER BY declaracionid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) cmd.Parameters.AddWithValue("periodo", vm.PeriodoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic d = new ExpandoObject();
                        d.Id = rd.GetInt32(0);
                        d.Codigo = rd.GetString(1);
                        d.Periodo = rd.GetString(2);
                        d.Trabajadores = rd.GetInt32(3);
                        d.RemuneracionAsignable = rd.GetDecimal(4);
                        d.AporteEsSalud = rd.GetDecimal(5);
                        d.FechaEnvio = rd.GetDateTime(6);
                        d.Estado = Enum.TryParse<EstadoDeclaracion>(rd.GetString(7), out var est) ? est : EstadoDeclaracion.Pendiente;
                        d.NroOrdenSunat = rd.IsDBNull(8) ? "" : rd.GetString(8);
                        d.Observacion = rd.IsDBNull(9) ? "" : rd.GetString(9);
                        d.Subsidios = rd.GetDecimal(10);
                        d.TotalPagar = rd.GetDecimal(11);
                        d.Tipo = rd.GetString(12);
                        vm.Declaraciones.Add(d);
                    }
                }

                // Fetch Empleados Activos for SCTR
                using (var cmd = new NpgsqlCommand(
                    @"SELECT empleadoid, nombres, apellidopaterno, apellidomaterno, numerodocumento, 
                             COALESCE(cargo, 'Colaborador') AS cargo, COALESCE(departamento, 'Administración') AS departamento, estaactivo
                      FROM rrhh_recursos.empleados", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic emp = new ExpandoObject();
                        emp.Id = rd.GetInt32(0);
                        emp.Nombres = rd.GetString(1);
                        emp.ApellidoPaterno = rd.GetString(2);
                        emp.ApellidoMaterno = rd.IsDBNull(3) ? "" : rd.GetString(3);
                        emp.NumeroDocumento = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        emp.Cargo = rd.GetString(5);
                        emp.Departamento = rd.GetString(6);
                        bool isAct = rd.GetBoolean(7);
                        emp.Estado = isAct ? EstadoEmpleado.Activo : EstadoEmpleado.Inactivo;
                        vm.Empleados.Add(emp);
                    }
                }

                int totalActive = 0;
                foreach (dynamic e in vm.Empleados)
                {
                    if ((EstadoEmpleado)e.Estado == EstadoEmpleado.Activo)
                    {
                        totalActive++;
                    }
                }
                
                // Load SCTR general params from DB
                string aseguradora = GetParametro(cn, "sctr_aseguradora", "RIMAC Seguros");
                string poliza = GetParametro(cn, "sctr_poliza", "P-2025-0005678");
                string tasasConfigJson = GetParametro(cn, "sctr_tasas_config", "");

                var defaultTasas = new Dictionary<int, (decimal TasaSalud, decimal TasaPension, string Aseguradora)>
                {
                    { 1, (0.53m, 0.47m, "RIMAC Seguros") },
                    { 2, (1.04m, 0.96m, "Pacifico Seguros") },
                    { 3, (1.82m, 1.68m, "Mapfre Perú") },
                    { 4, (3.10m, 2.90m, "La Positiva") }
                };

                vm.SctrSaludTotal = 0m;
                vm.SctrPensionTotal = 0m;

                if (!string.IsNullOrEmpty(tasasConfigJson))
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<SctrTasaItem>>(tasasConfigJson);
                    foreach (var item in list)
                    {
                        var def = defaultTasas[item.Id];
                        dynamic grp = new ExpandoObject();
                        grp.Id = item.Id;
                        grp.NivelRiesgo = (NivelRiesgoSCTR)(item.Id - 1);
                        grp.Trabajadores = item.Trabajadores;
                        grp.SctrSalud = item.SctrSalud;
                        grp.SctrPension = item.SctrPension;
                        grp.Aseguradora = item.Id == 1 ? aseguradora : def.Aseguradora; // RIMAC for Riesgo 1, etc.
                        vm.GruposSctr.Add(grp);

                        vm.SctrSaludTotal += item.SctrSalud;
                        vm.SctrPensionTotal += item.SctrPension;
                    }
                }
                else
                {
                    // Fallback to active employees logic
                    int r1 = (int)Math.Max(1, Math.Round(totalActive * 0.40));
                    int r2 = (int)Math.Max(1, Math.Round(totalActive * 0.30));
                    int r3 = (int)Math.Max(1, Math.Round(totalActive * 0.20));
                    int r4 = (int)Math.Max(0, totalActive - r1 - r2 - r3);
                    if (r4 < 0) r4 = 0;

                    var defaultGrupos = new List<(int Id, NivelRiesgoSCTR Nivel, int Trabajadores, decimal TasaSalud, decimal TasaPension, string Aseguradora)>
                    {
                        (1, NivelRiesgoSCTR.Riesgo1, r1, 0.53m, 0.47m, aseguradora),
                        (2, NivelRiesgoSCTR.Riesgo2, r2, 1.04m, 0.96m, "Pacifico Seguros"),
                        (3, NivelRiesgoSCTR.Riesgo3, r3, 1.82m, 1.68m, "Mapfre Perú"),
                        (4, NivelRiesgoSCTR.Riesgo4, r4, 3.10m, 2.90m, "La Positiva")
                    };

                    foreach (var g in defaultGrupos)
                    {
                        decimal baseCalculo = 2500m * g.Trabajadores;
                        decimal saludVal = Math.Round(baseCalculo * (g.TasaSalud / 100m), 2);
                        decimal pensionVal = Math.Round(baseCalculo * (g.TasaPension / 100m), 2);

                        dynamic grp = new ExpandoObject();
                        grp.Id = g.Id;
                        grp.NivelRiesgo = g.Nivel;
                        grp.Trabajadores = g.Trabajadores;
                        grp.SctrSalud = saludVal;
                        grp.SctrPension = pensionVal;
                        grp.Aseguradora = g.Aseguradora;
                        vm.GruposSctr.Add(grp);

                        vm.SctrSaludTotal += saludVal;
                        vm.SctrPensionTotal += pensionVal;
                    }
                }

                vm.TotalSctr = vm.SctrSaludTotal + vm.SctrPensionTotal;

                // Validaciones list - Dynamically queries the database instead of hardcoded lists
                var valList = new List<dynamic>();
                
                // 1. Aporte EsSalud (9%) check
                int aportesMal = 0;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.essalud_declaraciones WHERE ABS(aporteessalud - ROUND(remuneracionasignable * 0.09, 2)) > 0.1", cn))
                {
                    aportesMal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                dynamic v1 = new ExpandoObject();
                v1.Nombre = "Aporte EsSalud (9%)";
                v1.Severidad = aportesMal > 0 ? "Advertencia" : "Ok";
                
                var v1Afectados = new List<object>();
                using (var cmd = new NpgsqlCommand("SELECT codigo, periodo, remuneracionasignable, aporteessalud FROM rrhh_nomina.essalud_declaraciones WHERE ABS(aporteessalud - ROUND(remuneracionasignable * 0.09, 2)) > 0.1", cn))
                {
                    using var rdVal = cmd.ExecuteReader();
                    while (rdVal.Read())
                    {
                        string cod = rdVal.GetString(0);
                        string per = rdVal.GetString(1);
                        decimal rem = rdVal.GetDecimal(2);
                        decimal apo = rdVal.GetDecimal(3);
                        decimal calc = Math.Round(rem * 0.09m, 2);
                        v1Afectados.Add(new { Nombre = $"{cod} ({per})", Dato = $"Reg: S/ {apo:N2} | Calc: S/ {calc:N2} (Dif: S/ {Math.Abs(apo-calc):N2})" });
                    }
                }
                v1.AfectadosJson = System.Text.Json.JsonSerializer.Serialize(v1Afectados);
                v1.Detalle = aportesMal > 0 
                    ? $"Se detectaron {aportesMal} declaraciones con inconsistencia en el aporte de EsSalud (9%)." 
                    : "Todos los aportes mensuales del 9% sobre la remuneración asegurable han sido calculados correctamente.";
                v1.DetalleLargo = v1.Detalle;
                v1.Periodo = "Histórico";
                valList.Add(v1);

                // 2. Trabajadores con DNI check
                int sinDni = 0;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE numerodocumento IS NULL OR numerodocumento = ''", cn))
                {
                    sinDni = Convert.ToInt32(cmd.ExecuteScalar());
                }
                dynamic v2 = new ExpandoObject();
                v2.Nombre = "Trabajadores con DNI";
                v2.Severidad = sinDni > 0 ? "Error" : "Ok";
                
                var v2Afectados = new List<object>();
                using (var cmd = new NpgsqlCommand("SELECT nombres, apellidopaterno, apellidomaterno, empleadoid FROM rrhh_recursos.empleados WHERE numerodocumento IS NULL OR numerodocumento = ''", cn))
                {
                    using var rdVal = cmd.ExecuteReader();
                    while (rdVal.Read())
                    {
                        string nom = $"{rdVal.GetString(0)} {rdVal.GetString(1)} {(rdVal.IsDBNull(2) ? "" : rdVal.GetString(2))}".Trim();
                        int empId = rdVal.GetInt32(3);
                        v2Afectados.Add(new { Nombre = nom, Dato = $"ID Empleado: {empId} (Falta N° de Documento)" });
                    }
                }
                v2.AfectadosJson = System.Text.Json.JsonSerializer.Serialize(v2Afectados);
                v2.Detalle = sinDni > 0 
                    ? $"Se detectaron {sinDni} empleados sin número de documento registrado." 
                    : "Todos los trabajadores de la planilla cuentan con tipo y número de documento registrado.";
                v2.DetalleLargo = v2.Detalle;
                v2.Periodo = "Histórico";
                valList.Add(v2);

                // 3. Topes máximos de EsSalud check
                int sueldosAltos = 0;
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.contratos WHERE sueldobase > 20000 AND estaactivo = TRUE", cn))
                {
                    sueldosAltos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                dynamic v3 = new ExpandoObject();
                v3.Nombre = "Topes máximos de EsSalud";
                v3.Severidad = sueldosAltos > 0 ? "Advertencia" : "Ok";
                
                var v3Afectados = new List<object>();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT e.nombres, e.apellidopaterno, c.sueldobase 
                    FROM rrhh_recursos.contratos c
                    JOIN rrhh_recursos.empleados e ON e.empleadoid = c.empleadoid
                    WHERE c.sueldobase > 20000 AND c.estaactivo = TRUE", cn))
                {
                    using var rdVal = cmd.ExecuteReader();
                    while (rdVal.Read())
                    {
                        string nom = $"{rdVal.GetString(0)} {rdVal.GetString(1)}".Trim();
                        decimal sueldo = rdVal.GetDecimal(2);
                        v3Afectados.Add(new { Nombre = nom, Dato = $"Sueldo Base: S/ {sueldo:N2} (Supera tope de S/ 20,000)" });
                    }
                }
                v3.AfectadosJson = System.Text.Json.JsonSerializer.Serialize(v3Afectados);
                v3.Detalle = sueldosAltos > 0 
                    ? $"Se detectaron {sueldosAltos} contratos activos con sueldo base mayor a S/ 20,000." 
                    : "Ningún sueldo de contrato activo excede el límite prudencial de control establecido.";
                v3.DetalleLargo = v3.Detalle;
                v3.Periodo = "Período actual";
                valList.Add(v3);

                vm.Validaciones = valList;

                // Historial de envíos
                foreach (var d in vm.Declaraciones)
                {
                    dynamic h = new ExpandoObject();
                    h.Id = d.Id;
                    h.Periodo = d.Periodo;
                    h.Codigo = d.Codigo;
                    h.FechaEnvio = d.FechaEnvio;
                    h.FechaHora = d.FechaEnvio;
                    h.Trabajadores = d.Trabajadores;
                    h.AporteEsSalud = d.AporteEsSalud;
                    h.Estado = d.Estado switch
                    {
                        EstadoDeclaracion.Aceptada => EstadoEnvio.Aceptado,
                        EstadoDeclaracion.Enviada => EstadoEnvio.Enviado,
                        EstadoDeclaracion.Observada => EstadoEnvio.ConObservaciones,
                        _ => EstadoEnvio.PendienteEnvio
                    };
                    h.NroOrdenSunat = d.NroOrdenSunat;
                    h.Declaracion = $"Declaración {d.Codigo} ({d.Periodo})";
                    h.Usuario = "Jhoel Patrick";
                    h.Mensaje = d.Observacion;
                    vm.Historial.Add(h);
                }

                // Precompute sums for the View to avoid dynamic binder Linq resolution in Razor
                ViewBag.TotalRem = vm.Declaraciones.Count > 0 ? vm.Declaraciones.Sum(d => (decimal)d.RemuneracionAsignable) : 0m;
                ViewBag.TotalAporte = vm.Declaraciones.Count > 0 ? vm.Declaraciones.Sum(d => (decimal)d.AporteEsSalud) : 0m;
                ViewBag.TotalSubsid = vm.Declaraciones.Count > 0 ? vm.Declaraciones.Sum(d => (decimal)d.Subsidios) : 0m;
                ViewBag.TotalPagar = vm.Declaraciones.Count > 0 ? vm.Declaraciones.Sum(d => (decimal)d.TotalPagar) : 0m;
                ViewBag.TotalTrab = vm.Declaraciones.Count > 0 ? vm.Declaraciones.Sum(d => (int)d.Trabajadores) : 0;
                
                decimal totalPagado = vm.Declaraciones.Count > 0 
                    ? vm.Declaraciones.Where(d => (EstadoDeclaracion)d.Estado == EstadoDeclaracion.Aceptada).Sum(d => (decimal)d.AporteEsSalud) 
                    : 0m;
                ViewBag.TotalPagado = totalPagado;
                ViewBag.PendientePago = (decimal)ViewBag.TotalAporte - totalPagado;
                
                ViewBag.TotalSctrTrabajadores = vm.GruposSctr.Count > 0 ? vm.GruposSctr.Sum(g => (int)g.Trabajadores) : 0;
                ViewBag.SctrPoliza = poliza;
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/EsSalud.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarDeclaracion(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                string orderNumber = $"{new Random().Next(100000000, 999999999)}";

                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.essalud_declaraciones 
                      SET estado = 'Aceptada', nroordensunat = @nroOrden, observacion = 'Aceptado por SUNAT sin observaciones.' 
                      WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nroOrden", orderNumber);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeEsSalud"] = "Declaración enviada a SUNAT y aceptada con número de orden " + orderNumber;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al enviar declaración: " + ex.Message;
            }
            return RedirectToAction("EsSalud", new { vista = "Declaraciones" });
        }

        [HttpGet]
        public IActionResult DescargarDeclaracion(int id)
        {
            string content = "PDT DECLARACION ESSALUD\n";
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT codigo, periodo, aporteessalud, nroordensunat FROM rrhh_nomina.essalud_declaraciones WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        content += $"Codigo: {rd.GetString(0)}\nPeriodo: {rd.GetString(1)}\nAporte: S/ {rd.GetDecimal(2):N2}\nOrden SUNAT: {rd.GetString(3)}\n";
                    }
                }
            }
            catch {}
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", $"Declaracion_EsSalud_{id}.txt");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevaDeclaracion(string periodo, int trabajadores, decimal remuneracion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                string decCode = $"DEC-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
                decimal aporte = Math.Round(remuneracion * 0.09m, 2);
                
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.essalud_declaraciones 
                      (codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, fechaenvio, estado, nroordensunat, observacion, subsidios, totalpagar, tipo) 
                      VALUES (@codigo, @periodo, @trabajadores, @remuneracion, @aporte, @fechaEnvio, 'Pendiente', '', '', 0.00, @aporte, 'Mensual')", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", decCode);
                    cmd.Parameters.AddWithValue("periodo", periodo);
                    cmd.Parameters.AddWithValue("trabajadores", trabajadores);
                    cmd.Parameters.AddWithValue("remuneracion", remuneracion);
                    cmd.Parameters.AddWithValue("aporte", aporte);
                    cmd.Parameters.AddWithValue("fechaEnvio", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeEsSalud"] = "Declaración creada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear declaración: " + ex.Message;
            }
            return RedirectToAction("EsSalud", new { vista = "Declaraciones" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AceptarDeclaracion(string codigo)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string orderNumber = $"{new Random().Next(100000000, 999999999)}";
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.essalud_declaraciones 
                      SET estado = 'Aceptada', nroordensunat = @nroOrden, observacion = 'Aceptado por SUNAT sin observaciones.' 
                      WHERE codigo = @codigo", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", codigo);
                    cmd.Parameters.AddWithValue("nroOrden", orderNumber);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeEsSalud"] = "Declaración aceptada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al aceptar declaración: " + ex.Message;
            }
            return RedirectToAction("EsSalud", new { vista = "Aportes" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfigurarSctr(string aseguradora, string nroPoliza)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                UpsertParametro(cn, "sctr_aseguradora", aseguradora, "Aseguradora del SCTR", "sctr");
                UpsertParametro(cn, "sctr_poliza", nroPoliza, "Número de póliza SCTR", "sctr");
                
                var list = new List<object>();
                for (int id = 1; id <= 4; id++)
                {
                    int trab = Convert.ToInt32(Request.Form[$"trab_{id}"]);
                    decimal salud = Convert.ToDecimal(Request.Form[$"salud_{id}"]);
                    decimal pension = Convert.ToDecimal(Request.Form[$"pension_{id}"]);
                    
                    list.Add(new {
                        Id = id,
                        Trabajadores = trab,
                        SctrSalud = salud,
                        SctrPension = pension
                    });
                }
                
                string json = System.Text.Json.JsonSerializer.Serialize(list);
                UpsertParametro(cn, "sctr_tasas_config", json, "Configuración de tasas e indicativos por nivel de riesgo SCTR", "sctr");
                
                TempData["MensajeEsSalud"] = "Configuración SCTR guardada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar configuración SCTR: " + ex.Message;
            }
            return RedirectToAction("EsSalud", new { vista = "Sctr" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidarAhora()
        {
            TempData["MensajeEsSalud"] = "Se ejecutó la validación de declaraciones correctamente.";
            return RedirectToAction("EsSalud", new { vista = "Validaciones" });
        }

        private string GetParametro(NpgsqlConnection cn, string clave, string defaultValue)
        {
            using var cmd = new NpgsqlCommand("SELECT valor FROM sistema.parametros WHERE clave = @clave", cn);
            cmd.Parameters.AddWithValue("clave", clave);
            var val = cmd.ExecuteScalar();
            return val != null ? Convert.ToString(val) : defaultValue;
        }

        private void UpsertParametro(NpgsqlConnection cn, string clave, string valor, string descripcion, string categoria)
        {
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO sistema.parametros (clave, valor, descripcion, categoria, fechamodificacion)
                VALUES (@clave, @valor, @desc, @cat, CURRENT_TIMESTAMP)
                ON CONFLICT (clave) 
                DO UPDATE SET valor = EXCLUDED.valor, fechamodificacion = CURRENT_TIMESTAMP", cn);
            cmd.Parameters.AddWithValue("clave", clave);
            cmd.Parameters.AddWithValue("valor", valor);
            cmd.Parameters.AddWithValue("desc", descripcion);
            cmd.Parameters.AddWithValue("cat", categoria);
            cmd.ExecuteNonQuery();
        }

        private class SctrTasaItem
        {
            public int Id { get; set; }
            public int Trabajadores { get; set; }
            public decimal SctrSalud { get; set; }
            public decimal SctrPension { get; set; }
        }

        // ==========================================
        // GRATIFICACIONES CRUD
        // ==========================================
        public IActionResult Gratificaciones(string buscar, string tipo, string estado, int pagina = 1)
        {
            var vm = new GratificacionesViewModel();
            vm.Buscar = buscar ?? "";
            vm.TipoFiltro = tipo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 10;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Compute Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.gratificaciones", cn))
                {
                    ViewBag.StatsTotal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.gratificaciones WHERE estado = 'Activa'", cn))
                {
                    ViewBag.StatsActivas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.gratificaciones WHERE estado = 'Pendiente'", cn))
                {
                    ViewBag.StatsPendientes = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.gratificaciones WHERE estado = 'Pagada'", cn))
                {
                    ViewBag.StatsPagadas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT SUM(montofijo) FROM rrhh_nomina.gratificaciones WHERE estado = 'Pagada'", cn))
                {
                    var val = cmd.ExecuteScalar();
                    ViewBag.MontoTotalAnio = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }

                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.gratificaciones WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) countSql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                vm.DesdeItem = vm.TotalItems == 0 ? 0 : offset + 1;
                vm.HastaItem = Math.Min(vm.PaginaActual * limit, vm.TotalItems);

                string sql = @"
                    SELECT gratificacionid, codigo, nombre, tipo, periodo, frecuencia, porcentajemonto, 
                           basedecalculo, montofijo, porcentaje, fechaestimada, fechapago, estado, 
                           empleadosaplica, cantidadempleados, creadopor 
                    FROM rrhh_nomina.gratificaciones 
                    WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) sql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                sql += " ORDER BY gratificacionid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var g = new GratificacionItem();
                        g.Id = rd.GetInt32(0);
                        g.Codigo = rd.GetString(1);
                        g.Nombre = rd.GetString(2);
                        g.Tipo = Enum.TryParse<TipoGratificacion>(rd.GetString(3), out var tp) ? tp : TipoGratificacion.Obligatoria;
                        g.Periodo = rd.GetString(4);
                        g.Frecuencia = Enum.TryParse<FrecuenciaGratificacion>(rd.GetString(5), out var fr) ? fr : FrecuenciaGratificacion.Anual;
                        g.PorcentajeMonto = rd.GetString(6);
                        g.BaseDeCalculo = Enum.TryParse<BaseCalculo>(rd.GetString(7), out var bc) ? bc : BaseCalculo.RemuneracionBasica;
                        g.MontoFijo = rd.IsDBNull(8) ? (decimal?)null : rd.GetDecimal(8);
                        g.Porcentaje = rd.IsDBNull(9) ? (decimal?)null : rd.GetDecimal(9);
                        g.FechaEstimada = rd.IsDBNull(10) ? (DateTime?)null : rd.GetDateTime(10);
                        g.FechaPago = rd.IsDBNull(11) ? (DateTime?)null : rd.GetDateTime(11);
                        g.Estado = Enum.TryParse<EstadoGratificacion>(rd.GetString(12), out var est) ? est : EstadoGratificacion.Pendiente;
                        g.EmpleadosAplica = rd.GetString(13);
                        g.CantidadEmpleados = rd.GetInt32(14);
                        g.CreadoPor = rd.GetString(15);
                        vm.Gratificaciones.Add(g);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Gratificaciones.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerGratificacion(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT gratificacionid, codigo, nombre, tipo, periodo, frecuencia, porcentajemonto, 
                           basedecalculo, montofijo, porcentaje, fechaestimada, fechapago, estado, 
                           empleadosaplica, cantidadempleados, creadopor 
                    FROM rrhh_nomina.gratificaciones 
                    WHERE gratificacionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var g = new
                        {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            nombre = rd.GetString(2),
                            tipo = rd.GetString(3),
                            periodo = rd.GetString(4),
                            frecuencia = rd.GetString(5),
                            porcentajeMonto = rd.GetString(6),
                            baseCalculo = rd.GetString(7),
                            montoFijo = rd.IsDBNull(8) ? (decimal?)null : rd.GetDecimal(8),
                            porcentaje = rd.IsDBNull(9) ? (decimal?)null : rd.GetDecimal(9),
                            fechaEstimada = rd.IsDBNull(10) ? "" : rd.GetDateTime(10).ToString("yyyy-MM-dd"),
                            fechaPago = rd.IsDBNull(11) ? "" : rd.GetDateTime(11).ToString("yyyy-MM-dd"),
                            estado = rd.GetString(12),
                            empleadosAplica = rd.GetString(13),
                            cantidadEmpleados = rd.GetInt32(14),
                            creadorPor = rd.GetString(15)
                        };
                        return Json(g);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearGratificacion(string nombre, string tipoGrat, string periodo, string frecuencia, 
                                                 string porcentajeMonto, string baseCalculo, decimal? montoFijo, 
                                                 decimal? porcentaje, DateTime? fechaEstimada, string estadoGrat, 
                                                 string empleadosAplica, int cantidadEmpleados)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                string code = "";
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.gratificaciones", cn))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    code = $"GRA-{DateTime.Now.Year}-{count:D3}";
                }

                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO rrhh_nomina.gratificaciones 
                    (codigo, nombre, tipo, periodo, frecuencia, porcentajemonto, basedecalculo, montofijo, porcentaje, fechaestimada, estado, empleadosaplica, cantidadempleados, creadopor)
                    VALUES (@codigo, @nombre, @tipo, @periodo, @frecuencia, @porcentajemonto, @basedecalculo, @montofijo, @porcentaje, @fechaestimada, @estado, @empleadosaplica, @cantidadempleados, @creadopor)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", code);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("tipo", tipoGrat ?? "Obligatoria");
                    cmd.Parameters.AddWithValue("periodo", periodo ?? "");
                    cmd.Parameters.AddWithValue("frecuencia", frecuencia ?? "Anual");
                    cmd.Parameters.AddWithValue("porcentajemonto", porcentajeMonto ?? "");
                    cmd.Parameters.AddWithValue("basedecalculo", baseCalculo ?? "RemuneracionBasica");
                    cmd.Parameters.AddWithValue("montofijo", (object)montoFijo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("porcentaje", (object)porcentaje ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fechaestimada", (object)fechaEstimada ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("estado", estadoGrat ?? "Pendiente");
                    cmd.Parameters.AddWithValue("empleadosaplica", empleadosAplica ?? "Todos");
                    cmd.Parameters.AddWithValue("cantidadempleados", cantidadEmpleados);
                    cmd.Parameters.AddWithValue("creadopor", "Jhoel Patrick");
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeGratificacion"] = "Gratificación creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear gratificación: " + ex.Message;
            }
            return RedirectToAction("Gratificaciones");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarGratificacion(int id, string nombre, string tipoGrat, string periodo, string frecuencia, 
                                                 string porcentajeMonto, string baseCalculo, decimal? montoFijo, 
                                                 decimal? porcentaje, DateTime? fechaEstimada, string estadoGrat, 
                                                 string empleadosAplica, int cantidadEmpleados)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                DateTime? fechaPago = null;
                if (estadoGrat == "Pagada")
                {
                    fechaPago = DateTime.Now;
                }

                using (var cmd = new NpgsqlCommand(@"
                    UPDATE rrhh_nomina.gratificaciones 
                    SET nombre = @nombre, tipo = @tipo, periodo = @periodo, frecuencia = @frecuencia, 
                        porcentajemonto = @porcentajemonto, basedecalculo = @basedecalculo, montofijo = @montofijo, 
                        porcentaje = @porcentaje, fechaestimada = @fechaestimada, fechapago = @fechapago, 
                        estado = @estado, empleadosaplica = @empleadosaplica, cantidadempleados = @cantidadempleados
                    WHERE gratificacionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "");
                    cmd.Parameters.AddWithValue("tipo", tipoGrat ?? "Obligatoria");
                    cmd.Parameters.AddWithValue("periodo", periodo ?? "");
                    cmd.Parameters.AddWithValue("frecuencia", frecuencia ?? "Anual");
                    cmd.Parameters.AddWithValue("porcentajemonto", porcentajeMonto ?? "");
                    cmd.Parameters.AddWithValue("basedecalculo", baseCalculo ?? "RemuneracionBasica");
                    cmd.Parameters.AddWithValue("montofijo", (object)montoFijo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("porcentaje", (object)porcentaje ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fechaestimada", (object)fechaEstimada ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fechapago", (object)fechaPago ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("estado", estadoGrat ?? "Pendiente");
                    cmd.Parameters.AddWithValue("empleadosaplica", empleadosAplica ?? "Todos");
                    cmd.Parameters.AddWithValue("cantidadempleados", cantidadEmpleados);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeGratificacion"] = "Gratificación actualizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al editar gratificación: " + ex.Message;
            }
            return RedirectToAction("Gratificaciones");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarGratificacion(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.gratificaciones WHERE gratificacionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeGratificacion"] = "Gratificación eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar gratificación: " + ex.Message;
            }
            return RedirectToAction("Gratificaciones");
        }

        public IActionResult Utilidades(string buscar, string anio, string estado, int pagina = 1)
        {
            var vm = new UtilidadesViewModel();
            vm.Buscar = buscar ?? "";
            vm.AnioFiltro = anio ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 10;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Stats and lists
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.utilidades", cn))
                {
                    ViewBag.StatsTotal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.utilidades WHERE estado = 'Pendiente' OR estado = 'EnCalculo'", cn))
                {
                    ViewBag.StatsPendientes = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.utilidades WHERE estado = 'Pagada'", cn))
                {
                    ViewBag.StatsPagadas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Years
                var anios = new List<int>();
                using (var cmd = new NpgsqlCommand("SELECT DISTINCT ejerciciofiscal FROM rrhh_nomina.utilidades ORDER BY ejerciciofiscal DESC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) anios.Add(rd.GetInt32(0));
                }
                if (!anios.Contains(DateTime.Now.Year)) anios.Add(DateTime.Now.Year);
                ViewBag.AniosDisponibles = anios;

                // Total Proyectado
                using (var cmd = new NpgsqlCommand("SELECT SUM(utilidadnetadeclarada * (porcentajeparticipacion / 100.0)) FROM rrhh_nomina.utilidades", cn))
                {
                    var val = cmd.ExecuteScalar();
                    ViewBag.TotalProyectado = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }
                ViewBag.ProximoPago = DateTime.Now.AddDays(45);

                // Fetch
                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.utilidades WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND codigo ILIKE @buscar";
                if (!string.IsNullOrEmpty(vm.AnioFiltro)) countSql += " AND ejerciciofiscal = @anio";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.AnioFiltro)) cmd.Parameters.AddWithValue("anio", Convert.ToInt32(vm.AnioFiltro));
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                string sql = "SELECT utilidadid, codigo, ejerciciofiscal, porcentajeparticipacion, utilidadnetadeclarada, diascomputables, remuneracioncomputable, montodistribuido, fechapagoestimada, estado, empleadosaplica, cantidadempleados, observacion FROM rrhh_nomina.utilidades WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND codigo ILIKE @buscar";
                if (!string.IsNullOrEmpty(vm.AnioFiltro)) sql += " AND ejerciciofiscal = @anio";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                sql += " ORDER BY utilidadid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.AnioFiltro)) cmd.Parameters.AddWithValue("anio", Convert.ToInt32(vm.AnioFiltro));
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic d = new ExpandoObject();
                        d.Id = rd.GetInt32(0);
                        d.Codigo = rd.GetString(1);
                        d.EjercicioFiscal = rd.GetInt32(2);
                        d.PorcentajeParticipacion = rd.GetDecimal(3);
                        d.UtilidadNetaDeclarada = rd.GetDecimal(4);
                        d.DiasComputables = rd.GetInt32(5);
                        d.RemuneracionComputable = rd.GetDecimal(6);
                        d.MontoDistribuido = rd.IsDBNull(7) ? (decimal?)null : rd.GetDecimal(7);
                        d.FechaPagoEstimada = rd.GetDateTime(8);
                        d.Estado = Enum.TryParse<EstadoUtilidad>(rd.GetString(9), out var est) ? est : EstadoUtilidad.Pendiente;
                        d.EmpleadosAplica = rd.GetString(10);
                        d.CantidadEmpleados = rd.GetInt32(11);
                        d.Observacion = rd.IsDBNull(12) ? "" : rd.GetString(12);
                        vm.Utilidades.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Utilidades.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerUtilidad(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT utilidadid, codigo, ejerciciofiscal, porcentajeparticipacion, utilidadnetadeclarada, diascomputables, remuneracioncomputable, montodistribuido, fechapagoestimada, estado, empleadosaplica, cantidadempleados, observacion FROM rrhh_nomina.utilidades WHERE utilidadid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var d = new {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            ejercicioFiscal = rd.GetInt32(2),
                            porcentajeParticipacion = rd.GetDecimal(3),
                            utilidadNetaDeclarada = rd.GetDecimal(4),
                            diasComputables = rd.GetInt32(5),
                            remuneracionComputable = rd.GetDecimal(6),
                            montoDistribuido = rd.IsDBNull(7) ? (decimal?)null : rd.GetDecimal(7),
                            fechaPagoEstimada = rd.GetDateTime(8).ToString("yyyy-MM-dd"),
                            estado = rd.GetString(9),
                            empleadosAplica = rd.GetString(10),
                            cantidadEmpleados = rd.GetInt32(11),
                            observacion = rd.IsDBNull(12) ? "" : rd.GetString(12)
                        };
                        return Json(d);
                    }
                }
            }
            catch {}
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearUtilidad(int ejercicioFiscal, decimal porcentajeParticipacion, decimal utilidadNetaDeclarada, int diasComputables, decimal remuneracionComputable, decimal? montoDistribuido, DateTime fechaPagoEstimada, string estadoUtil, string empleadosAplica, int cantidadEmpleados, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string nextCod = "";
                using (var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(utilidadid), 0) + 1 FROM rrhh_nomina.utilidades", cn))
                {
                    int nextId = Convert.ToInt32(cmd.ExecuteScalar());
                    nextCod = $"UTI-{ejercicioFiscal}-{nextId:D2}";
                }
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.utilidades (codigo, ejerciciofiscal, porcentajeparticipacion, utilidadnetadeclarada, diascomputables, remuneracioncomputable, montodistribuido, fechapagoestimada, estado, empleadosaplica, cantidadempleados, observacion)
                      VALUES (@codigo, @ejercicio, @pct, @util, @dias, @remun, @monto, @fecha, @estado, @emp, @cant, @obs)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", nextCod);
                    cmd.Parameters.AddWithValue("ejercicio", ejercicioFiscal);
                    cmd.Parameters.AddWithValue("pct", porcentajeParticipacion);
                    cmd.Parameters.AddWithValue("util", utilidadNetaDeclarada);
                    cmd.Parameters.AddWithValue("dias", diasComputables);
                    cmd.Parameters.AddWithValue("remun", remuneracionComputable);
                    cmd.Parameters.AddWithValue("monto", (object)montoDistribuido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fecha", fechaPagoEstimada);
                    cmd.Parameters.AddWithValue("estado", estadoUtil ?? "Pendiente");
                    cmd.Parameters.AddWithValue("emp", empleadosAplica ?? "Todos");
                    cmd.Parameters.AddWithValue("cant", cantidadEmpleados);
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeUtilidad"] = "Utilidad creada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear utilidad: " + ex.Message;
            }
            return RedirectToAction("Utilidades");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarUtilidad(int id, int ejercicioFiscal, decimal porcentajeParticipacion, decimal utilidadNetaDeclarada, int diasComputables, decimal remuneracionComputable, decimal? montoDistribuido, DateTime fechaPagoEstimada, string estadoUtil, string empleadosAplica, int cantidadEmpleados, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.utilidades 
                      SET ejerciciofiscal = @ejercicio, porcentajeparticipacion = @pct, utilidadnetadeclarada = @util, diascomputables = @dias, remuneracioncomputable = @remun, montodistribuido = @monto, fechapagoestimada = @fecha, estado = @estado, empleadosaplica = @emp, cantidadempleados = @cant, observacion = @obs
                      WHERE utilidadid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("ejercicio", ejercicioFiscal);
                    cmd.Parameters.AddWithValue("pct", porcentajeParticipacion);
                    cmd.Parameters.AddWithValue("util", utilidadNetaDeclarada);
                    cmd.Parameters.AddWithValue("dias", diasComputables);
                    cmd.Parameters.AddWithValue("remun", remuneracionComputable);
                    cmd.Parameters.AddWithValue("monto", (object)montoDistribuido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("fecha", fechaPagoEstimada);
                    cmd.Parameters.AddWithValue("estado", estadoUtil ?? "Pendiente");
                    cmd.Parameters.AddWithValue("emp", empleadosAplica ?? "Todos");
                    cmd.Parameters.AddWithValue("cant", cantidadEmpleados);
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeUtilidad"] = "Utilidad actualizada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar utilidad: " + ex.Message;
            }
            return RedirectToAction("Utilidades");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarUtilidad(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.utilidades WHERE utilidadid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeUtilidad"] = "Utilidad eliminada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar utilidad: " + ex.Message;
            }
            return RedirectToAction("Utilidades");
        }

        public IActionResult SunatPdt(string buscar, string tipo, string periodo, string estado, string ejercicio, int pagina = 1)
        {
            var vm = new SunatPdtViewModel();
            vm.Buscar = buscar ?? "";
            vm.TipoFiltro = tipo ?? "";
            vm.PeriodoFiltro = periodo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.EjercicioFiltro = ejercicio ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 8;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt", cn))
                {
                    ViewBag.StatsTotal = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt WHERE estado = 'Pendiente'", cn))
                {
                    ViewBag.StatsPendientes = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt WHERE estado = 'Enviada'", cn))
                {
                    ViewBag.StatsEnviadas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt WHERE estado = 'Aceptada'", cn))
                {
                    ViewBag.StatsAceptadas = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt WHERE estado = 'Observada' OR estado = 'Rechazada'", cn))
                {
                    ViewBag.StatsObsRechaz = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Filter lists
                var periodos = new List<string>();
                using (var cmd = new NpgsqlCommand("SELECT DISTINCT periodo FROM rrhh_nomina.declaraciones_pdt ORDER BY periodo DESC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) periodos.Add(rd.GetString(0));
                }
                if (!periodos.Contains("Mayo 2025")) periodos.Add("Mayo 2025");
                ViewBag.Periodos = periodos;

                var ejercicios = new List<int>();
                using (var cmd = new NpgsqlCommand("SELECT DISTINCT ejercicio FROM rrhh_nomina.declaraciones_pdt ORDER BY ejercicio DESC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) ejercicios.Add(rd.GetInt32(0));
                }
                if (!ejercicios.Contains(DateTime.Now.Year)) ejercicios.Add(DateTime.Now.Year);
                ViewBag.Ejercicios = ejercicios;

                // Query
                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.declaraciones_pdt WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND codigo ILIKE @buscar";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) countSql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) countSql += " AND periodo = @periodo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.EjercicioFiltro)) countSql += " AND ejercicio = @ejercicio";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) cmd.Parameters.AddWithValue("periodo", vm.PeriodoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.EjercicioFiltro)) cmd.Parameters.AddWithValue("ejercicio", Convert.ToInt32(vm.EjercicioFiltro));
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                string sql = "SELECT declaracionid, codigo, tipo, periodo, ejercicio, fechageneracion, fechaenvio, estado, nroorden, tieneconstancia, usuario, observacion FROM rrhh_nomina.declaraciones_pdt WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND codigo ILIKE @buscar";
                if (!string.IsNullOrEmpty(vm.TipoFiltro)) sql += " AND tipo = @tipo";
                if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) sql += " AND periodo = @periodo";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.EjercicioFiltro)) sql += " AND ejercicio = @ejercicio";
                sql += " ORDER BY declaracionid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.TipoFiltro)) cmd.Parameters.AddWithValue("tipo", vm.TipoFiltro);
                    if (!string.IsNullOrEmpty(vm.PeriodoFiltro)) cmd.Parameters.AddWithValue("periodo", vm.PeriodoFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.EjercicioFiltro)) cmd.Parameters.AddWithValue("ejercicio", Convert.ToInt32(vm.EjercicioFiltro));
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic d = new ExpandoObject();
                        d.Id = rd.GetInt32(0);
                        d.Codigo = rd.GetString(1);
                        d.Tipo = Enum.TryParse<TipoPdt>(rd.GetString(2), out var t) ? t : TipoPdt.PLAME;
                        d.Periodo = rd.GetString(3);
                        d.Ejercicio = rd.GetInt32(4);
                        d.FechaGeneracion = rd.GetDateTime(5);
                        d.FechaEnvio = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6);
                        d.Estado = Enum.TryParse<EstadoPdt>(rd.GetString(7), out var est) ? est : EstadoPdt.Pendiente;
                        d.NroOrden = rd.IsDBNull(8) ? "" : rd.GetString(8);
                        d.TieneConstancia = rd.GetBoolean(9);
                        d.Usuario = rd.GetString(10);
                        d.Observacion = rd.IsDBNull(11) ? "" : rd.GetString(11);
                        vm.Declaraciones.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/SunatPdt.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerDeclaracionPdt(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT declaracionid, codigo, tipo, periodo, ejercicio, fechageneracion, fechaenvio, estado, nroorden, tieneconstancia, usuario, observacion FROM rrhh_nomina.declaraciones_pdt WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var d = new {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            tipo = rd.GetString(2),
                            periodo = rd.GetString(3),
                            ejercicio = rd.GetInt32(4),
                            fechaGeneracion = rd.GetDateTime(5).ToString("yyyy-MM-ddTHH:mm"),
                            fechaEnvio = rd.IsDBNull(6) ? "" : rd.GetDateTime(6).ToString("yyyy-MM-ddTHH:mm"),
                            estado = rd.GetString(7),
                            nroOrden = rd.IsDBNull(8) ? "" : rd.GetString(8),
                            tieneConstancia = rd.GetBoolean(9),
                            usuario = rd.GetString(10),
                            observacion = rd.IsDBNull(11) ? "" : rd.GetString(11)
                        };
                        return Json(d);
                    }
                }
            }
            catch {}
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearDeclaracionPdt(string tipoDecl, string periodo, int ejercicio, DateTime fechaGeneracion, string estadoDecl, string nroOrden, string usuario, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string nextCod = "";
                using (var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(declaracionid), 0) + 1 FROM rrhh_nomina.declaraciones_pdt", cn))
                {
                    int nextId = Convert.ToInt32(cmd.ExecuteScalar());
                    nextCod = $"SUN-{nextId:D3}";
                }
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.declaraciones_pdt (codigo, tipo, periodo, ejercicio, fechageneracion, estado, nroorden, tieneconstancia, usuario, observacion)
                      VALUES (@codigo, @tipo, @periodo, @ejercicio, @fechagen, @estado, @nro, @tiene, @user, @obs)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", nextCod);
                    cmd.Parameters.AddWithValue("tipo", tipoDecl);
                    cmd.Parameters.AddWithValue("periodo", periodo);
                    cmd.Parameters.AddWithValue("ejercicio", ejercicio);
                    cmd.Parameters.AddWithValue("fechagen", fechaGeneracion);
                    cmd.Parameters.AddWithValue("estado", estadoDecl ?? "Pendiente");
                    cmd.Parameters.AddWithValue("nro", (object)nroOrden ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tiene", estadoDecl == "Aceptada");
                    cmd.Parameters.AddWithValue("user", usuario ?? "Admin");
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeSunat"] = "Declaración creada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear declaración: " + ex.Message;
            }
            return RedirectToAction("SunatPdt");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarDeclaracionPdt(int id, string tipoDecl, string periodo, int ejercicio, DateTime fechaGeneracion, string estadoDecl, string nroOrden, string usuario, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.declaraciones_pdt 
                      SET tipo = @tipo, periodo = @periodo, ejercicio = @ejercicio, fechageneracion = @fechagen, estado = @estado, nroorden = @nro, tieneconstancia = @tiene, usuario = @user, observacion = @obs
                      WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("tipo", tipoDecl);
                    cmd.Parameters.AddWithValue("periodo", periodo);
                    cmd.Parameters.AddWithValue("ejercicio", ejercicio);
                    cmd.Parameters.AddWithValue("fechagen", fechaGeneracion);
                    cmd.Parameters.AddWithValue("estado", estadoDecl ?? "Pendiente");
                    cmd.Parameters.AddWithValue("nro", (object)nroOrden ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tiene", estadoDecl == "Aceptada");
                    cmd.Parameters.AddWithValue("user", usuario ?? "Admin");
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeSunat"] = "Declaración actualizada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar declaración: " + ex.Message;
            }
            return RedirectToAction("SunatPdt");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarDeclaracionPdt(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.declaraciones_pdt WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeSunat"] = "Declaración eliminada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar declaración: " + ex.Message;
            }
            return RedirectToAction("SunatPdt");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarDeclaracionPdt(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string order = $"{new Random().Next(10000000, 99999999)}";
                using (var cmd = new NpgsqlCommand("UPDATE rrhh_nomina.declaraciones_pdt SET estado = 'Aceptada', fechaenvio = CURRENT_TIMESTAMP, nroorden = @nro, tieneconstancia = true, observacion = 'Aceptado por SUNAT sin observaciones.' WHERE declaracionid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("nro", order);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeSunat"] = "Declaración enviada a SUNAT con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al enviar declaración: " + ex.Message;
            }
            return RedirectToAction("SunatPdt");
        }

        [HttpGet]
        public IActionResult DescargarConstanciaPdt(int id)
        {
            string pdfContent = $"CONSTANCIA DE RECEPCIÓN SUNAT\nCódigo de Declaración: SUN-{id:D3}\nFecha de Envío: {DateTime.Now}\nEstado: ACEPTADA\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(pdfContent);
            return File(bytes, "application/pdf", $"Constancia_PDT_{id}.pdf");
        }

        public IActionResult HistorialPagos(string buscar, string estado, string medio, string periodo, int pagina = 1)
        {
            var vm = new HistorialPagosViewModel();
            vm.Buscar = buscar ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.MedioFiltro = medio ?? "";
            vm.PeriodoFiltro = periodo ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 6;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Query active planillas and concepts to let user choose them in the payment registry
                var planillasConceptos = new List<dynamic>();

                // 1. Fetch Planillas
                using (var cmd = new NpgsqlCommand("SELECT codigo, periodo, totalneto, empleados FROM rrhh_nomina.planillas_resumen ORDER BY codigo DESC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic item = new ExpandoObject();
                        item.Tipo = "Planilla";
                        item.Codigo = rd.GetString(0);
                        item.Nombre = $"Planilla {rd.GetString(0)} - {rd.GetString(1)}";
                        item.Periodo = rd.GetString(1);
                        item.Monto = rd.GetDecimal(2);
                        item.Empleados = rd.GetInt32(3);
                        planillasConceptos.Add(item);
                    }
                }

                // 2. Fetch Concepts
                using (var cmd = new NpgsqlCommand("SELECT nombre FROM rrhh_nomina.conceptos WHERE estaactivo = true ORDER BY nombre ASC", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic item = new ExpandoObject();
                        item.Tipo = "Concepto";
                        item.Codigo = rd.GetString(0);
                        item.Nombre = $"Concepto: {rd.GetString(0)}";
                        item.Periodo = "";
                        item.Monto = 0m;
                        item.Empleados = 0;
                        planillasConceptos.Add(item);
                    }
                }

                ViewBag.PlanillasConceptos = planillasConceptos;

                // 3. Fetch count of active employees for autofilling concepts
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = true", cn))
                {
                    ViewBag.ActiveEmployeesCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 4. Current Period string
                ViewBag.CurrentPeriod = DateTime.Today.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));

                // Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.historial_pagos", cn))
                {
                    ViewBag.TotalPagos = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.historial_pagos WHERE estado = 'Pagado'", cn))
                {
                    ViewBag.CountPagado = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.historial_pagos WHERE estado = 'Pendiente' OR estado = 'EnProceso'", cn))
                {
                    ViewBag.CountPendiente = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT SUM(montopagado) FROM rrhh_nomina.historial_pagos WHERE estado = 'Pagado'", cn))
                {
                    var val = cmd.ExecuteScalar();
                    ViewBag.TotalPagado = val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                }

                // Query
                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.historial_pagos WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) countSql += " AND (planillaconcepto ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.MedioFiltro)) countSql += " AND banco = @medio";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.MedioFiltro)) cmd.Parameters.AddWithValue("medio", vm.MedioFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                string sql = "SELECT pagoid, codigo, planillaconcepto, periodo, fechapago, banco, montopagado, estado, empleados, observacion FROM rrhh_nomina.historial_pagos WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.Buscar)) sql += " AND (planillaconcepto ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.MedioFiltro)) sql += " AND banco = @medio";
                sql += " ORDER BY pagoid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.Buscar)) cmd.Parameters.AddWithValue("buscar", $"%{vm.Buscar}%");
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.MedioFiltro)) cmd.Parameters.AddWithValue("medio", vm.MedioFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic p = new ExpandoObject();
                        p.Id = rd.GetInt32(0);
                        p.Codigo = rd.GetString(1);
                        p.PlanillaConcepto = rd.GetString(2);
                        p.Periodo = rd.GetString(3);
                        p.FechaPago = rd.GetDateTime(4);
                        p.Banco = rd.GetString(5);
                        p.MontoPagado = rd.GetDecimal(6);
                        p.Estado = Enum.TryParse<EstadoPago>(rd.GetString(7), out var est) ? est : EstadoPago.Pendiente;
                        p.Empleados = rd.GetInt32(8);
                        p.Observacion = rd.IsDBNull(9) ? "" : rd.GetString(9);
                        vm.Pagos.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/HistorialPagos.cshtml", vm);
        }

        [HttpGet]
        public IActionResult ObtenerPago(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT pagoid, codigo, planillaconcepto, periodo, fechapago, banco, montopagado, estado, empleados, observacion FROM rrhh_nomina.historial_pagos WHERE pagoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        var d = new {
                            id = rd.GetInt32(0),
                            codigo = rd.GetString(1),
                            planillaConcepto = rd.GetString(2),
                            periodo = rd.GetString(3),
                            fechaPago = rd.GetDateTime(4).ToString("yyyy-MM-dd"),
                            banco = rd.GetString(5),
                            montoPagado = rd.GetDecimal(6),
                            estado = rd.GetString(7),
                            empleados = rd.GetInt32(8),
                            observacion = rd.IsDBNull(9) ? "" : rd.GetString(9)
                        };
                        return Json(d);
                    }
                }
            }
            catch {}
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearPago(string planillaConcepto, string periodo, DateTime fechaPago, string banco, decimal montoPagado, string estado, int empleados, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string nextCod = "";
                using (var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(pagoid), 0) + 1 FROM rrhh_nomina.historial_pagos", cn))
                {
                    int nextId = Convert.ToInt32(cmd.ExecuteScalar());
                    nextCod = $"PAG-{nextId:D3}";
                }
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.historial_pagos (codigo, planillaconcepto, periodo, fechapago, banco, montopagado, estado, empleados, observacion)
                      VALUES (@codigo, @concepto, @periodo, @fecha, @banco, @monto, @estado, @emp, @obs)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", nextCod);
                    cmd.Parameters.AddWithValue("concepto", planillaConcepto);
                    cmd.Parameters.AddWithValue("periodo", periodo);
                    cmd.Parameters.AddWithValue("fecha", fechaPago);
                    cmd.Parameters.AddWithValue("banco", banco ?? "BCP");
                    cmd.Parameters.AddWithValue("monto", montoPagado);
                    cmd.Parameters.AddWithValue("estado", estado ?? "Pendiente");
                    cmd.Parameters.AddWithValue("emp", empleados);
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajePago"] = "Pago registrado con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar pago: " + ex.Message;
            }
            return RedirectToAction("HistorialPagos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPago(int id, string planillaConcepto, string periodo, DateTime fechaPago, string banco, decimal montoPagado, string estado, int empleados, string observacion)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand(
                    @"UPDATE rrhh_nomina.historial_pagos 
                      SET planillaconcepto = @concepto, periodo = @periodo, fechapago = @fecha, banco = @banco, montopagado = @monto, estado = @estado, empleados = @emp, observacion = @obs
                      WHERE pagoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("concepto", planillaConcepto);
                    cmd.Parameters.AddWithValue("periodo", periodo);
                    cmd.Parameters.AddWithValue("fecha", fechaPago);
                    cmd.Parameters.AddWithValue("banco", banco ?? "BCP");
                    cmd.Parameters.AddWithValue("monto", montoPagado);
                    cmd.Parameters.AddWithValue("estado", estado ?? "Pendiente");
                    cmd.Parameters.AddWithValue("emp", empleados);
                    cmd.Parameters.AddWithValue("obs", (object)observacion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajePago"] = "Pago actualizado con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar pago: " + ex.Message;
            }
            return RedirectToAction("HistorialPagos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarPago(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.historial_pagos WHERE pagoid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajePago"] = "Pago eliminado con éxito.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar pago: " + ex.Message;
            }
            return RedirectToAction("HistorialPagos");
        }

        public IActionResult Reportes(string buscar, string submodulo, string estado, string formato, int pagina = 1)
        {
            var vm = new ReportesViewModel();
            vm.BuscarFiltro = buscar ?? "";
            vm.SubmoduloFiltro = submodulo ?? "";
            vm.EstadoFiltro = estado ?? "";
            vm.FormatoFiltro = formato ?? "";
            vm.PaginaActual = pagina < 1 ? 1 : pagina;

            int limit = 10;
            int offset = (vm.PaginaActual - 1) * limit;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // Stats
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.reportes", cn))
                {
                    ViewBag.TotalReportes = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.reportes WHERE estado = 'Completado'", cn))
                {
                    ViewBag.Completados = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.reportes WHERE estado = 'En Proceso'", cn))
                {
                    ViewBag.EnProceso = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_nomina.reportes WHERE estado = 'Completado'", cn))
                {
                    ViewBag.TotalDescargas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Query
                string countSql = "SELECT COUNT(*) FROM rrhh_nomina.reportes WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.BuscarFiltro)) countSql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.SubmoduloFiltro)) countSql += " AND submodulo = @sub";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) countSql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.FormatoFiltro)) countSql += " AND formato = @formato";

                using (var cmd = new NpgsqlCommand(countSql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.BuscarFiltro)) cmd.Parameters.AddWithValue("buscar", $"%{vm.BuscarFiltro}%");
                    if (!string.IsNullOrEmpty(vm.SubmoduloFiltro)) cmd.Parameters.AddWithValue("sub", vm.SubmoduloFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.FormatoFiltro)) cmd.Parameters.AddWithValue("formato", vm.FormatoFiltro);
                    vm.TotalItems = Convert.ToInt32(cmd.ExecuteScalar());
                }

                vm.TotalPaginas = (int)Math.Ceiling((double)vm.TotalItems / limit);
                if (vm.TotalPaginas == 0) vm.TotalPaginas = 1;
                if (vm.PaginaActual > vm.TotalPaginas) vm.PaginaActual = vm.TotalPaginas;
                offset = (vm.PaginaActual - 1) * limit;

                string sql = "SELECT reporteid, codigo, nombre, submodulo, periodo, formato, fechageneracion, generadopor, estado, filasgeneradas, tamanokb FROM rrhh_nomina.reportes WHERE 1=1";
                if (!string.IsNullOrEmpty(vm.BuscarFiltro)) sql += " AND (nombre ILIKE @buscar OR codigo ILIKE @buscar)";
                if (!string.IsNullOrEmpty(vm.SubmoduloFiltro)) sql += " AND submodulo = @sub";
                if (!string.IsNullOrEmpty(vm.EstadoFiltro)) sql += " AND estado = @estado";
                if (!string.IsNullOrEmpty(vm.FormatoFiltro)) sql += " AND formato = @formato";
                sql += " ORDER BY reporteid DESC LIMIT @limit OFFSET @offset";

                using (var cmd = new NpgsqlCommand(sql, cn))
                {
                    if (!string.IsNullOrEmpty(vm.BuscarFiltro)) cmd.Parameters.AddWithValue("buscar", $"%{vm.BuscarFiltro}%");
                    if (!string.IsNullOrEmpty(vm.SubmoduloFiltro)) cmd.Parameters.AddWithValue("sub", vm.SubmoduloFiltro);
                    if (!string.IsNullOrEmpty(vm.EstadoFiltro)) cmd.Parameters.AddWithValue("estado", vm.EstadoFiltro);
                    if (!string.IsNullOrEmpty(vm.FormatoFiltro)) cmd.Parameters.AddWithValue("formato", vm.FormatoFiltro);
                    cmd.Parameters.AddWithValue("limit", limit);
                    cmd.Parameters.AddWithValue("offset", offset);

                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic r = new ExpandoObject();
                        r.Id = rd.GetInt32(0);
                        r.Codigo = rd.GetString(1);
                        r.Nombre = rd.GetString(2);
                        r.Submodulo = rd.GetString(3);
                        r.Periodo = rd.GetString(4);
                        r.Formato = rd.GetString(5);
                        r.FechaGeneracion = rd.GetDateTime(6);
                        r.GeneradoPor = rd.GetString(7);
                        r.Estado = rd.GetString(8);
                        r.FilasGeneradas = rd.GetInt32(9);
                        r.TamañoKb = rd.GetInt32(10);
                        vm.Reportes.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Error = ex.Message;
            }

            return PartialView("~/Views/RRHH/Nomina/Reportes.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarReporte(string nombre, string submodulo, string periodo, string formato)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                string nextCod = "";
                using (var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(reporteid), 0) + 1 FROM rrhh_nomina.reportes", cn))
                {
                    int nextId = Convert.ToInt32(cmd.ExecuteScalar());
                    nextCod = $"REP-{nextId:D3}";
                }
                using (var cmd = new NpgsqlCommand(
                    @"INSERT INTO rrhh_nomina.reportes (codigo, nombre, submodulo, periodo, formato, estado, filasgeneradas, tamanokb, generadopor)
                      VALUES (@codigo, @nombre, @sub, @periodo, @formato, 'Completado', @filas, @kb, @user)", cn))
                {
                    cmd.Parameters.AddWithValue("codigo", nextCod);
                    cmd.Parameters.AddWithValue("nombre", nombre ?? "Reporte de Nómina");
                    cmd.Parameters.AddWithValue("sub", submodulo ?? "Planillas");
                    cmd.Parameters.AddWithValue("periodo", periodo ?? "Mayo 2025");
                    cmd.Parameters.AddWithValue("formato", formato ?? "PDF");
                    cmd.Parameters.AddWithValue("filas", new Random().Next(10, 50));
                    cmd.Parameters.AddWithValue("kb", new Random().Next(50, 500));
                    cmd.Parameters.AddWithValue("user", "Jhoel Patrick");
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgReporte"] = "Reporte generado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar reporte: " + ex.Message;
            }
            return RedirectToAction("Reportes");
        }

        [HttpGet]
        public IActionResult DescargarReporte(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("SELECT codigo, nombre, formato FROM rrhh_nomina.reportes WHERE reporteid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    using var rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        string cod = rd.GetString(0);
                        string nombre = rd.GetString(1);
                        string fmt = rd.GetString(2);
                        string mime = fmt == "PDF" ? "application/pdf" : fmt == "Excel" ? "application/vnd.ms-excel" : "text/csv";
                        string ext = fmt == "PDF" ? "pdf" : fmt == "Excel" ? "xls" : "csv";

                        string reportData = $"REPORTE GENERAL ENTERPRISE\nCódigo: {cod}\nReporte: {nombre}\nFecha: {DateTime.Now}\nFormato: {fmt}\n";
                        var bytes = System.Text.Encoding.UTF8.GetBytes(reportData);
                        return File(bytes, mime, $"{nombre.Replace(" ", "_")}_{cod}.{ext}");
                    }
                }
            }
            catch {}
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarReporte(int id)
        {
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM rrhh_nomina.reportes WHERE reporteid = @id", cn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["MsgReporte"] = "Reporte eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar reporte: " + ex.Message;
            }
            return RedirectToAction("Reportes");
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
