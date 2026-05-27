using Microsoft.EntityFrameworkCore;
using SGE.Models;

namespace SGE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { CategoriaId = 1, Nombre = "Electronica", Activo = true },
            new Categoria { CategoriaId = 2, Nombre = "Materiales de Oficina", Activo = true },
            new Categoria { CategoriaId = 3, Nombre = "Servicios Logisticos", Activo = true });

        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                ProductoId = 1,
                SKU = "ELC-001",
                Nombre = "Scanner de codigos",
                Descripcion = "Scanner portatil para lectura de codigos de barras.",
                Marca = "Zebra",
                Proveedor = "LogiTech Supply",
                Almacen = "Principal",
                ImagenUrl = "",
                CostoCompra = 210.00m,
                PrecioUnitario = 320.00m,
                UnidadDeMedida = "pieza",
                Peso = 0.45m,
                Dimensiones = "16 x 7 x 9 cm",
                StockActual = 18,
                StockMinimo = 5,
                RequiereInventario = true,
                Activo = true,
                IsDeleted = false,
                FechaCreacion = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                FechaActualizacion = new DateTime(2026, 5, 20, 8, 30, 0, DateTimeKind.Utc),
                UsuarioCreacion = "admin",
                CategoriaId = 1
            },
            new Producto
            {
                ProductoId = 2,
                SKU = "SRV-001",
                Nombre = "Picking y embalaje",
                Descripcion = "Servicio operativo de preparacion y embalaje de pedidos.",
                Marca = "Operacion Interna",
                Proveedor = "SGE Logistics",
                Almacen = "Centro Lima",
                ImagenUrl = "",
                CostoCompra = 0m,
                PrecioUnitario = 12.50m,
                UnidadDeMedida = "servicio",
                Peso = 0m,
                Dimensiones = "N/A",
                StockActual = 0,
                StockMinimo = 0,
                RequiereInventario = false,
                Activo = true,
                IsDeleted = false,
                FechaCreacion = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
                FechaActualizacion = new DateTime(2026, 5, 12, 17, 15, 0, DateTimeKind.Utc),
                UsuarioCreacion = "admin",
                CategoriaId = 3
            },
            new Producto
            {
                ProductoId = 3,
                SKU = "OFC-014",
                Nombre = "Caja corrugada mediana",
                Descripcion = "Caja para despacho de paqueteria y almacenamiento.",
                Marca = "PackPro",
                Proveedor = "Andes Packaging",
                Almacen = "Principal",
                ImagenUrl = "",
                CostoCompra = 1.10m,
                PrecioUnitario = 2.40m,
                UnidadDeMedida = "caja",
                Peso = 0.12m,
                Dimensiones = "40 x 30 x 25 cm",
                StockActual = 3,
                StockMinimo = 20,
                RequiereInventario = true,
                Activo = true,
                IsDeleted = false,
                FechaCreacion = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc),
                FechaActualizacion = new DateTime(2026, 5, 21, 11, 45, 0, DateTimeKind.Utc),
                UsuarioCreacion = "compras",
                CategoriaId = 2
            });
    }
}
