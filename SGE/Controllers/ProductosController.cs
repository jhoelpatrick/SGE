using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGE.Data;
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
            filtros.Pagina = filtros.Pagina < 1 ? 1 : filtros.Pagina;
            filtros.RegistrosPorPagina = NormalizarTamanoPagina(filtros.RegistrosPorPagina);

            var consultaFiltrada = AplicarFiltros(ConsultaBase(), filtros);

            filtros.TotalRegistros = await consultaFiltrada.CountAsync();
            filtros.Productos = await consultaFiltrada
                .OrderBy(producto => producto.Nombre)
                .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
                .Take(filtros.RegistrosPorPagina)
                .ToListAsync();

            filtros.Kpis = await ObtenerKpisAsync();
            await CargarCombosAsync(filtros);

            return View(filtros);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el catalogo de productos.");
            TempData["Error"] = "No se pudo cargar el catalogo de productos.";
            filtros.Productos = new List<Producto>();
            await CargarCombosAsync(filtros);
            return View(filtros);
        }
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
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
                return View(producto);
            }

            producto.FechaCreacion = DateTime.UtcNow;
            producto.UsuarioCreacion = User.Identity?.Name ?? "Sistema";
            producto.IsDeleted = false;
            producto.ImagenUrl = await GuardarImagenAsync(imagenArchivo) ?? producto.ImagenUrl;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el producto {SKU}.", producto.SKU);
            ModelState.AddModelError(string.Empty, "No se pudo crear el producto. Verifica los datos e intentalo nuevamente.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            return View(producto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && !p.IsDeleted);
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
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
                return View(producto);
            }

            var productoDb = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && !p.IsDeleted);
            if (productoDb is null)
            {
                return NotFound();
            }

            productoDb.SKU = producto.SKU;
            productoDb.Nombre = producto.Nombre;
            productoDb.Descripcion = producto.Descripcion;
            productoDb.Marca = producto.Marca;
            productoDb.Proveedor = producto.Proveedor;
            productoDb.Almacen = producto.Almacen;
            productoDb.ImagenUrl = await GuardarImagenAsync(imagenArchivo) ?? producto.ImagenUrl ?? productoDb.ImagenUrl;
            productoDb.CostoCompra = producto.CostoCompra;
            productoDb.PrecioUnitario = producto.PrecioUnitario;
            productoDb.UnidadDeMedida = producto.UnidadDeMedida;
            productoDb.Peso = producto.Peso;
            productoDb.Dimensiones = producto.Dimensiones;
            productoDb.StockActual = producto.StockActual;
            productoDb.StockMinimo = producto.StockMinimo;
            productoDb.RequiereInventario = producto.RequiereInventario;
            productoDb.Activo = producto.Activo;
            productoDb.CategoriaId = producto.CategoriaId;
            productoDb.FechaActualizacion = DateTime.UtcNow;
            productoDb.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Conflicto de concurrencia al actualizar el producto {ProductoId}.", id);
            ModelState.AddModelError(string.Empty, "El producto fue modificado por otro usuario. Vuelve a cargarlo e intentalo otra vez.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
            return View(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el producto {ProductoId}.", id);
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el producto.");
            ViewBag.Categorias = await ObtenerCategoriasSelectListAsync(producto.CategoriaId);
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
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && !p.IsDeleted);
        if (producto is null)
        {
            return NotFound();
        }

        producto.Activo = !producto.Activo;
        producto.FechaActualizacion = DateTime.UtcNow;
        producto.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";
        await _context.SaveChangesAsync();

        TempData["Success"] = producto.Activo ? "Producto activado correctamente." : "Producto inactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, int nuevoStock)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.ProductoId == id && !p.IsDeleted);
        if (producto is null)
        {
            return NotFound();
        }

        if (!producto.RequiereInventario)
        {
            TempData["Error"] = "Los servicios no manejan stock.";
            return RedirectToAction(nameof(Index));
        }

        producto.StockActual = Math.Max(0, nuevoStock);
        producto.FechaActualizacion = DateTime.UtcNow;
        producto.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Stock ajustado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(int id)
    {
        var producto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.ProductoId == id && !p.IsDeleted);
        if (producto is null)
        {
            return NotFound();
        }

        producto.ProductoId = 0;
        producto.SKU = $"{producto.SKU}-COPIA";
        producto.Nombre = $"{producto.Nombre} (Copia)";
        producto.FechaCreacion = DateTime.UtcNow;
        producto.FechaActualizacion = null;
        producto.UsuarioCreacion = User.Identity?.Name ?? "Sistema";
        producto.UsuarioActualizacion = null;

        _context.Productos.Add(producto);
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
            .Where(p => productoIds.Contains(p.ProductoId) && !p.IsDeleted)
            .ToListAsync();

        foreach (var producto in productos)
        {
            switch (accionMasiva)
            {
                case "activar":
                    producto.Activo = true;
                    break;
                case "inactivar":
                    producto.Activo = false;
                    break;
                case "categoria" when categoriaMasivaId.HasValue:
                    producto.CategoriaId = categoriaMasivaId.Value;
                    break;
                case "stock" when stockMasivo.HasValue && producto.RequiereInventario:
                    producto.StockActual = Math.Max(0, stockMasivo.Value);
                    break;
            }

            producto.FechaActualizacion = DateTime.UtcNow;
            producto.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";
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

                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted);
                var esNuevo = producto is null;
                producto ??= new Producto
                {
                    SKU = sku,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioCreacion = User.Identity?.Name ?? "Sistema",
                    Activo = true
                };

                producto.Nombre = fila.Cell(2).GetString().Trim();
                producto.Descripcion = fila.Cell(3).GetString().Trim();
                producto.CategoriaId = await ResolverCategoriaIdAsync(fila.Cell(4).GetString().Trim());
                producto.Marca = fila.Cell(5).GetString().Trim();
                producto.Proveedor = fila.Cell(6).GetString().Trim();
                producto.Almacen = fila.Cell(7).GetString().Trim();
                producto.UnidadDeMedida = string.IsNullOrWhiteSpace(fila.Cell(8).GetString()) ? "pieza" : fila.Cell(8).GetString().Trim();
                producto.CostoCompra = ObtenerDecimal(fila.Cell(9));
                producto.PrecioUnitario = ObtenerDecimal(fila.Cell(10));
                producto.StockActual = ObtenerEntero(fila.Cell(11));
                producto.StockMinimo = ObtenerEntero(fila.Cell(12));
                producto.RequiereInventario = !string.Equals(fila.Cell(13).GetString().Trim(), "Servicio", StringComparison.OrdinalIgnoreCase);
                producto.FechaActualizacion = DateTime.UtcNow;
                producto.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";

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
            .OrderBy(p => p.Nombre)
            .Take(100)
            .ToListAsync();

        var texto = new StringBuilder();
        texto.AppendLine("Catalogo de Productos");
        texto.AppendLine($"Generado: {DateTime.Now:g}");
        texto.AppendLine();

        foreach (var producto in productos)
        {
            texto.AppendLine($"{producto.SKU} | {producto.Nombre} | {producto.Categoria?.Nombre} | {producto.EstadoStock} | {producto.PrecioUnitario:C}");
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

            producto.IsDeleted = true;
            producto.Activo = false;
            producto.FechaActualizacion = DateTime.UtcNow;
            producto.UsuarioActualizacion = User.Identity?.Name ?? "Sistema";
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
            .Include(producto => producto.Categoria)
            .AsNoTracking()
            .Where(producto => !producto.IsDeleted);
    }

    private static IQueryable<Producto> AplicarFiltros(IQueryable<Producto> consulta, ProductoListViewModel filtros)
    {
        var busqueda = filtros.Busqueda ?? filtros.NombreOSKU;
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim().ToLower();
            consulta = consulta.Where(producto =>
                producto.SKU.ToLower().Contains(termino)
                || producto.Nombre.ToLower().Contains(termino)
                || (producto.Descripcion != null && producto.Descripcion.ToLower().Contains(termino)));
        }

        consulta = filtros.FiltroRapido switch
        {
            "activos" => consulta.Where(producto => producto.Activo),
            "inactivos" => consulta.Where(producto => !producto.Activo),
            "productos" => consulta.Where(producto => producto.RequiereInventario),
            "servicios" => consulta.Where(producto => !producto.RequiereInventario),
            "bajo-stock" => consulta.Where(producto => producto.RequiereInventario && producto.StockActual > 0 && producto.StockActual <= producto.StockMinimo),
            "agotados" => consulta.Where(producto => producto.RequiereInventario && producto.StockActual <= 0),
            _ => consulta
        };

        if (filtros.CategoriaId.HasValue)
        {
            consulta = consulta.Where(producto => producto.CategoriaId == filtros.CategoriaId.Value);
        }

        var activo = filtros.Activo ?? filtros.SoloActivos;
        if (activo.HasValue)
        {
            consulta = consulta.Where(producto => producto.Activo == activo.Value);
        }

        var esServicio = filtros.EsServicio ?? filtros.SoloServicios;
        if (esServicio.HasValue)
        {
            consulta = consulta.Where(producto => producto.RequiereInventario != esServicio.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Proveedor))
        {
            consulta = consulta.Where(producto => producto.Proveedor == filtros.Proveedor);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Almacen))
        {
            consulta = consulta.Where(producto => producto.Almacen == filtros.Almacen);
        }

        if (filtros.PrecioMinimo.HasValue)
        {
            consulta = consulta.Where(producto => producto.PrecioUnitario >= filtros.PrecioMinimo.Value);
        }

        if (filtros.PrecioMaximo.HasValue)
        {
            consulta = consulta.Where(producto => producto.PrecioUnitario <= filtros.PrecioMaximo.Value);
        }

        if (filtros.BajoStock)
        {
            consulta = consulta.Where(producto => producto.RequiereInventario && producto.StockActual <= producto.StockMinimo);
        }

        if (filtros.FechaCreacionDesde.HasValue)
        {
            consulta = consulta.Where(producto => producto.FechaCreacion.Date >= filtros.FechaCreacionDesde.Value.Date);
        }

        if (filtros.FechaCreacionHasta.HasValue)
        {
            consulta = consulta.Where(producto => producto.FechaCreacion.Date <= filtros.FechaCreacionHasta.Value.Date);
        }

        return consulta;
    }

    private async Task<ProductoKpiViewModel> ObtenerKpisAsync()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return new ProductoKpiViewModel
        {
            TotalProductos = productos.Count(p => p.RequiereInventario),
            TotalServicios = productos.Count(p => !p.RequiereInventario),
            ProductosBajoStock = productos.Count(p => p.RequiereInventario && p.StockActual > 0 && p.StockActual <= p.StockMinimo),
            ProductosAgotados = productos.Count(p => p.RequiereInventario && p.StockActual <= 0),
            ValorTotalInventario = productos.Sum(p => p.ValorInventario),
            ProductosActivos = productos.Count(p => p.Activo),
            ProductosInactivos = productos.Count(p => !p.Activo)
        };
    }

    private async Task CargarCombosAsync(ProductoListViewModel model)
    {
        model.Categorias = await ObtenerCategoriasSelectListAsync(model.CategoriaId);
        model.Proveedores = new SelectList(await ObtenerValoresUnicosAsync(p => p.Proveedor), model.Proveedor);
        model.Almacenes = new SelectList(await ObtenerValoresUnicosAsync(p => p.Almacen), model.Almacen);
        model.RegistrosPorPaginaOpciones = new SelectList(new[] { 10, 25, 50, 100 }, model.RegistrosPorPagina);
    }

    private async Task<List<string>> ObtenerValoresUnicosAsync(Func<Producto, string?> selector)
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        return productos
            .Select(selector)
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .Distinct()
            .OrderBy(valor => valor)
            .ToList()!;
    }

    private async Task<SelectList> ObtenerCategoriasSelectListAsync(int? categoriaSeleccionadaId = null)
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Activo)
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync();

        return new SelectList(categorias, nameof(Categoria.CategoriaId), nameof(Categoria.Nombre), categoriaSeleccionadaId);
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

    private async Task<int> ResolverCategoriaIdAsync(string nombreCategoria)
    {
        if (string.IsNullOrWhiteSpace(nombreCategoria))
        {
            return await _context.Categorias.Select(c => c.CategoriaId).FirstAsync();
        }

        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Nombre == nombreCategoria);
        if (categoria is not null)
        {
            return categoria.CategoriaId;
        }

        categoria = new Categoria { Nombre = nombreCategoria, Activo = true };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria.CategoriaId;
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
