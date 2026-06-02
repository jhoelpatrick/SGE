namespace Reportes.Models
{
    public class ParametroViewModel
    {
        public int ParametroId { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaModificacion { get; set; }
    }

    public class ParametrosIndexViewModel
    {
        public List<ParametroViewModel> Parametros { get; set; } = new();
        public List<ParametroViewModel> Recientes { get; set; } = new();

        public int TotalParametros { get; set; }
        public int TotalCategorias { get; set; }
        public int ActualizadosHoy { get; set; }
        public DateTime? UltimaActualizacion { get; set; }

        public List<string> ChartCategoriaLabels { get; set; } = new();
        public List<int> ChartCategoriaValues { get; set; } = new();

        public string? Mensaje { get; set; }
        public string? Error { get; set; }

        public IEnumerable<IGrouping<string, ParametroViewModel>> ParametrosPorCategoria
        {
            get
            {
                return Parametros
                    .OrderBy(x => x.Categoria)
                    .ThenBy(x => x.Clave)
                    .GroupBy(x => x.Categoria);
            }
        }
    }
}