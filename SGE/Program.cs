using Microsoft.Data.SqlClient;
using System.Data;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;


AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
{
    Console.WriteLine("ERROR FATAL:");
    Console.WriteLine(error.ExceptionObject.ToString());
};

TaskScheduler.UnobservedTaskException += (sender, error) =>
{
    Console.WriteLine("ERROR TASK:");
    Console.WriteLine(error.Exception.ToString());
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");
    }

    return new SqlConnection(connectionString);
});

var app = builder.Build();

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
    name: "sistema-reportes",
    pattern: "Sistema/Reportes/{action=Index}",
    defaults: new { controller = "Reportes" });

app.MapControllerRoute(
    name: "sistema-auditoria",
    pattern: "Sistema/Auditoria/{action=Index}",
    defaults: new { controller = "Auditoria" });

app.MapControllerRoute(
    name: "sistema-configuracion",
    pattern: "Sistema/Configuracion/{action=Index}",
    defaults: new { controller = "Configuracion" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Sistema}/{action=Index}/{id?}");
app.Run();