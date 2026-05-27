using Microsoft.AspNetCore.Mvc.Rendering;
using SGE.Models;

namespace SGE.ViewModels;

public class ProductoListViewModel
{
    public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();

    public string? NombreOSKU { get; set; }

    public string? Busqueda { get; set; }

    public string? FiltroRapido { get; set; }

    public int? CategoriaId { get; set; }

    public bool? SoloActivos { get; set; }

    public bool? SoloServicios { get; set; }

    public bool? Activo { get; set; }

    public bool? EsServicio { get; set; }

    public string? Proveedor { get; set; }

    public string? Almacen { get; set; }

    public decimal? PrecioMinimo { get; set; }

    public decimal? PrecioMaximo { get; set; }

    public bool BajoStock { get; set; }

    public DateTime? FechaCreacionDesde { get; set; }

    public DateTime? FechaCreacionHasta { get; set; }

    public int Pagina { get; set; } = 1;

    public int RegistrosPorPagina { get; set; } = 10;

    public int TotalRegistros { get; set; }

    public int TotalPaginas => RegistrosPorPagina <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina));

    public ProductoKpiViewModel Kpis { get; set; } = new();

    public SelectList Categorias { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public SelectList Proveedores { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public SelectList Almacenes { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

    public SelectList RegistrosPorPaginaOpciones { get; set; } = new SelectList(new[] { 10, 25, 50, 100 });
}

public class ProductoKpiViewModel
{
    public int TotalProductos { get; set; }

    public int TotalServicios { get; set; }

    public int ProductosBajoStock { get; set; }

    public int ProductosAgotados { get; set; }

    public decimal ValorTotalInventario { get; set; }

    public int ProductosActivos { get; set; }

    public int ProductosInactivos { get; set; }
}
