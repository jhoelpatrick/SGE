using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;

namespace SGE.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly string _conn;

        public PrincipalController(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public IActionResult Index()
        {
            int usuariosActivos = 0;
            int ventasMes = 0;
            int facturasEmitidas = 0;
            decimal ingresosTotales = 0m;
            int clientesCount = 0;
            int proveedoresCount = 0;
            int productosCount = 0;
            int comprasCount = 0;
            int almacenesCount = 0;
            int empleadosCount = 0;
            int proyectosCount = 0;

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                cn.Open();

                // 1. Usuarios Activos
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.usuarios_nomina WHERE estaactivo = TRUE", cn))
                {
                    usuariosActivos = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2. Ventas del Mes
                using (var cmd = new NpgsqlCommand(
                    @"SELECT COUNT(*) FROM operaciones.pedidosventa 
                      WHERE EXTRACT(MONTH FROM fechaemision) = EXTRACT(MONTH FROM CURRENT_DATE) 
                      AND EXTRACT(YEAR FROM fechaemision) = EXTRACT(YEAR FROM CURRENT_DATE)", cn))
                {
                    ventasMes = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3. Facturas Emitidas (comprobantes del mes actual)
                using (var cmd = new NpgsqlCommand(
                    @"SELECT COUNT(*) FROM operaciones.comprobantesfacturacion 
                      WHERE EXTRACT(MONTH FROM fechaemision) = EXTRACT(MONTH FROM CURRENT_DATE) 
                      AND EXTRACT(YEAR FROM fechaemision) = EXTRACT(YEAR FROM CURRENT_DATE)", cn))
                {
                    facturasEmitidas = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 4. Ingresos Totales (Suma de totalneto de todos los pedidos de venta)
                using (var cmd = new NpgsqlCommand("SELECT COALESCE(SUM(totalneto), 0) FROM operaciones.pedidosventa", cn))
                {
                    ingresosTotales = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // 5. Clientes
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM comercial.clientes", cn))
                {
                    clientesCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 6. Proveedores
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM comercial.proveedores", cn))
                {
                    proveedoresCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 7. Productos
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM comercial.productos", cn))
                {
                    productosCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 8. Compras (ordenes de compra)
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM operaciones.ordenescompra", cn))
                {
                    comprasCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 9. Almacenes
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM operaciones.almacenes", cn))
                {
                    almacenesCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 10. Empleados
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM rrhh_recursos.empleados", cn))
                {
                    empleadosCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 11. Proyectos
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM operaciones.proyectos", cn))
                {
                    proyectosCount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                // Silently fallback to default zeros
                Console.WriteLine("Error fetching dashboard statistics: " + ex.Message);
            }

            // Additional counts from finanzas schema (separate try to avoid single failure breaking all)
            int activosFijosCount = 0;
            int cuentasBancariasCount = 0;
            int impuestosCount = 0;
            int rolesCount = 0;
            try
            {
                using var cn2 = new NpgsqlConnection(_conn);
                cn2.Open();

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM finanzas.activosfijos", cn2))
                    activosFijosCount = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM finanzas.cuentasbancarias", cn2))
                    cuentasBancariasCount = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM finanzas.impuestos", cn2))
                    impuestosCount = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd = new NpgsqlCommand("SELECT COUNT(DISTINCT rol) FROM rrhh_recursos.usuarios_nomina", cn2))
                    rolesCount = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching finanzas statistics: " + ex.Message);
            }

            ViewBag.UsuariosActivos = usuariosActivos;
            ViewBag.VentasMes = ventasMes;
            ViewBag.FacturasEmitidas = facturasEmitidas;
            ViewBag.IngresosTotales = ingresosTotales;
            ViewBag.ClientesCount = clientesCount;
            ViewBag.ProveedoresCount = proveedoresCount;
            ViewBag.ProductosCount = productosCount;
            ViewBag.ComprasCount = comprasCount;
            ViewBag.AlmacenesCount = almacenesCount;
            ViewBag.EmpleadosCount = empleadosCount;
            ViewBag.ProyectosCount = proyectosCount;
            ViewBag.ActivosFijosCount = activosFijosCount;
            ViewBag.CuentasBancariasCount = cuentasBancariasCount;
            ViewBag.ImpuestosCount = impuestosCount;
            ViewBag.RolesCount = rolesCount;

            return PartialView();
        }
    }
}
