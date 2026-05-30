using Microsoft.Data.SqlClient;

namespace SGE.Services;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SGE")!;
    }

    public SqlConnection Create() => new SqlConnection(_connectionString);
}