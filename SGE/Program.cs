using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Load embedded appsettings.json configuration
var assembly = Assembly.GetExecutingAssembly();
using (var stream = assembly.GetManifestResourceStream("SGE.appsettings.json"))
{
    if (stream != null)
    {
        builder.Configuration.AddJsonStream(stream);
    }
}

var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Configure Antiforgery to read the token from the RequestVerificationToken header
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});


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

builder.Services.AddTransient<SGE.Services.IEmailService, SGE.Services.EmailService>();

// ── Registro de Conexión de Base de Datos y Servicios del Sistema/Finanzas ────
builder.Services.AddScoped<System.Data.IDbConnection>(sp => 
    new Npgsql.NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<SGE.Services.ISgeDbConnectionFactory, SGE.Services.SgeDbConnectionFactory>();
builder.Services.AddScoped<SGE.Services.IFinanzasDataService, SGE.Services.FinanzasDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
