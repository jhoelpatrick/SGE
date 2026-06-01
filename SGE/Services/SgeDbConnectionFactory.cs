using Microsoft.Data.SqlClient;

namespace SGE.Services
{
    public interface ISgeDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class SgeDbConnectionFactory : ISgeDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SgeDbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("sge_crm");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se encontro la cadena de conexion 'sge_crm'.");
            }

            return new SqlConnection(connectionString);
        }
    }
}