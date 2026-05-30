using SGE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── Módulo de Gestión de Usuarios ──────────────────────────────────────────
// Fábrica de conexión (lee ConnectionStrings:SGE de appsettings.json)
builder.Services.AddSingleton<DbConnectionFactory>();

// Servicios del módulo de usuarios
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<PermisosService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
