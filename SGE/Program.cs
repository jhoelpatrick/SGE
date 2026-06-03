var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// ── Repositorios del Módulo Comercial (sge_crm) ───────────────────────────────
builder.Services.AddScoped<SGE.Services.IClienteRepository,   SGE.Services.ClienteRepository>();
builder.Services.AddScoped<SGE.Services.IProductoRepository,  SGE.Services.ProductoRepository>();
builder.Services.AddScoped<SGE.Services.IProveedorRepository, SGE.Services.ProveedorRepository>();

// ── Repositorios del Módulo de Operaciones (sge_crm) ─────────────────────────
builder.Services.AddScoped<SGE.Services.IProyectoRepository,    SGE.Services.ProyectoRepository>();
builder.Services.AddScoped<SGE.Services.IVentaRepository,       SGE.Services.VentaRepository>();
builder.Services.AddScoped<SGE.Services.ICompraRepository,      SGE.Services.CompraRepository>();
builder.Services.AddScoped<SGE.Services.IFacturacionRepository, SGE.Services.FacturacionRepository>();
builder.Services.AddScoped<SGE.Services.IInventarioRepository,  SGE.Services.InventarioRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
