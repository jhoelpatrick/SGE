using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGE.Data;
using SGE.Helpers;
using SGE.Models;
using SGE.ViewModels;
using System.Globalization;
using System.Text;

namespace SGE.Controllers;

public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductosController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ProductosController(ApplicationDbContext context, ILogger<ProductosController> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    public async Task<IActionResult> Index(ProductoListViewModel filtros)
    {
        try
        {
            await CargarListadoProductosAsync(filtros);
            return View(filtros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el catalogo de productos.");
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            filtros.Productos = new List<Producto>();
            await CargarCombosAsync(filtros);
            return View(filtros);
        }
    }

    public async Task<IActionResult> Insumos(ProductoListViewModel filtros)
    {
        try
        {
            var consulta = _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado && p.SeFabrica);

            if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
            {
                var t = filtros.Busqueda.Trim().ToLower();
                consulta = consulta.Where(p => p.CodigoSku.ToLower().Contains(t) || p.Descripcion.ToLower().Contains(t));
            }

            consulta = filtros.FiltroRapido switch
            {
                "activos"    => consulta.Where(p => p.Estado),
                "inactivos"  => consulta.Where(p => !p.Estado),
                _            => consulta
            };

            filtros.Pagina = filtros.Pagina < 1 ? 1 : filtros.Pagina;
            filtros.RegistrosPorPagina = NormalizarTamanoPagina(filtros.RegistrosPorPagina);
            filtros.TotalRegistros = await consulta.CountAsync();
            filtros.Productos = await consulta
                .OrderBy(p => p.Descripcion)
                .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
                .Take(filtros.RegistrosPorPagina)
                .ToListAsync();

            filtros.Kpis = await ObtenerKpisInsumosAsync();
            await CargarCombosAsync(filtros);
            ViewBag.Categorias = filtros.Categorias;
            return View(filtros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar los insumos.");
            TempData["Error"] = DatabaseErrorHelper.ObtenerMensaje(ex);
            filtros.Productos = new List<Producto>();
            await CargarCombosAsync(filtros);
            ViewBag.Categorias = filtros.Categorias;
            return View(filtros);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInsumo(Producto producto, IFormFile? imagenArchivo)
    {
        try
        {
            producto.CodigoSku = producto.CodigoSku.Trim();
            producto.SeFabrica = true;
            producto.SeVende = false;
            producto.EsServicio = false;
            producto.Estado = true;

            if (await _context.Productos.AnyAsync(p => p.CodigoSku.ToUpper() == producto.CodigoSku.ToUpper() && p.Estado))
                ModelState.AddModelError(nameof(Producto.SKU), "Ya existe un insumo con este SKU.");

            if (!ModelState.IsValid)
            {
                if (EsSolicitudAjax())
                    return BadRequest(new { success = false, message = ObtenerErroresModelo() });
                return RedirectToAction(nameof(Insumos));
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Insumo creado correctamente.";
            if (EsSolicitudAjax())
                return Json(new { success = true });
            return RedirectToAction(nameof(Insumos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear insumo {SKU}.", producto.SKU);
            TempData["Error"] = "No se pudo crear el insumo.";
            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = "No se pudo crear el insumo." });
            return RedirectToAction(nameof(Insumos));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInsumo(int id, Producto producto, IFormFile? imagenArchivo)
    {
        if (id != producto.ProductoId) return BadRequest();
        try
        {
            producto.CodigoSku = producto.CodigoSku.Trim();

            if (await _context.Productos.AnyAsync(p => p.ProductoId != id && p.CodigoSku.ToUpper() == producto.CodigoSku.ToUpper() && p.Estado))
                ModelState.AddModelError(nameof(Producto.SKU), "Ya existe otro insumo con este SKU.");

            if (!ModelState.IsValid)
            {
                if (EsSolicitudAjax())
                    return BadRequest(new { success = false, message = ObtenerErroresModelo() });
                return RedirectToAction(nameof(Insumos));
            }

            var db = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && p.Estado);
            if (db is null) return NotFound();

            db.CodigoSku = producto.CodigoSku;
            db.Descripcion = producto.Descripcion;
            db.UnidadMedida = producto.UnidadMedida;
            db.CostoPromedio = producto.CostoPromedio;
            db.PrecioVentaSugerido = producto.PrecioVentaSugerido;
            db.Estado = producto.Activo;
            db.SeFabrica = true;
            db.SeVende = false;
            db.EsServicio = false;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Insumo actualizado correctamente.";
            if (EsSolicitudAjax())
                return Json(new { success = true });
            return RedirectToAction(nameof(Insumos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar insumo {id}.", id);
            TempData["Error"] = "No se pudo actualizar el insumo.";
            if (EsSolicitudAjax())
                return BadRequest(new { success = false, message = "No se pudo actualizar el insumo." });
            return RedirectToAction(nameof(Insumos));
        }
    }

    public IActionResult Categorias()
    {
        TempData["Error"] = "Las categorias no existen en comercial.productos. Use el catalogo de productos SUNAT.";
        return View(new List<Categoria>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateCategoria(string Nombre, bool Activo)
    {
        TempData["Error"] = "Las categorias no estan disponibles en la base de datos comercial.";
        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCategoria(int id, string Nombre, bool Activo)
    {
        TempData["Error"] = "Las categorias no estan disponibles en la base de datos comercial.";
        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCategoria(int id)
    {
        TempData["Error"] = "Las categorias no estan disponibles en la base de datos comercial.";
        return RedirectToAction(nameof(Categorias));
    }

    public async Task<IActionResult> Analytics()
    {
        return View(await ObtenerKpisAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categorias = await ObtenerCategoriasSelectListAsync();
        return View(new Producto { Activo = true, RequiereInventario = true, FechaCreacion = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto, IFormFile? imagenArchivo)
    {
        try
        {
            producto.CodigoSku = producto.CodigoSku.Trim();
            var skuNormalizado = producto.CodigoSku.ToUpper();

            if (await _context.Productos.AnyAsync(p => p.CodigoSku.ToUpper() == skuNormalizado && p.Estado))
            {
                ModelState.AddModelError(nameof(Producto.SKU), "Ya existe un producto con este SKU.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
                if (EsSolicitudAjax())
                {
                    return BadRequest(new { success = false, message = ObtenerErroresModelo() });
                }

                return View(producto);
            }

            producto.SeVende = true;
            producto.SeFabrica = false;
            producto.EsServicio = !producto.RequiereInventario;
            producto.Estado = producto.Activo;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto creado correctamente.";
            if (EsSolicitudAjax())
            {
                return Json(new { success = true, message = "Producto creado correctamente." });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el producto {SKU}.", producto.SKU);
            ModelState.AddModelError(string.Empty, "No se pudo crear el producto. Verifica los datos e intentalo nuevamente.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            if (EsSolicitudAjax())
            {
                return BadRequest(new { success = false, message = ObtenerErroresModelo() });
            }

            return View(producto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && p.Estado);
            if (producto is null)
            {
                return NotFound();
            }

            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            return View(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el producto {ProductoId}.", id);
            TempData["Error"] = "No se pudo cargar el producto solicitado.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto, IFormFile? imagenArchivo)
    {
        if (id != producto.ProductoId)
        {
            return BadRequest();
        }

        try
        {
            producto.CodigoSku = producto.CodigoSku.Trim();
            var skuNormalizado = producto.CodigoSku.ToUpper();

            if (await _context.Productos.AnyAsync(p => p.ProductoId != id && p.CodigoSku.ToUpper() == skuNormalizado && p.Estado))
            {
                ModelState.AddModelError(nameof(Producto.SKU), "Ya existe otro producto con este SKU.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
                if (EsSolicitudAjax())
                {
                    return BadRequest(new { success = false, message = ObtenerErroresModelo() });
                }

                return View(producto);
            }

            var productoDb = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && p.Estado);
            if (productoDb is null)
            {
                return NotFound();
            }

            productoDb.CodigoSku = producto.CodigoSku;
            productoDb.Descripcion = producto.Descripcion;
            productoDb.CodigoSunat = producto.CodigoSunat;
            productoDb.UnidadMedida = producto.UnidadMedida;
            productoDb.TipoAfectacionIgv = producto.TipoAfectacionIgv;
            productoDb.CostoPromedio = producto.CostoPromedio;
            productoDb.PrecioVentaSugerido = producto.PrecioVentaSugerido;
            productoDb.EsServicio = !producto.RequiereInventario;
            productoDb.Estado = producto.Activo;
            productoDb.SeVende = true;
            productoDb.SeFabrica = false;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto actualizado correctamente.";
            if (EsSolicitudAjax())
            {
                return Json(new { success = true, message = "Producto actualizado correctamente." });
            }

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Conflicto de concurrencia al actualizar el producto {ProductoId}.", id);
            ModelState.AddModelError(string.Empty, "El producto fue modificado por otro usuario. Vuelve a cargarlo e intentalo otra vez.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            if (EsSolicitudAjax())
            {
                return BadRequest(new { success = false, message = ObtenerErroresModelo() });
            }

            return View(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el producto {ProductoId}.", id);
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el producto.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            if (EsSolicitudAjax())
            {
                return BadRequest(new { success = false, message = ObtenerErroresModelo() });
            }

            return View(producto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        return await CambiarEliminacionLogicaAsync(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && p.Estado);
        if (producto is null)
        {
            return NotFound();
        }

        producto.Estado = !producto.Estado;
        await _context.SaveChangesAsync();

        TempData["Success"] = producto.Estado ? "Producto activado correctamente." : "Producto inactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, int nuevoStock)
    {
        TempData["Error"] = "El stock se gestiona en operaciones.stockalmacen.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id)
    {
        var origen = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.ProductoId == id && p.Estado);
        if (origen is null)
        {
            return NotFound();
        }

        var copia = new Producto
        {
            CodigoSku = $"{origen.CodigoSku}-COPIA",
            CodigoSunat = origen.CodigoSunat,
            Descripcion = $"{origen.Descripcion} (Copia)",
            UnidadMedida = origen.UnidadMedida,
            TipoAfectacionIgv = origen.TipoAfectacionIgv,
            PrecioVentaSugerido = origen.PrecioVentaSugerido,
            CostoPromedio = origen.CostoPromedio,
            EsServicio = origen.EsServicio,
            SeVende = origen.SeVende,
            NoSeVende = origen.NoSeVende,
            SeFabrica = origen.SeFabrica,
            Estado = true
        };

        _context.Productos.Add(copia);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Producto duplicado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string accionMasiva, List<int> productoIds, int? categoriaMasivaId, int? stockMasivo)
    {
        if (productoIds.Count == 0)
        {
            TempData["Error"] = "Selecciona al menos un producto.";
            return RedirectToAction(nameof(Index));
        }

        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.ProductoId) && p.Estado)
            .ToListAsync();

        foreach (var producto in productos)
        {
            switch (accionMasiva)
            {
                case "activar":
                    producto.Estado = true;
                    break;
                case "inactivar":
                    producto.Estado = false;
                    break;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Accion masiva aplicada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel(ProductoListViewModel filtros)
    {
        var productos = await AplicarFiltros(ConsultaBase(), filtros)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Productos");
        var encabezados = new[] { "SKU", "Nombre", "Categoria", "Tipo", "Marca", "Proveedor", "Almacen", "Unidad", "Costo", "Precio", "Stock", "Stock Minimo", "Estado", "Fecha Actualizacion" };

        for (var i = 0; i < encabezados.Length; i++)
        {
            hoja.Cell(1, i + 1).Value = encabezados[i];
            hoja.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (var row = 0; row < productos.Count; row++)
        {
            var producto = productos[row];
            var excelRow = row + 2;
            hoja.Cell(excelRow, 1).Value = producto.SKU;
            hoja.Cell(excelRow, 2).Value = producto.Nombre;
            hoja.Cell(excelRow, 3).Value = producto.Categoria?.Nombre;
            hoja.Cell(excelRow, 4).Value = producto.RequiereInventario ? "Producto" : "Servicio";
            hoja.Cell(excelRow, 5).Value = producto.Marca;
            hoja.Cell(excelRow, 6).Value = producto.Proveedor;
            hoja.Cell(excelRow, 7).Value = producto.Almacen;
            hoja.Cell(excelRow, 8).Value = producto.UnidadDeMedida;
            hoja.Cell(excelRow, 9).Value = producto.CostoCompra;
            hoja.Cell(excelRow, 10).Value = producto.PrecioUnitario;
            hoja.Cell(excelRow, 11).Value = producto.StockActual;
            hoja.Cell(excelRow, 12).Value = producto.StockMinimo;
            hoja.Cell(excelRow, 13).Value = producto.EstadoStock;
            hoja.Cell(excelRow, 14).Value = producto.FechaActualizacion ?? producto.FechaCreacion;
        }

        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"productos-{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcel(IFormFile archivoExcel)
    {
        if (archivoExcel is null || archivoExcel.Length == 0)
        {
            TempData["Error"] = "Selecciona un archivo Excel valido.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var stream = archivoExcel.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var hoja = workbook.Worksheets.First();
            var filas = hoja.RowsUsed().Skip(1);
            var importados = 0;

            foreach (var fila in filas)
            {
                var sku = fila.Cell(1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(sku))
                {
                    continue;
                }

                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.CodigoSku == sku && p.Estado);
                var esNuevo = producto is null;
                producto ??= new Producto
                {
                    CodigoSku = sku,
                    SeVende = true,
                    SeFabrica = false,
                    Estado = true
                };

                var nombre = fila.Cell(2).GetString().Trim();
                var descripcion = fila.Cell(3).GetString().Trim();
                producto.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? nombre : descripcion;
                producto.UnidadMedida = string.IsNullOrWhiteSpace(fila.Cell(8).GetString()) ? "NIU" : fila.Cell(8).GetString().Trim()[..Math.Min(3, fila.Cell(8).GetString().Trim().Length)];
                producto.CostoPromedio = ObtenerDecimal(fila.Cell(9));
                producto.PrecioVentaSugerido = ObtenerDecimal(fila.Cell(10));
                producto.EsServicio = string.Equals(fila.Cell(13).GetString().Trim(), "Servicio", StringComparison.OrdinalIgnoreCase);
                producto.SeVende = true;
                producto.SeFabrica = false;
                producto.Estado = true;

                if (esNuevo)
                {
                    _context.Productos.Add(producto);
                }

                importados++;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{importados} registros importados correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar productos desde Excel.");
            TempData["Error"] = "No se pudo importar el archivo Excel.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportPdf(ProductoListViewModel filtros)
    {
        var productos = await AplicarFiltros(ConsultaBase(), filtros)
            .OrderBy(p => p.Descripcion)
            .Take(100)
            .ToListAsync();

        var texto = new StringBuilder();
        texto.AppendLine("Catalogo de Productos");
        texto.AppendLine($"Generado: {DateTime.Now:g}");
        texto.AppendLine();

        foreach (var producto in productos)
        {
            texto.AppendLine($"{producto.CodigoSku} | {producto.Descripcion} | {(producto.EsServicio ? "Servicio" : "Producto")} | {producto.PrecioVentaSugerido:C}");
        }

        return File(CrearPdfBasico(texto.ToString()), "application/pdf", $"productos-{DateTime.Now:yyyyMMddHHmm}.pdf");
    }

    private async Task<IActionResult> CambiarEliminacionLogicaAsync(int id)
    {
        try
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto is null)
            {
                return NotFound();
            }

            producto.Estado = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto archivado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al archivar el producto {ProductoId}.", id);
            TempData["Error"] = "No se pudo archivar el producto.";
        }

        return RedirectToAction(nameof(Index));
    }

    private IQueryable<Producto> ConsultaBase()
    {
        return _context.Productos
            .AsNoTracking()
            .Where(producto => producto.Estado && producto.SeVende && !producto.SeFabrica);
    }

    private async Task CargarListadoProductosAsync(ProductoListViewModel filtros)
    {
        filtros.Pagina = filtros.Pagina < 1 ? 1 : filtros.Pagina;
        filtros.RegistrosPorPagina = NormalizarTamanoPagina(filtros.RegistrosPorPagina);

        var consultaFiltrada = AplicarFiltros(ConsultaBase(), filtros);

        filtros.TotalRegistros = await consultaFiltrada.CountAsync();
        filtros.Productos = await consultaFiltrada
            .OrderBy(producto => producto.Descripcion)
            .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
            .Take(filtros.RegistrosPorPagina)
            .ToListAsync();

        filtros.Kpis = await ObtenerKpisAsync();
        await CargarCombosAsync(filtros);
    }

    private static IQueryable<Producto> AplicarFiltros(IQueryable<Producto> consulta, ProductoListViewModel filtros)
    {
        var busqueda = filtros.Busqueda ?? filtros.NombreOSKU;
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim().ToLower();
            consulta = consulta.Where(producto =>
                producto.CodigoSku.ToLower().Contains(termino)
                || producto.Descripcion.ToLower().Contains(termino));
        }

        consulta = filtros.FiltroRapido switch
        {
            "activos" => consulta.Where(producto => producto.Estado),
            "inactivos" => consulta.Where(producto => !producto.Estado),
            "productos" => consulta.Where(producto => !producto.EsServicio),
            "servicios" => consulta.Where(producto => producto.EsServicio),
            _ => consulta
        };

        var activo = filtros.Activo ?? filtros.SoloActivos;
        if (activo.HasValue)
        {
            consulta = consulta.Where(producto => producto.Estado == activo.Value);
        }

        var esServicio = filtros.EsServicio ?? filtros.SoloServicios;
        if (esServicio.HasValue)
        {
            consulta = consulta.Where(producto => producto.EsServicio == esServicio.Value);
        }

        if (filtros.PrecioMinimo.HasValue)
        {
            consulta = consulta.Where(producto => producto.PrecioVentaSugerido >= filtros.PrecioMinimo.Value);
        }

        if (filtros.PrecioMaximo.HasValue)
        {
            consulta = consulta.Where(producto => producto.PrecioVentaSugerido <= filtros.PrecioMaximo.Value);
        }

        return consulta;
    }

    private async Task<ProductoKpiViewModel> ObtenerKpisAsync()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado && p.SeVende && !p.SeFabrica)
            .ToListAsync();

        return new ProductoKpiViewModel
        {
            TotalProductos = productos.Count(p => !p.EsServicio),
            TotalServicios = productos.Count(p => p.EsServicio),
            ProductosBajoStock = 0,
            ProductosAgotados = 0,
            ValorTotalInventario = productos.Where(p => !p.EsServicio).Sum(p => p.CostoPromedio),
            ProductosActivos = productos.Count(p => p.Estado),
            ProductosInactivos = productos.Count(p => !p.Estado)
        };
    }

    private async Task<ProductoKpiViewModel> ObtenerKpisInsumosAsync()
    {
        var insumos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado && p.SeFabrica)
            .ToListAsync();

        return new ProductoKpiViewModel
        {
            TotalProductos = insumos.Count,
            TotalServicios = 0,
            ProductosBajoStock = 0,
            ProductosAgotados = 0,
            ValorTotalInventario = insumos.Sum(p => p.CostoPromedio),
            ProductosActivos = insumos.Count(p => p.Estado),
            ProductosInactivos = insumos.Count(p => !p.Estado)
        };
    }

    private Task CargarCombosAsync(ProductoListViewModel model)
    {
        model.Categorias = ObtenerCategoriasSelectList(model.CategoriaId);
        model.Proveedores = new SelectList(Enumerable.Empty<string>(), model.Proveedor);
        model.Almacenes = new SelectList(Enumerable.Empty<string>(), model.Almacen);
        model.RegistrosPorPaginaOpciones = new SelectList(new[] { 10, 25, 50, 100 }, model.RegistrosPorPagina);
        return Task.CompletedTask;
    }

    private static SelectList ObtenerCategoriasSelectList(int? categoriaSeleccionadaId = null)
    {
        return new SelectList(Enumerable.Empty<Categoria>(), nameof(Categoria.CategoriaId), nameof(Categoria.Nombre), categoriaSeleccionadaId);
    }

    private Task<SelectList> ObtenerCategoriasSelectListAsync(int? categoriaSeleccionadaId = null)
    {
        return Task.FromResult(ObtenerCategoriasSelectList(categoriaSeleccionadaId));
    }

    private Task<int> ResolverCategoriaIdAsync(string nombreCategoria)
    {
        return Task.FromResult(0);
    }

    private async Task<string?> GuardarImagenAsync(IFormFile? imagenArchivo)
    {
        if (imagenArchivo is null || imagenArchivo.Length == 0)
        {
            return null;
        }

        var extensionesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        var extension = Path.GetExtension(imagenArchivo.FileName);
        if (!extensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagen no permitido.");
        }

        var carpeta = Path.Combine(_environment.WebRootPath, "uploads", "productos");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(carpeta, nombreArchivo);

        await using var stream = System.IO.File.Create(ruta);
        await imagenArchivo.CopyToAsync(stream);

        return $"/uploads/productos/{nombreArchivo}";
    }
    private static decimal ObtenerDecimal(IXLCell celda)
    {
        return celda.TryGetValue<decimal>(out var valor) ? valor : 0m;
    }

    private static int ObtenerEntero(IXLCell celda)
    {
        return celda.TryGetValue<int>(out var valor) ? Math.Max(0, valor) : 0;
    }

    private static int NormalizarTamanoPagina(int registrosPorPagina)
    {
        return new[] { 10, 25, 50, 100 }.Contains(registrosPorPagina) ? registrosPorPagina : 10;
    }

    private bool EsSolicitudAjax()
    {
        return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    private string ObtenerErroresModelo()
    {
        var errores = ModelState.Values
            .SelectMany(valor => valor.Errors)
            .Select(error => error.ErrorMessage)
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .ToList();

        return errores.Count == 0 ? "Revisa los datos del producto." : string.Join(" ", errores);
    }

    private static byte[] CrearPdfBasico(string texto)
    {
        var lineas = texto.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
            .Split(Environment.NewLine, StringSplitOptions.None)
            .Take(42)
            .ToList();

        var contenido = new StringBuilder("BT /F1 10 Tf 40 790 Td 14 TL ");
        foreach (var linea in lineas)
        {
            contenido.Append(CultureInfo.InvariantCulture, $"({linea}) Tj T* ");
        }

        contenido.Append("ET");
        var stream = contenido.ToString();
        var objetos = new List<string>
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj",
            "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj",
            $"5 0 obj << /Length {Encoding.ASCII.GetByteCount(stream)} >> stream\n{stream}\nendstream endobj"
        };

        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var objeto in objetos)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(objeto).Append('\n');
        }

        var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 6\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            pdf.Append(CultureInfo.InvariantCulture, $"{offset:0000000000} 00000 n \n");
        }

        pdf.Append("trailer << /Size 6 /Root 1 0 R >>\nstartxref\n");
        pdf.Append(xref).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }
}
