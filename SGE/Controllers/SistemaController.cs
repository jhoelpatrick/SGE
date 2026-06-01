using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using SGE.Models;
using SGE.Services;
using static SGE.Models.Model_Configuracion;

namespace SGE.Controllers
{
    public class SistemaController : Controller
    {
        private readonly ISgeDbConnectionFactory _connectionFactory;

        // Inyección de dependencias para la conexión a Base de Datos
        public SistemaController(ISgeDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Vistas parciales simples
        public IActionResult Reportes() => PartialView();
        public IActionResult Auditoria() => PartialView();

        // ========================================================
        // ACCIÓN CONFIGURACIÓN: Carga datos y maneja errores
        // ========================================================
        public IActionResult Configuracion(string version = "v1")
        {
            // 1. Inicializamos el modelo maestro
            var configGlobal = new SistemaConfiguracionDTO();

            try
            {
                // 2. Intentamos la conexión a la base de datos
                using (SqlConnection conn = _connectionFactory.CreateConnection())
                {
                    using (SqlCommand cmd = new SqlCommand("sistema.Enterprise_InfoVr", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Version", version);

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 3. Procesamos cada fila devuelta (Empresa, Regional, Correo, Seguridad)
                            while (reader.Read())
                            {
                                string categoria = reader["categoria"].ToString().ToLower();
                                string json = reader["valor"].ToString();

                                switch (categoria)
                                {
                                    case "empresa":
                                        configGlobal.Empresa = JsonSerializer.Deserialize<EmpresaModel>(json);
                                        break;
                                    case "regional":
                                        configGlobal.Regional = JsonSerializer.Deserialize<RegionalModel>(json);
                                        break;
                                    case "correo":
                                        configGlobal.Correo = JsonSerializer.Deserialize<CorreoModel>(json);
                                        break;
                                    case "seguridad":
                                        configGlobal.Seguridad = JsonSerializer.Deserialize<SeguridadModel>(json);
                                        break;
                                }
                            }
                        }
                    }
                }

                // 4. IMPORTANTE: Retornamos la vista DENTRO del try.
                // Si el archivo Configuracion.cshtml tiene errores (como campos nulos),
                // el catch de abajo lo atrapará y te dará el mensaje de error.
                return PartialView(configGlobal);
            }
            catch (Exception ex)
            {
                // Registra el error en la consola de salida de Visual Studio (Debug)
                System.Diagnostics.Debug.WriteLine($"Error detectado: {ex.Message}");

                // Devuelve el error 500 con el mensaje real para que lo veas en F12 -> Network
                return StatusCode(500, $"Error en el servidor: {ex.Message}");
            }
        }
    }
}