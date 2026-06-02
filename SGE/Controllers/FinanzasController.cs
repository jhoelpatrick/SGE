using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Controllers
{
    public class FinanzasController : Controller
    {
        private readonly string _conn;

        public FinanzasController(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public IActionResult Impuestos()
        {
            var vm = new ImpuestosViewModel();
            try
            {
                using var cn = new SqlConnection(_conn);
                cn.Open();
                // KPIs desde finanzas.impuestos
                using var cmd = new SqlCommand(
                    "SELECT codigoimpuestosunat, nombreimpuesto, porcentaje, estado FROM finanzas.impuestos", cn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    vm.Impuestos.Add(new Impuesto
                    {
                        CodigoImpuestoSunat = rd.GetString(0),
                        NombreImpuesto      = rd.GetString(1),
                        Porcentaje          = rd.GetDecimal(2),
                        Estado              = rd.GetBoolean(3)
                    });
                }
                var igv = vm.Impuestos.FirstOrDefault(i => i.CodigoImpuestoSunat == "1000");
                vm.Kpis = new List<KPI>
                {
                    new() { Titulo = "IGV Neto",          Icono = "bi-percent",        Valor = vm.IgvNeto.ToString("C") },
                    new() { Titulo = "Débito Fiscal",     Icono = "bi-arrow-up-circle", Valor = vm.DebitoFiscal.ToString("C") },
                    new() { Titulo = "Crédito Fiscal",    Icono = "bi-arrow-down-circle", Valor = vm.CreditoFiscal.ToString("C") },
                    new() { Titulo = "Retenciones",       Icono = "bi-lock",           Valor = vm.Retenciones.ToString("C") }
                };
            }
            catch { vm.Kpis = DefaultKpis("Impuestos"); }
            return PartialView("~/Views/Finanzas/Impuestos.cshtml", vm);
        }

        public IActionResult Contabilidad()
        {
            var vm = new ContabilidadFinanzasViewModel();
            try
            {
                using var cn = new SqlConnection(_conn);
                cn.Open();
                using var cmd = new SqlCommand(
                    @"SELECT TOP 50 a.numeroasiento, a.fechaasiento, a.tipolibrosunat,
                             c.cuentacodigo, c.descripcion, a.glosa, ad.debe, ad.haber
                      FROM finanzas.asientoscabecera a
                      JOIN finanzas.asientosdetalle ad ON ad.asientoid = a.asientoid
                      JOIN finanzas.plancuentas c ON c.cuentacodigo = ad.cuentacodigo
                      ORDER BY a.fechaasiento DESC", cn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                    vm.LibroDiario.Add(new LibroDiarioItem
                    {
                        NumeroAsiento   = rd.GetString(0),
                        FechaAsiento    = rd.GetDateTime(1),
                        TipoLibroSunat  = rd.IsDBNull(2) ? "" : rd.GetString(2),
                        CuentaCodigo    = rd.GetString(3),
                        NombreCuenta    = rd.GetString(4),
                        Glosa           = rd.IsDBNull(5) ? "" : rd.GetString(5),
                        Debe            = rd.GetDecimal(6),
                        Haber           = rd.GetDecimal(7)
                    });
            }
            catch { }
            vm.Kpis = DefaultKpis("Contabilidad");
            return PartialView("~/Views/Finanzas/Contabilidad.cshtml", vm);
        }

        public IActionResult Caja_y_Bancos()
        {
            var vm = new CajaBancosViewModel();
            try
            {
                using var cn = new SqlConnection(_conn);
                cn.Open();
                using var cmd = new SqlCommand(
                    "SELECT cuentabancariaid, banconombre, numerocuenta, tipocuenta, moneda, saldoactual, estado FROM finanzas.cuentasbancarias", cn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                    vm.Cuentas.Add(new CuentaBancaria
                    {
                        CuentaBancariaId = rd.GetInt32(0),
                        BancoNombre      = rd.GetString(1),
                        NumeroCuenta     = rd.GetString(2),
                        TipoCuenta       = rd.GetString(3),
                        Moneda           = rd.GetString(4),
                        SaldoActual      = rd.GetDecimal(5),
                        Estado           = rd.GetBoolean(6)
                    });
            }
            catch { }
            vm.Kpis = DefaultKpis("Caja y Bancos");
            return PartialView("~/Views/Finanzas/Caja_y_Bancos.cshtml", vm);
        }

        public IActionResult ActivosFijos()
        {
            var vm = new ActivosFijosViewModel();
            try
            {
                using var cn = new SqlConnection(_conn);
                cn.Open();
                using var cmd = new SqlCommand(
                    @"SELECT activoid, codigoactivo, descripcion, fechadquisicion,
                             valorinicial, tasadepreciacionanual, depreciacionacumulada,
                             valornetolibros, estado
                      FROM finanzas.activosfijos ORDER BY fechadquisicion DESC", cn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                    vm.Activos.Add(new ActivoFijo
                    {
                        ActivoId               = rd.GetInt32(0),
                        CodigoActivo           = rd.GetString(1),
                        Descripcion            = rd.GetString(2),
                        FechaAdquisicion       = rd.GetDateTime(3),
                        ValorInicial           = rd.GetDecimal(4),
                        TasaDepreciacionAnual  = rd.GetDecimal(5),
                        DepreciacionAcumulada  = rd.GetDecimal(6),
                        ValorNetoLibros        = rd.GetDecimal(7),
                        Estado                 = rd.IsDBNull(8) ? "" : rd.GetString(8)
                    });
                vm.DepreciacionTotal = vm.Activos.Sum(a => a.DepreciacionAcumulada);
                vm.ValorNetoTotal    = vm.Activos.Sum(a => a.ValorNetoLibros);
            }
            catch { }
            vm.Kpis = DefaultKpis("Activos Fijos");
            return PartialView("~/Views/Finanzas/ActivosFijos.cshtml", vm);
        }

        private static List<KPI> DefaultKpis(string modulo) => new()
        {
            new() { Titulo = "Total",   Icono = "bi-collection",   Valor = "—", Detalle = modulo },
            new() { Titulo = "Activos", Icono = "bi-check-circle", Valor = "—", Detalle = "Activos" },
            new() { Titulo = "Periodo", Icono = "bi-calendar",     Valor = DateTime.Now.ToString("MMM yyyy"), Detalle = "Período actual" },
            new() { Titulo = "Estado",  Icono = "bi-shield-check", Valor = "OK", Detalle = "Sistema" }
        };
    }
}
