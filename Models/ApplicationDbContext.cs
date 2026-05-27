using Microsoft.EntityFrameworkCore;

namespace SyS_ERP.Models
{
    /// <summary>
    /// Contexto principal de Entity Framework Core para SyS_ERP.
    /// Enfoque Database-First / Híbrido: los DbSet se irán añadiendo
    /// conforme se realice el scaffold de cada tabla de la base de datos.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── Módulos ──────────────────────────────────────────────────────────
        // Descomentar y mapear cada entidad al hacer scaffold de la BD:
        // public DbSet<Cliente>    Clientes    { get; set; } = null!;
        // public DbSet<Proveedor>  Proveedores { get; set; } = null!;
        // public DbSet<Producto>   Productos   { get; set; } = null!;
        // public DbSet<Venta>      Ventas      { get; set; } = null!;
        // public DbSet<Factura>    Facturas    { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aquí se aplican configuraciones Fluent API cuando sea necesario.
        }
    }
}
