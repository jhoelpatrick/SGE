namespace SGE.Helpers;

public static class DatabaseErrorHelper
{
    public static string ObtenerMensaje(Exception ex)
    {
        var actual = ex;
        while (actual.InnerException is not null)
            actual = actual.InnerException;

        if (actual.Message.Contains("network-related", StringComparison.OrdinalIgnoreCase)
            || actual.Message.Contains("No se pudo abrir", StringComparison.OrdinalIgnoreCase)
            || actual.Message.Contains("Could not open", StringComparison.OrdinalIgnoreCase)
            || actual.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || actual.Message.Contains("tiempo de espera", StringComparison.OrdinalIgnoreCase))
        {
            return "No hay conexion con SQL Server. Inicie el servicio 'SQL Server (MSSQLSERVER)' en services.msc y vuelva a intentar.";
        }

        if (actual.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase)
            || actual.Message.Contains("no existe la base de datos", StringComparison.OrdinalIgnoreCase))
        {
            return "La base de datos sge_crm no existe. Ejecute script_crm.sql en SQL Server Management Studio.";
        }

        return $"No se pudo completar la operacion: {actual.Message}";
    }
}
