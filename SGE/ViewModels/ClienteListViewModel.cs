using Microsoft.AspNetCore.Mvc.Rendering;
using SGE.Models;

namespace SGE.ViewModels;

public class ClienteListViewModel
{
    public IEnumerable<Cliente> Clientes { get; set; } = new List<Cliente>();

    public string? Busqueda { get; set; }

    public string? FiltroEstado { get; set; }

    public string? FiltroRubro { get; set; }

    public int Pagina { get; set; } = 1;

    public int RegistrosPorPagina { get; set; } = 10;

    public int TotalRegistros { get; set; }

    public int TotalPaginas => RegistrosPorPagina <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina));

    public ClienteKpiViewModel Kpis { get; set; } = new();
}

public class ClienteKpiViewModel
{
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int TotalContactos { get; set; }
    public decimal VentasTotales { get; set; }
}
