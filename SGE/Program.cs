using SGE.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar los controladores y vistas (Solo una vez)
builder.Services.AddControllersWithViews();

// 2. Registrar tu fábrica de conexiones
builder.Services.AddScoped<ISgeDbConnectionFactory, SgeDbConnectionFactory>();

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