using Microsoft.EntityFrameworkCore;
using SGE.Models;

namespace SGE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ubigeo> Ubigeos => Set<Ubigeo>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<ContactoCliente> ContactosClientes => Set<ContactoCliente>();

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();

    public DbSet<ContactoProveedor> ContactosProveedores => Set<ContactoProveedor>();

    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(c => new { c.TipoDocumento, c.NumeroDocumento }).IsUnique();
            entity.HasMany(c => c.Contactos)
                .WithOne(c => c.Cliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasIndex(p => new { p.TipoDocumento, p.NumeroDocumento }).IsUnique();
            entity.HasMany(p => p.Contactos)
                .WithOne(c => c.Proveedor)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasIndex(p => p.CodigoSku).IsUnique();
        });
    }
}
