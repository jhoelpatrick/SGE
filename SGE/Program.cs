using SGE.Services;
using SGE.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── Módulo: Gestión de Usuarios ─────────────────────────────
// Fábrica de conexión (lee appsettings.json)
builder.Services.AddSingleton<DbConnectionFactory>();

// Servicios del módulo de usuarios — usan BD real
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<PermisosService>();

// ── Módulo: Nómina ───────────────────────────────────────────
// Repositorio Dapper compartido por NominaController
builder.Services.AddScoped<SgeDb>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=GestionUsuarios}/{action=Index}/{id?}");

app.Run();
