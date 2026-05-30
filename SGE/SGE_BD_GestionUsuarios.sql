-- ============================================================
-- SGE – Script de base de datos para el Módulo de Gestión de Usuarios
-- Ejecutar en la base de datos: SGE
-- ============================================================

-- ── 1. Tablas base ─────────────────────────────────────────

CREATE TABLE dbo.Roles (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(255) NULL,
    EsSistema   BIT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.Modulos (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE dbo.EstadosUsuario (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE dbo.Usuarios (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    Nombre         NVARCHAR(100) NOT NULL,
    Apellido       NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(255) NOT NULL UNIQUE,
    Telefono       NVARCHAR(50)  NOT NULL DEFAULT '',
    ContrasenaHash NVARCHAR(255) NOT NULL,
    RolId          INT NOT NULL REFERENCES dbo.Roles(Id),
    EstadoId       INT NOT NULL REFERENCES dbo.EstadosUsuario(Id),
    FechaCreacion  DATETIME NOT NULL DEFAULT GETDATE(),
    MfaActivo      BIT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.RolPermisos (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    RolId       INT NOT NULL REFERENCES dbo.Roles(Id),
    ModuloId    INT NOT NULL REFERENCES dbo.Modulos(Id),
    Ver         BIT NOT NULL DEFAULT 0,
    CrearEditar BIT NOT NULL DEFAULT 0,
    Eliminar    BIT NOT NULL DEFAULT 0,
    Reportes    BIT NOT NULL DEFAULT 0,
    UNIQUE (RolId, ModuloId)
);

-- ── 2. Datos semilla ───────────────────────────────────────

INSERT INTO dbo.EstadosUsuario (Nombre) VALUES ('Activo'), ('Inactivo');

INSERT INTO dbo.Roles (Nombre, Descripcion, EsSistema) VALUES
    ('Administrador',   'Acceso total al sistema',             1),
    ('Asesor Comercial','Gestión de clientes y ventas',        0),
    ('Gerente RRHH',    'Gestión de recursos humanos',         0),
    ('Contador',        'Acceso a módulos financieros',        0);

INSERT INTO dbo.Modulos (Nombre) VALUES
    ('Clientes'), ('Ventas'), ('Productos'), ('Facturación'), ('Configuración');

-- Permisos por defecto – Administrador (acceso total)
INSERT INTO dbo.RolPermisos (RolId, ModuloId, Ver, CrearEditar, Eliminar, Reportes)
SELECT r.Id, m.Id, 1, 1, 1, 1
FROM dbo.Roles r CROSS JOIN dbo.Modulos m
WHERE r.Nombre = 'Administrador';

-- Permisos por defecto – Asesor Comercial
INSERT INTO dbo.RolPermisos (RolId, ModuloId, Ver, CrearEditar, Eliminar, Reportes)
SELECT r.Id, m.Id,
    CASE m.Nombre WHEN 'Configuración' THEN 0 ELSE 1 END,
    CASE m.Nombre WHEN 'Productos' THEN 0 WHEN 'Configuración' THEN 0 ELSE 1 END,
    0,
    CASE m.Nombre WHEN 'Facturación' THEN 0 WHEN 'Configuración' THEN 0 ELSE 1 END
FROM dbo.Roles r CROSS JOIN dbo.Modulos m
WHERE r.Nombre = 'Asesor Comercial';

-- Permisos por defecto – Gerente RRHH
INSERT INTO dbo.RolPermisos (RolId, ModuloId, Ver, CrearEditar, Eliminar, Reportes)
SELECT r.Id, m.Id,
    CASE m.Nombre WHEN 'Facturación' THEN 1 ELSE 0 END,
    0, 0,
    CASE m.Nombre WHEN 'Facturación' THEN 1 ELSE 0 END
FROM dbo.Roles r CROSS JOIN dbo.Modulos m
WHERE r.Nombre = 'Gerente RRHH';

-- Permisos por defecto – Contador
INSERT INTO dbo.RolPermisos (RolId, ModuloId, Ver, CrearEditar, Eliminar, Reportes)
SELECT r.Id, m.Id,
    CASE m.Nombre WHEN 'Configuración' THEN 0 ELSE 1 END,
    CASE m.Nombre WHEN 'Facturación'   THEN 1 ELSE 0 END,
    0,
    CASE m.Nombre WHEN 'Productos' THEN 0 WHEN 'Configuración' THEN 0 ELSE 1 END
FROM dbo.Roles r CROSS JOIN dbo.Modulos m
WHERE r.Nombre = 'Contador';

-- Usuarios de ejemplo
INSERT INTO dbo.Usuarios (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
SELECT 'Alejandro','Rodríguez','a.rodriguez@empresa.com','+52 81 1234 5678','$2a$12$placeholder',
       (SELECT Id FROM dbo.Roles WHERE Nombre='Asesor Comercial'),
       (SELECT Id FROM dbo.EstadosUsuario WHERE Nombre='Activo'),
       '2026-05-12';

INSERT INTO dbo.Usuarios (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
SELECT 'María','González','m.gonzalez@empresa.com','+52 81 9876 5432','$2a$12$placeholder',
       (SELECT Id FROM dbo.Roles WHERE Nombre='Gerente RRHH'),
       (SELECT Id FROM dbo.EstadosUsuario WHERE Nombre='Activo'),
       '2026-03-08';

INSERT INTO dbo.Usuarios (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
SELECT 'Carlos','López','c.lopez@empresa.com','+52 55 4567 8901','$2a$12$placeholder',
       (SELECT Id FROM dbo.Roles WHERE Nombre='Administrador'),
       (SELECT Id FROM dbo.EstadosUsuario WHERE Nombre='Activo'),
       '2026-01-01';

INSERT INTO dbo.Usuarios (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
SELECT 'Laura','Martínez','l.martinez@empresa.com','+52 33 2345 6789','$2a$12$placeholder',
       (SELECT Id FROM dbo.Roles WHERE Nombre='Contador'),
       (SELECT Id FROM dbo.EstadosUsuario WHERE Nombre='Inactivo'),
       '2026-02-15';

INSERT INTO dbo.Usuarios (Nombre, Apellido, Email, Telefono, ContrasenaHash, RolId, EstadoId, FechaCreacion)
SELECT 'Jorge','Flores','j.flores@empresa.com','+52 81 3456 7890','$2a$12$placeholder',
       (SELECT Id FROM dbo.Roles WHERE Nombre='Asesor Comercial'),
       (SELECT Id FROM dbo.EstadosUsuario WHERE Nombre='Activo'),
       '2026-04-20';

-- ── 3. Vista principal de usuarios ────────────────────────

CREATE OR ALTER VIEW dbo.vw_Usuarios AS
SELECT
    u.Id,
    u.Nombre,
    u.Apellido,
    u.Email,
    u.Telefono,
    r.Nombre   AS Rol,
    e.Nombre   AS Estado,
    u.FechaCreacion,
    u.MfaActivo
FROM dbo.Usuarios u
JOIN dbo.Roles           r ON r.Id = u.RolId
JOIN dbo.EstadosUsuario  e ON e.Id = u.EstadoId;
GO

-- ── 4. Vista matriz de permisos ────────────────────────────

CREATE OR ALTER VIEW dbo.vw_MatrizPermisos AS
SELECT
    r.Nombre  AS Rol,
    m.Nombre  AS NombreModulo,
    rp.Ver,
    rp.CrearEditar,
    rp.Eliminar,
    rp.Reportes
FROM dbo.RolPermisos rp
JOIN dbo.Roles   r ON r.Id = rp.RolId
JOIN dbo.Modulos m ON m.Id = rp.ModuloId;
GO

-- ── 5. Stored Procedures ───────────────────────────────────

-- sp_CambiarEstadoUsuario: alterna Activo ↔ Inactivo
CREATE OR ALTER PROCEDURE dbo.sp_CambiarEstadoUsuario
    @UsuarioId   INT,
    @RealizadoPor INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET EstadoId = CASE EstadoId WHEN 1 THEN 2 ELSE 1 END
    WHERE Id = @UsuarioId;
END;
GO

-- sp_GuardarPermisosRol: UPSERT de un permiso para un rol/módulo
CREATE OR ALTER PROCEDURE dbo.sp_GuardarPermisosRol
    @RolId      INT,
    @ModuloId   INT,
    @Ver        BIT,
    @CrearEditar BIT,
    @Eliminar   BIT,
    @Reportes   BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.RolPermisos WHERE RolId = @RolId AND ModuloId = @ModuloId)
        UPDATE dbo.RolPermisos
        SET Ver = @Ver, CrearEditar = @CrearEditar, Eliminar = @Eliminar, Reportes = @Reportes
        WHERE RolId = @RolId AND ModuloId = @ModuloId;
    ELSE
        INSERT INTO dbo.RolPermisos (RolId, ModuloId, Ver, CrearEditar, Eliminar, Reportes)
        VALUES (@RolId, @ModuloId, @Ver, @CrearEditar, @Eliminar, @Reportes);
END;
GO
