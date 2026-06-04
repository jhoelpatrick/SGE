using System.Data;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Configuration;

namespace SGE.Sql
{
    public class ConexionSql
    {
        private readonly string _cadenaSql;

        // El constructor recibe la configuración del appsettings.json automáticamente
        public ConexionSql(IConfiguration configuration)
        {
            _cadenaSql = configuration.GetConnectionString("CadenaSQL");
        }

        // Este método te devolverá una conexión lista para usar (abierta)
        public SqlConnection ObtenerConexion()
        {
            var conexion = new SqlConnection(_cadenaSql);

            if (conexion.State == ConnectionState.Closed)
            {
                conexion.Open();
            }

            return conexion;
        }
    }
}