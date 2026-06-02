namespace Reportes.Models
{
    public class AuditoriaItemViewModel
    {
        public long LogId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string TablaAfectada { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string IdRegistroAfectado { get; set; } = string.Empty;
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }
    }

    public class AuditoriaAlertaViewModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
    }

    public class AuditoriaViewModel
    {
        public List<AuditoriaItemViewModel> Registros { get; set; } = new();
        public List<AuditoriaItemViewModel> Recientes { get; set; } = new();
        public List<AuditoriaAlertaViewModel> Alertas { get; set; } = new();

        public List<string> Tablas { get; set; } = new();
        public List<string> Usuarios { get; set; } = new();

        public string? TablaAfectada { get; set; }
        public string? Accion { get; set; }
        public string? UsuarioFiltro { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
        public int TotalRegistros { get; set; }

        public int TotalEventos { get; set; }
        public int UsuariosUnicos { get; set; }
        public int CambiosCriticos { get; set; }
        public int EventosHoy { get; set; }

        public string NivelRiesgo { get; set; } = "Bajo";

        public List<string> ChartAccionLabels { get; set; } = new();
        public List<int> ChartAccionValues { get; set; } = new();

        public List<string> ChartTablaLabels { get; set; } = new();
        public List<int> ChartTablaValues { get; set; } = new();

        public string? Error { get; set; }

        public int TotalPaginas
        {
            get
            {
                if (RegistrosPorPagina <= 0) return 1;
                return (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
            }
        }
    }
}