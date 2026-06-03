namespace SGE.ViewModels;

public class ProveedorListViewModel
{
    public List<Models.Proveedor> Proveedores { get; set; } = new();
    public string? Busqueda { get; set; }
    public string? FiltroEstado { get; set; }
    public int Pagina { get; set; } = 1;
    public int RegistrosPorPagina { get; set; } = 10;
    public int TotalRegistros { get; set; }
    public ProveedorKpiViewModel Kpis { get; set; } = new();

    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
}

public class ProveedorKpiViewModel
{
    public int TotalProveedores { get; set; }
    public int ProveedoresActivos { get; set; }
    public int TotalContactos { get; set; }
    public decimal ComprasTotales { get; set; }
}