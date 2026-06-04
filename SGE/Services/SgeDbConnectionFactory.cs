using Npgsql;

namespace SGE.Services
{
    public interface ISgeDbConnectionFactory
    {
        NpgsqlConnection CreateConnection();
    }

    public class SgeDbConnectionFactory : ISgeDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SgeDbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public NpgsqlConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");
            }

            return new NpgsqlConnection(connectionString);
        }
    }
}
