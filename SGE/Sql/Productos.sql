CREATE TABLE Categorias (
    CategoriaId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);

CREATE TABLE Productos (
    ProductoId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SKU NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    CategoriaId INT NOT NULL,
    Marca NVARCHAR(120) NULL,
    Proveedor NVARCHAR(150) NULL,
    Almacen NVARCHAR(120) NULL,
    ImagenUrl NVARCHAR(500) NULL,
    UnidadDeMedida NVARCHAR(50) NOT NULL,
    CostoCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioUnitario DECIMAL(18,2) NOT NULL DEFAULT 0,
    Peso DECIMAL(18,3) NOT NULL DEFAULT 0,
    Dimensiones NVARCHAR(100) NULL,
    StockActual INT NOT NULL DEFAULT 0,
    StockMinimo INT NOT NULL DEFAULT 0,
    RequiereInventario BIT NOT NULL DEFAULT 1,
    Activo BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaActualizacion DATETIME2 NULL,
    UsuarioCreacion NVARCHAR(100) NOT NULL DEFAULT 'Sistema',
    UsuarioActualizacion NVARCHAR(100) NULL,
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(CategoriaId),
    CONSTRAINT UX_Productos_SKU UNIQUE (SKU)
);

CREATE INDEX IX_Productos_Busqueda ON Productos (SKU, Nombre, CategoriaId, Activo, IsDeleted);
CREATE INDEX IX_Productos_Inventario ON Productos (RequiereInventario, StockActual, StockMinimo);
CREATE INDEX IX_Productos_ProveedorAlmacen ON Productos (Proveedor, Almacen);
