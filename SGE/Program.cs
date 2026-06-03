using Microsoft.EntityFrameworkCore;
using SGE.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Insert(0, "/Views/Comercial/{1}/{0}.cshtml");
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(30)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!await context.Database.CanConnectAsync())
        {
            logger.LogError(
                "No se pudo conectar a SQL Server. Inicie el servicio 'SQL Server (MSSQLSERVER)' y ejecute script_crm.sql.");
        }
        else
        {
            logger.LogInformation("Conexion a la base de datos sge_crm establecida correctamente.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Error al conectar con SQL Server. Verifique que el servicio este iniciado y que exista la base sge_crm.");
    }
}

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
