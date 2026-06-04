using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SGE.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace SGE.Controllers
{
    public class RRHHController : Controller
    {
        private readonly string _conn;

        public RRHHController(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
        }

        // Sidebar: /RRHH/Recursos
        public IActionResult Recursos()
        {
            var vm = new HRStatsViewModel();
            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();
                
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados WHERE estaactivo = TRUE", cn))
                {
                    vm.ActiveEmployees = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.contratos WHERE estaactivo = TRUE", cn))
                {
                    vm.ActiveContracts = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.programacion_vacaciones WHERE estadosolicitud = 'pendiente'", cn))
                {
                    vm.PendingVacations = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Recent employees (limit to 5)
                using (var cmd = new NpgsqlCommand("SELECT empleadoid, nombres, apellidopaterno, apellidomaterno, correocorporativo, telefonocelular FROM rrhh_recursos.empleados ORDER BY empleadoid DESC LIMIT 5", cn))
                {
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        dynamic emp = new ExpandoObject();
                        emp.Id = rd.GetInt32(0);
                        emp.Nombre = rd.GetString(1);
                        emp.ApellidoPaterno = rd.GetString(2);
                        emp.ApellidoMaterno = rd.IsDBNull(3) ? "" : rd.GetString(3);
                        emp.Correo = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        emp.Telefono = rd.IsDBNull(5) ? "" : rd.GetString(5);
                        
                        var firstInitial = emp.Nombre.Length > 0 ? emp.Nombre[0].ToString() : "";
                        var lastInitial = emp.ApellidoPaterno.Length > 0 ? emp.ApellidoPaterno[0].ToString() : "";
                        emp.Iniciales = (firstInitial + lastInitial).ToUpper();

                        vm.RecentEmployees.Add(emp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RRHHController.Recursos: " + ex.Message);
            }

            return PartialView(vm);
        }

        // Sidebar: /RRHH/Nominas — redirects to Nomina Index action
        public IActionResult Nominas()
        {
            return RedirectToAction("Index", "Nomina");
        }
    }
}
