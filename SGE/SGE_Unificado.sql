-- ============================================================
--  SGE – Sistema de Gestión Empresarial
--  Script unificado: Nómina + Gestión de Usuarios y Roles
--  Motor:   SQL Server 2019+ / Azure SQL
--  Collation: Modern_Spanish_CI_AI
--  Norma:   3FN (Tercera Forma Normal)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SGE')
    CREATE DATABASE SGE COLLATE Modern_Spanish_CI_AI;
GO

USE SGE;
GO

-- ============================================================
--  0. LIMPIEZA (orden inverso por FK)
-- ============================================================

-- Esquema dbo (Gestión de Usuarios y Roles)
IF OBJECT_ID('dbo.AuditoriaUsuarios', 'U') IS NOT NULL DROP TABLE dbo.AuditoriaUsuarios;
IF OBJECT_ID('dbo.RolPermisos',       'U') IS NOT NULL DROP TABLE dbo.RolPermisos;
IF OBJECT_ID('dbo.Usuarios',          'U') IS NOT NULL DROP TABLE dbo.Usuarios;
IF OBJECT_ID('dbo.Permisos',          'U') IS NOT NULL DROP TABLE dbo.Permisos;
IF OBJECT_ID('dbo.Modulos',           'U') IS NOT NULL DROP TABLE dbo.Modulos;
IF OBJECT_ID('dbo.Roles',             'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.EstadosUsuario',    'U') IS NOT NULL DROP TABLE dbo.EstadosUsuario;

-- Esquemas nomina / configuracion / seguridad
IF OBJECT_ID('nomina.historial_envios_essalud', 'U') IS NOT NULL DROP TABLE nomina.historial_envios_essalud;
IF OBJECT_ID('nomina.declaraciones_sunat',      'U') IS NOT NULL DROP TABLE nomina.declaraciones_sunat;
IF OBJECT_ID('nomina.declaraciones_essalud',    'U') IS NOT NULL DROP TABLE nomina.declaraciones_essalud;
IF OBJECT_ID('nomina.pagos_planilla',           'U') IS NOT NULL DROP TABLE nomina.pagos_planilla;
IF OBJECT_ID('nomina.detalle_planilla',         'U') IS NOT NULL DROP TABLE nomina.detalle_planilla;
IF OBJECT_ID('nomina.planillas',                'U') IS NOT NULL DROP TABLE nomina.planillas;
IF OBJECT_ID('nomina.utilidades',               'U') IS NOT NULL DROP TABLE nomina.utilidades;
IF OBJECT_ID('nomina.grupos_sctr',              'U') IS NOT NULL DROP TABLE nomina.grupos_sctr;
IF OBJECT_ID('nomina.gratificaciones',          'U') IS NOT NULL DROP TABLE nomina.gratificaciones;
IF OBJECT_ID('nomina.beneficios',               'U') IS NOT NULL DROP TABLE nomina.beneficios;
IF OBJECT_ID('nomina.descuentos',               'U') IS NOT NULL DROP TABLE nomina.descuentos;
IF OBJECT_ID('nomina.conceptos',                'U') IS NOT NULL DROP TABLE nomina.conceptos;
IF OBJECT_ID('nomina.empleados',                'U') IS NOT NULL DROP TABLE nomina.empleados;
IF OBJECT_ID('configuracion.reportes',          'U') IS NOT NULL DROP TABLE configuracion.reportes;
IF OBJECT_ID('configuracion.rangos_renta',      'U') IS NOT NULL DROP TABLE configuracion.rangos_renta;
IF OBJECT_ID('configuracion.feriados',          'U') IS NOT NULL DROP TABLE configuracion.feriados;
IF OBJECT_ID('configuracion.centros_costo',     'U') IS NOT NULL DROP TABLE configuracion.centros_costo;
IF OBJECT_ID('configuracion.parametros_generales','U') IS NOT NULL DROP TABLE configuracion.parametros_generales;
IF OBJECT_ID('configuracion.bancos',            'U') IS NOT NULL DROP TABLE configuracion.bancos;
IF OBJECT_ID('seguridad.usuarios_nomina',       'U') IS NOT NULL DROP TABLE seguridad.usuarios_nomina;
GO

-- ============================================================
--  1. ESQUEMAS
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'configuracion')
    EXEC('CREATE SCHEMA configuracion');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'nomina')
    EXEC('CREATE SCHEMA nomina');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'seguridad')
    EXEC('CREATE SCHEMA seguridad');
GO

-- ============================================================
--  2. TABLAS – ESQUEMA dbo  (Usuarios, Roles y Permisos)
-- ============================================================

-- 2.1 Catálogo de estados de usuario
CREATE TABLE dbo.EstadosUsuario (
    Id     TINYINT      NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(20) NOT NULL UNIQUE
);
GO

-- 2.2 Roles del sistema
CREATE TABLE dbo.Roles (
    Id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(50)  NOT NULL UNIQUE,
    Descripcion NVARCHAR(200) NOT NULL DEFAULT '',
    EsSistema   BIT           NOT NULL DEFAULT 0,   -- 1 = no eliminable desde UI
    FechaAlta   DATETIME2     NOT NULL DEFAULT GETDATE()
);
GO

-- 2.3 Módulos funcionales del ERP
CREATE TABLE dbo.Modulos (
    Id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Codigo      NVARCHAR(30)  NOT NULL UNIQUE,
    Nombre      NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(200) NOT NULL DEFAULT '',
    Activo      BIT           NOT NULL DEFAULT 1
);
GO

-- 2.4 Matriz de permisos (Rol × Módulo)
CREATE TABLE dbo.RolPermisos (
    RolId       INT  NOT NULL,
    ModuloId    INT  NOT NULL,
    Ver         BIT  NOT NULL DEFAULT 0,
    CrearEditar BIT  NOT NULL DEFAULT 0,
    Eliminar    BIT  NOT NULL DEFAULT 0,
    Reportes    BIT  NOT NULL DEFAULT 0,
    CONSTRAINT PK_RolPermisos PRIMARY KEY (RolId, ModuloId),
    CONSTRAINT FK_RolPermisos_Roles   FOREIGN KEY (RolId)    REFERENCES dbo.Roles(Id)   ON DELETE CASCADE,
    CONSTRAINT FK_RolPermisos_Modulos FOREIGN KEY (ModuloId) REFERENCES dbo.Modulos(Id) ON DELETE CASCADE
);
GO

-- 2.5 Usuarios del sistema
CREATE TABLE dbo.Usuarios (
    Id                  INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nombre              NVARCHAR(80)   NOT NULL,
    Apellido            NVARCHAR(80)   NOT NULL,
    Email               NVARCHAR(150)  NOT NULL UNIQUE,
    Telefono            NVARCHAR(30)   NOT NULL DEFAULT '',
    -- Hash BCrypt/Argon2. NUNCA texto plano.
    ContrasenaHash      NVARCHAR(256)  NOT NULL,
    MfaActivo           BIT            NOT NULL DEFAULT 0,
    RolId               INT            NOT NULL,
    EstadoId            TINYINT        NOT NULL DEFAULT 1,
    FechaCreacion       DATETIME2      NOT NULL DEFAULT GETDATE(),
    FechaUltimoAcceso   DATETIME2      NULL,
    CONSTRAINT FK_Usuarios_Roles          FOREIGN KEY (RolId)    REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_Usuarios_EstadosUsuario FOREIGN KEY (EstadoId) REFERENCES dbo.EstadosUsuario(Id)
);
GO

-- 2.6 Auditoría de cambios en usuarios
CREATE TABLE dbo.AuditoriaUsuarios (
    Id           INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    UsuarioId    INT            NOT NULL,              -- usuario afectado
    RealizadoPor INT            NULL,                  -- NULL = sistema
    Accion       NVARCHAR(30)   NOT NULL,              -- CREAR | EDITAR | ELIMINAR | CAMBIO_ESTADO | LOGIN
    Detalle      NVARCHAR(500)  NOT NULL DEFAULT '',
    FechaHora    DATETIME2      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Auditoria_UsuarioId    FOREIGN KEY (UsuarioId)    REFERENCES dbo.Usuarios(Id),
    CONSTRAINT FK_Auditoria_RealizadoPor FOREIGN KEY (RealizadoPor) REFERENCES dbo.Usuarios(Id)
);
GO

-- ============================================================
--  3. TABLAS – ESQUEMA configuracion
-- ============================================================

CREATE TABLE configuracion.bancos (
    id               INT           IDENTITY(1,1) PRIMARY KEY,
    codigo           VARCHAR(20)   NOT NULL UNIQUE,
    nombre           NVARCHAR(100) NOT NULL,
    moneda           NVARCHAR(30)  NOT NULL DEFAULT 'Soles (S/)',
    cuenta_principal VARCHAR(30)   NULL,
    activo           BIT           NOT NULL DEFAULT 1,
    emoji            NVARCHAR(10)  NOT NULL DEFAULT N'🏦'
);
GO

CREATE TABLE configuracion.parametros_generales (
    id                     INT           IDENTITY(1,1) PRIMARY KEY,
    empresa                NVARCHAR(200) NOT NULL DEFAULT N'Mi Empresa S.A.C.',
    moneda                 NVARCHAR(30)  NOT NULL DEFAULT 'Soles (S/)',
    dia_cierre_planilla    TINYINT       NOT NULL DEFAULT 20,
    dia_pago_planilla      TINYINT       NOT NULL DEFAULT 31,
    calc_horas_extras_auto BIT           NOT NULL DEFAULT 1,
    incl_feriados_asist    BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE configuracion.feriados (
    id          INT           IDENTITY(1,1) PRIMARY KEY,
    fecha       DATE          NOT NULL,
    nombre      NVARCHAR(150) NOT NULL,
    tipo        VARCHAR(20)   NOT NULL DEFAULT 'Nacional'
        CONSTRAINT ck_feriados_tipo CHECK (tipo IN ('Nacional','Personalizado')),
    recuperable BIT           NOT NULL DEFAULT 0,
    activo      BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE configuracion.centros_costo (
    id          INT           IDENTITY(1,1) PRIMARY KEY,
    codigo      VARCHAR(20)   NOT NULL UNIQUE,
    nombre      NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(300) NULL,
    responsable NVARCHAR(150) NULL,
    activo      BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE configuracion.rangos_renta (
    id         INT            IDENTITY(1,1) PRIMARY KEY,
    desde      DECIMAL(14,2)  NOT NULL,
    hasta      DECIMAL(14,2)  NULL,          -- NULL = "en adelante"
    tasa       DECIMAL(5,2)   NOT NULL,
    monto_fijo DECIMAL(14,2)  NOT NULL DEFAULT 0,
    activo     BIT            NOT NULL DEFAULT 1
);
GO

CREATE TABLE configuracion.reportes (
    id               INT           IDENTITY(1,1) PRIMARY KEY,
    codigo           VARCHAR(30)   NOT NULL UNIQUE,
    nombre           NVARCHAR(200) NOT NULL,
    submodulo        NVARCHAR(100) NULL,
    periodo          VARCHAR(20)   NULL,
    fecha_generacion DATETIME2     NOT NULL DEFAULT GETDATE(),
    generado_por     NVARCHAR(100) NOT NULL DEFAULT 'Administrador',
    estado           VARCHAR(20)   NOT NULL DEFAULT 'Completado'
        CONSTRAINT ck_reportes_estado  CHECK (estado  IN ('Completado','En Proceso','Error')),
    formato          VARCHAR(10)   NOT NULL DEFAULT 'PDF'
        CONSTRAINT ck_reportes_formato CHECK (formato IN ('PDF','Excel','CSV')),
    filas_generadas  INT           NOT NULL DEFAULT 0,
    tamano_kb        BIGINT        NOT NULL DEFAULT 0
);
GO

-- ============================================================
--  4. TABLAS – ESQUEMA seguridad
-- ============================================================

CREATE TABLE seguridad.usuarios_nomina (
    id      INT           IDENTITY(1,1) PRIMARY KEY,
    usuario NVARCHAR(50)  NOT NULL UNIQUE,
    nombre  NVARCHAR(150) NOT NULL,
    rol     NVARCHAR(50)  NOT NULL,
    email   NVARCHAR(200) NULL,
    activo  BIT           NOT NULL DEFAULT 1,
    emoji   NVARCHAR(10)  NOT NULL DEFAULT N'👤'
);
GO

-- ============================================================
--  5. TABLAS – ESQUEMA nomina
-- ============================================================

CREATE TABLE nomina.empleados (
    id                  INT            IDENTITY(1,1) PRIMARY KEY,
    codigo              VARCHAR(20)    NOT NULL UNIQUE,

    -- Datos personales
    nombres             NVARCHAR(100)  NOT NULL,
    apellido_paterno    NVARCHAR(100)  NOT NULL,
    apellido_materno    NVARCHAR(100)  NOT NULL,
    tipo_documento      VARCHAR(10)    NOT NULL DEFAULT 'DNI'
        CONSTRAINT ck_emp_tipo_doc CHECK (tipo_documento IN ('DNI','CE','Pasaporte')),
    numero_documento    VARCHAR(20)    NOT NULL UNIQUE,
    fecha_nacimiento    DATE           NOT NULL,
    sexo                CHAR(1)        NOT NULL DEFAULT 'M'
        CONSTRAINT ck_emp_sexo CHECK (sexo IN ('M','F')),
    telefono            VARCHAR(20)    NULL,
    email               NVARCHAR(200)  NULL,
    direccion           NVARCHAR(300)  NULL,

    -- Datos laborales
    fecha_ingreso       DATE           NOT NULL,
    fecha_cese          DATE           NULL,
    cargo               NVARCHAR(150)  NOT NULL,
    departamento        NVARCHAR(100)  NOT NULL,
    centro_costo_id     INT            NOT NULL
        CONSTRAINT fk_emp_centro_costo REFERENCES configuracion.centros_costo(id),
    tipo_contrato       VARCHAR(30)    NOT NULL DEFAULT 'Indeterminado'
        CONSTRAINT ck_emp_contrato CHECK (tipo_contrato IN (
            'Indeterminado','PlazoFijo','ServiciosEspecificos','Practicante')),
    regimen_laboral     VARCHAR(20)    NOT NULL DEFAULT 'Regimen728'
        CONSTRAINT ck_emp_regimen CHECK (regimen_laboral IN (
            'Regimen728','Regimen276','Mype','CAS')),
    estado              VARCHAR(15)    NOT NULL DEFAULT 'Activo'
        CONSTRAINT ck_emp_estado CHECK (estado IN (
            'Activo','Inactivo','Vacaciones','Suspendido')),

    -- Datos remunerativos
    sueldo_base         DECIMAL(14,2)  NOT NULL,
    asignacion_familiar DECIMAL(14,2)  NOT NULL DEFAULT 0,
    tiene_hijos         BIT            NOT NULL DEFAULT 0,

    -- Previsión social
    sistema_previsional VARCHAR(20)    NOT NULL DEFAULT 'ONP'
        CONSTRAINT ck_emp_afp CHECK (sistema_previsional IN (
            'AFP_Integra','AFP_Habitat','AFP_Prima','AFP_Profuturo','ONP')),
    codigo_afp          VARCHAR(30)    NULL,
    cuspp               VARCHAR(30)    NULL,

    -- Datos bancarios
    banco_pago          VARCHAR(20)    NOT NULL DEFAULT 'BCP'
        CONSTRAINT ck_emp_banco CHECK (banco_pago IN (
            'BCP','BBVA','Interbank','Scotiabank','Efectivo','Transferencia')),
    numero_cuenta       VARCHAR(30)    NULL,
    tipo_cuenta         VARCHAR(15)    NOT NULL DEFAULT 'Ahorros'
        CONSTRAINT ck_emp_tipo_cuenta CHECK (tipo_cuenta IN ('Ahorros','Corriente')),
    cci                 VARCHAR(30)    NULL,

    -- SUNAT
    afecto_renta_5ta    BIT            NOT NULL DEFAULT 1,
    afecto_essalud      BIT            NOT NULL DEFAULT 1,

    fecha_registro      DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE nomina.conceptos (
    id              INT           IDENTITY(1,1) PRIMARY KEY,
    codigo          VARCHAR(20)   NOT NULL UNIQUE,
    nombre          NVARCHAR(150) NOT NULL,
    tipo            VARCHAR(10)   NOT NULL DEFAULT 'Fijo'
        CONSTRAINT ck_concepto_tipo CHECK (tipo IN ('Fijo','Variable')),
    afecta_calculo  BIT           NOT NULL DEFAULT 1,
    es_remunerativo BIT           NOT NULL DEFAULT 1,
    activo          BIT           NOT NULL DEFAULT 1,
    fecha_creacion  DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

CREATE TABLE nomina.descuentos (
    id          INT           IDENTITY(1,1) PRIMARY KEY,
    codigo      VARCHAR(20)   NOT NULL UNIQUE,
    nombre      NVARCHAR(150) NOT NULL,
    tipo        VARCHAR(20)   NOT NULL
        CONSTRAINT ck_desc_tipo CHECK (tipo IN ('Obligatorio','Voluntario')),
    obligatorio BIT           NOT NULL DEFAULT 0,
    afecta_neto BIT           NOT NULL DEFAULT 1,
    porcentaje  DECIMAL(5,2)  NOT NULL DEFAULT 0,
    activo      BIT           NOT NULL DEFAULT 1
);
GO

CREATE TABLE nomina.beneficios (
    id             INT           IDENTITY(1,1) PRIMARY KEY,
    codigo         VARCHAR(20)   NOT NULL UNIQUE,
    nombre         NVARCHAR(150) NOT NULL,
    categoria      VARCHAR(20)   NOT NULL
        CONSTRAINT ck_ben_cat CHECK (categoria IN (
            'Alimentacion','Transporte','Salud','Educacion','Otros')),
    tipo           VARCHAR(20)   NOT NULL
        CONSTRAINT ck_ben_tipo CHECK (tipo IN ('Beneficio','Bonificacion','Subsidio')),
    periodicidad   VARCHAR(20)   NOT NULL
        CONSTRAINT ck_ben_period CHECK (periodicidad IN (
            'Diario','Mensual','Trimestral','Anual','Unico','Variable')),
    monto_cadena   NVARCHAR(50)  NULL,
    monto_fijo     DECIMAL(14,2) NULL,
    activo         BIT           NOT NULL DEFAULT 1,
    fecha_creacion DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

CREATE TABLE nomina.gratificaciones (
    id                INT           IDENTITY(1,1) PRIMARY KEY,
    codigo            VARCHAR(20)   NOT NULL UNIQUE,
    nombre            NVARCHAR(200) NOT NULL,
    tipo              VARCHAR(20)   NOT NULL
        CONSTRAINT ck_grat_tipo CHECK (tipo IN ('Obligatoria','Voluntaria')),
    periodo           VARCHAR(20)   NULL,
    frecuencia        VARCHAR(20)   NOT NULL
        CONSTRAINT ck_grat_frec CHECK (frecuencia IN (
            'Mensual','Semestral','Anual','Unica','Variable')),
    porcentaje_monto  NVARCHAR(50)  NULL,
    monto_fijo        DECIMAL(14,2) NULL,
    porcentaje        DECIMAL(5,2)  NULL,
    base_calculo      VARCHAR(30)   NOT NULL DEFAULT 'RemuneracionBasica'
        CONSTRAINT ck_grat_base CHECK (base_calculo IN (
            'RemuneracionBasica','RemuneracionComputable','SalarioNeto',
            'Fijo','PorcentajeVariable')),
    fecha_estimada    DATE          NULL,
    fecha_pago        DATE          NULL,
    estado            VARCHAR(15)   NOT NULL DEFAULT 'Activa'
        CONSTRAINT ck_grat_estado CHECK (estado IN (
            'Activa','Pendiente','Programada','Pagada','Borrador')),
    empleados_aplica  NVARCHAR(100) NOT NULL DEFAULT 'Todos',
    cantidad_empleados INT          NOT NULL DEFAULT 0,
    creado_por        NVARCHAR(100) NOT NULL DEFAULT 'Admin',
    fecha_creacion    DATE          NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

CREATE TABLE nomina.planillas (
    id               INT            IDENTITY(1,1) PRIMARY KEY,
    codigo           VARCHAR(30)    NOT NULL UNIQUE,
    periodo          VARCHAR(20)    NOT NULL,
    empleados        INT            NOT NULL DEFAULT 0,
    total_bruto      DECIMAL(14,2)  NOT NULL DEFAULT 0,
    descuentos       DECIMAL(14,2)  NOT NULL DEFAULT 0,
    total_neto       DECIMAL(14,2)  NOT NULL DEFAULT 0,
    estado           VARCHAR(20)    NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_plan_estado CHECK (estado IN (
            'Pendiente','EnCalculo','Aprobada','Cerrada','Anulada')),
    fecha_cierre     DATE           NULL,
    total_descuentos DECIMAL(14,2)  NOT NULL DEFAULT 0,
    fecha_registro   DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE nomina.detalle_planilla (
    id                  INT            IDENTITY(1,1) PRIMARY KEY,
    codigo_planilla     VARCHAR(30)    NOT NULL
        CONSTRAINT fk_det_planilla REFERENCES nomina.planillas(codigo),
    empleado_id         INT            NOT NULL
        CONSTRAINT fk_det_empleado REFERENCES nomina.empleados(id),
    periodo             VARCHAR(20)    NOT NULL,

    -- Ingresos
    sueldo_base         DECIMAL(14,2)  NOT NULL DEFAULT 0,
    asignacion_familiar DECIMAL(14,2)  NOT NULL DEFAULT 0,
    horas_extras        DECIMAL(14,2)  NOT NULL DEFAULT 0,
    movilidad           DECIMAL(14,2)  NOT NULL DEFAULT 0,
    refrigerio          DECIMAL(14,2)  NOT NULL DEFAULT 0,
    bonif_desempenio    DECIMAL(14,2)  NOT NULL DEFAULT 0,
    otros_ingresos      DECIMAL(14,2)  NOT NULL DEFAULT 0,
    total_bruto         DECIMAL(14,2)  NOT NULL DEFAULT 0,

    -- Descuentos trabajador
    descuento_afp_onp   DECIMAL(14,2)  NOT NULL DEFAULT 0,
    comision_afp        DECIMAL(14,2)  NOT NULL DEFAULT 0,
    seguro_afp          DECIMAL(14,2)  NOT NULL DEFAULT 0,
    essalud_trabajador  DECIMAL(14,2)  NOT NULL DEFAULT 0,
    renta_5ta_categoria DECIMAL(14,2)  NOT NULL DEFAULT 0,
    sctr                DECIMAL(14,2)  NOT NULL DEFAULT 0,
    prestamos           DECIMAL(14,2)  NOT NULL DEFAULT 0,
    adelantos           DECIMAL(14,2)  NOT NULL DEFAULT 0,
    tardanzas_faltas    DECIMAL(14,2)  NOT NULL DEFAULT 0,
    otros_descuentos    DECIMAL(14,2)  NOT NULL DEFAULT 0,
    total_descuentos    DECIMAL(14,2)  NOT NULL DEFAULT 0,

    -- Cargas empleador
    essalud_empleador   DECIMAL(14,2)  NOT NULL DEFAULT 0,
    sctr_empleador      DECIMAL(14,2)  NOT NULL DEFAULT 0,

    total_neto          DECIMAL(14,2)  NOT NULL DEFAULT 0,
    estado              VARCHAR(20)    NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_det_estado CHECK (estado IN (
            'Pendiente','Calculado','Aprobado','Anulado')),
    fecha_calculo       DATETIME2      NOT NULL DEFAULT GETDATE(),
    calculado_por       NVARCHAR(100)  NOT NULL DEFAULT 'Sistema',

    CONSTRAINT uq_det_planilla_emp UNIQUE (codigo_planilla, empleado_id)
);
GO

CREATE TABLE nomina.pagos_planilla (
    id                INT            IDENTITY(1,1) PRIMARY KEY,
    codigo            VARCHAR(30)    NOT NULL UNIQUE,
    planilla_concepto NVARCHAR(200)  NULL,
    periodo           VARCHAR(20)    NOT NULL,
    fecha_pago        DATE           NOT NULL,
    banco             VARCHAR(20)    NOT NULL DEFAULT 'BCP'
        CONSTRAINT ck_pago_banco CHECK (banco IN (
            'BCP','BBVA','Interbank','Scotiabank','Efectivo','Transferencia')),
    monto_pagado      DECIMAL(14,2)  NOT NULL DEFAULT 0,
    estado            VARCHAR(15)    NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_pago_estado CHECK (estado IN (
            'Pagado','Pendiente','Anulado','EnProceso')),
    observacion       NVARCHAR(300)  NULL,
    empleados         INT            NOT NULL DEFAULT 0,
    fecha_registro    DATETIME2      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE nomina.declaraciones_essalud (
    id                     INT            IDENTITY(1,1) PRIMARY KEY,
    codigo                 VARCHAR(30)    NOT NULL UNIQUE,
    periodo                VARCHAR(20)    NOT NULL,
    trabajadores           INT            NOT NULL DEFAULT 0,
    remuneracion_asignable DECIMAL(14,2)  NOT NULL DEFAULT 0,
    aporte_essalud         DECIMAL(14,2)  NOT NULL DEFAULT 0,
    subsidios              DECIMAL(14,2)  NOT NULL DEFAULT 0,
    total_pagar            DECIMAL(14,2)  NOT NULL DEFAULT 0,
    fecha_envio            DATETIME2      NULL,
    estado                 VARCHAR(15)    NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_ess_estado CHECK (estado IN (
            'Pendiente','Enviada','Aceptada','Observada','Rechazada')),
    tipo_declaracion       VARCHAR(15)    NOT NULL DEFAULT 'Mensual'
        CONSTRAINT ck_ess_tipo CHECK (tipo_declaracion IN (
            'Mensual','Rectificatoria','Anual')),
    nro_orden_sunat        VARCHAR(50)    NULL,
    observacion            NVARCHAR(500)  NULL
);
GO

CREATE TABLE nomina.grupos_sctr (
    id           INT            IDENTITY(1,1) PRIMARY KEY,
    nivel_riesgo VARCHAR(10)    NOT NULL
        CONSTRAINT ck_sctr_nivel CHECK (nivel_riesgo IN (
            'Riesgo1','Riesgo2','Riesgo3','Riesgo4')),
    trabajadores INT            NOT NULL DEFAULT 0,
    sctr_salud   DECIMAL(14,2)  NOT NULL DEFAULT 0,
    sctr_pension DECIMAL(14,2)  NOT NULL DEFAULT 0,
    aseguradora  NVARCHAR(100)  NOT NULL DEFAULT 'RIMAC Seguros',
    activo       BIT            NOT NULL DEFAULT 1
);
GO

CREATE TABLE nomina.historial_envios_essalud (
    id          INT           IDENTITY(1,1) PRIMARY KEY,
    fecha_hora  DATETIME2     NOT NULL DEFAULT GETDATE(),
    declaracion NVARCHAR(100) NOT NULL,
    usuario     NVARCHAR(100) NOT NULL,
    estado      VARCHAR(20)   NOT NULL
        CONSTRAINT ck_hist_estado CHECK (estado IN (
            'Enviado','PendienteEnvio','ConObservaciones','Aceptado')),
    mensaje     NVARCHAR(500) NULL
);
GO

CREATE TABLE nomina.declaraciones_sunat (
    id               INT           IDENTITY(1,1) PRIMARY KEY,
    codigo           VARCHAR(30)   NOT NULL UNIQUE,
    tipo             VARCHAR(10)   NOT NULL
        CONSTRAINT ck_pdt_tipo CHECK (tipo IN ('PLAME','PDT601','AFPNet')),
    periodo          VARCHAR(20)   NOT NULL,
    ejercicio        SMALLINT      NOT NULL,
    fecha_generacion DATETIME2     NOT NULL DEFAULT GETDATE(),
    fecha_envio      DATETIME2     NULL,
    estado           VARCHAR(15)   NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_pdt_estado CHECK (estado IN (
            'Pendiente','Enviada','Aceptada','Observada','Rechazada')),
    nro_orden        VARCHAR(50)   NULL,
    tiene_constancia BIT           NOT NULL DEFAULT 0,
    usuario          NVARCHAR(100) NOT NULL DEFAULT 'Admin',
    observacion      NVARCHAR(500) NULL
);
GO

CREATE TABLE nomina.utilidades (
    id                       INT            IDENTITY(1,1) PRIMARY KEY,
    codigo                   VARCHAR(30)    NOT NULL UNIQUE,
    ejercicio_fiscal         SMALLINT       NOT NULL,
    porcentaje_participacion DECIMAL(5,2)   NOT NULL,
    utilidad_neta_declarada  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    dias_computables         INT            NOT NULL DEFAULT 360,
    remuneracion_computable  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    monto_distribuido        DECIMAL(18,2)  NULL,
    fecha_pago_estimada      DATE           NOT NULL,
    fecha_pago_real          DATE           NULL,
    estado                   VARCHAR(15)    NOT NULL DEFAULT 'Pendiente'
        CONSTRAINT ck_util_estado CHECK (estado IN (
            'Pendiente','EnCalculo','Aprobada','Pagada','Anulada')),
    empleados_aplica         NVARCHAR(100)  NOT NULL DEFAULT 'Todos',
    cantidad_empleados       INT            NOT NULL DEFAULT 0,
    observacion              NVARCHAR(500)  NULL,
    fecha_creacion           DATE           NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

-- ============================================================
--  6. ÍNDICES
-- ============================================================
CREATE INDEX IX_Usuarios_RolId    ON dbo.Usuarios(RolId);
CREATE INDEX IX_Usuarios_EstadoId ON dbo.Usuarios(EstadoId);
CREATE INDEX IX_Usuarios_Email    ON dbo.Usuarios(Email);
CREATE INDEX IX_Auditoria_UsrId   ON dbo.AuditoriaUsuarios(UsuarioId);
GO

-- ============================================================
--  7. SEED – CATÁLOGOS dbo
-- ============================================================

INSERT INTO dbo.EstadosUsuario (Id, Nombre) VALUES
    (1, 'Activo'),
    (2, 'Inactivo');

INSERT INTO dbo.Roles (Nombre, Descripcion, EsSistema) VALUES
    ('Administrador',    'Acceso total al sistema',              1),
    ('Asesor Comercial', 'Ventas, clientes y reportes básicos',  1),
    ('Gerente RRHH',     'Personal, nóminas y asistencias',      1),
    ('Contador',         'Facturación, impuestos y contabilidad',1);

INSERT INTO dbo.Modulos (Codigo, Nombre, Descripcion) VALUES
    ('Clientes',      'Clientes',         'Gestión de cartera de clientes'),
    ('Ventas',        'Ventas',           'Pedidos, cotizaciones y ventas'),
    ('Productos',     'Productos',        'Catálogo e inventario de productos'),
    ('Facturacion',   'Facturación',      'Emisión y control de facturas'),
    ('RRHH',          'Recursos Humanos', 'Personal, nómina y asistencias'),
    ('Configuracion', 'Configuración',    'Parámetros generales del sistema');
GO

-- ============================================================
--  8. SEED – MATRIZ DE PERMISOS
-- ============================================================
DECLARE
    @Admin  INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Administrador'),
    @Asesor INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Asesor Comercial'),
    @RRHH   INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Gerente RRHH'),
    @Cont   INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Contador'),
    @mCli   INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'Clientes'),
    @mVen   INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'Ventas'),
    @mProd  INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'Productos'),
    @mFact  INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'Facturacion'),
    @mRRHH  INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'RRHH'),
    @mConf  INT = (SELECT Id FROM dbo.Modulos WHERE Codigo = 'Configuracion');

-- Administrador: acceso total
INSERT INTO dbo.RolPermisos (RolId,ModuloId,Ver,CrearEditar,Eliminar,Reportes) VALUES
    (@Admin,@mCli, 1,1,1,1), (@Admin,@mVen, 1,1,1,1), (@Admin,@mProd,1,1,1,1),
    (@Admin,@mFact,1,1,1,1), (@Admin,@mRRHH,1,1,1,1), (@Admin,@mConf,1,1,1,1);

-- Asesor Comercial
INSERT INTO dbo.RolPermisos (RolId,ModuloId,Ver,CrearEditar,Eliminar,Reportes) VALUES
    (@Asesor,@mCli, 1,1,0,1), (@Asesor,@mVen, 1,1,0,1), (@Asesor,@mProd,1,0,0,0),
    (@Asesor,@mFact,1,1,0,0), (@Asesor,@mRRHH,0,0,0,0), (@Asesor,@mConf,0,0,0,0);

-- Gerente RRHH
INSERT INTO dbo.RolPermisos (RolId,ModuloId,Ver,CrearEditar,Eliminar,Reportes) VALUES
    (@RRHH,@mCli, 0,0,0,0), (@RRHH,@mVen, 0,0,0,0), (@RRHH,@mProd,0,0,0,0),
    (@RRHH,@mFact,1,0,0,1), (@RRHH,@mRRHH,1,1,0,1), (@RRHH,@mConf,0,0,0,0);

-- Contador
INSERT INTO dbo.RolPermisos (RolId,ModuloId,Ver,CrearEditar,Eliminar,Reportes) VALUES
    (@Cont,@mCli, 1,0,0,1), (@Cont,@mVen, 1,0,0,1), (@Cont,@mProd,1,0,0,0),
    (@Cont,@mFact,1,1,0,1), (@Cont,@mRRHH,0,0,0,0), (@Cont,@mConf,0,0,0,0);
GO

-- ============================================================
--  9. SEED – USUARIOS DE PRUEBA  (contraseña: Pass1234!)
--     Reemplazar el hash con BCrypt.Net.BCrypt.HashPassword()
-- ============================================================
DECLARE
    @RolAdmin  INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Administrador'),
    @RolAsesor INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Asesor Comercial'),
    @RolRRHH   INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Gerente RRHH'),
    @RolCont   INT = (SELECT Id FROM dbo.Roles WHERE Nombre = 'Contador');

DECLARE @hash NVARCHAR(256) = '$2a$12$Vz3RNfVbSqjDqq7vJwGAV.example.hash.placeholder';

INSERT INTO dbo.Usuarios
    (Nombre, Apellido, Email, Telefono, ContrasenaHash, MfaActivo, RolId, EstadoId, FechaCreacion)
VALUES
    ('Carlos',    'López',     'c.lopez@empresa.com',     '+52 55 4567 8901', @hash, 1, @RolAdmin,  1, '2026-01-01'),
    ('Alejandro', 'Rodríguez', 'a.rodriguez@empresa.com', '+52 81 1234 5678', @hash, 0, @RolAsesor, 1, '2026-05-12'),
    ('María',     'González',  'm.gonzalez@empresa.com',  '+52 81 9876 5432', @hash, 0, @RolRRHH,   1, '2026-03-08'),
    ('Laura',     'Martínez',  'l.martinez@empresa.com',  '+52 33 2345 6789', @hash, 0, @RolCont,   2, '2026-02-15'),
    ('Jorge',     'Flores',    'j.flores@empresa.com',    '+52 81 3456 7890', @hash, 0, @RolAsesor, 1, '2026-04-20');
GO

-- ============================================================
--  10. SEED – CATÁLOGOS configuracion
-- ============================================================

INSERT INTO configuracion.parametros_generales
    (empresa, moneda, dia_cierre_planilla, dia_pago_planilla, calc_horas_extras_auto, incl_feriados_asist)
VALUES
    (N'Mi Empresa S.A.C.', 'Soles (S/)', 20, 31, 1, 1);
GO

INSERT INTO configuracion.bancos (codigo, nombre, moneda, cuenta_principal, activo, emoji) VALUES
('BCP',        'Banco de Crédito del Perú', 'Soles (S/)', NULL, 1, N'🏦'),
('BBVA',       'BBVA Perú',                 'Soles (S/)', NULL, 1, N'🏦'),
('INTERBANK',  'Interbank',                 'Soles (S/)', NULL, 1, N'🏦'),
('SCOTIABANK', 'Scotiabank Perú',           'Soles (S/)', NULL, 1, N'🏦'),
('BN',         'Banco de la Nación',        'Soles (S/)', NULL, 1, N'🏦'),
('BANBIF',     'BanBif',                    'Soles (S/)', NULL, 1, N'🏦');
GO

INSERT INTO configuracion.centros_costo (codigo, nombre, descripcion, responsable, activo) VALUES
('CC-001', 'Tecnología de la Información', 'Departamento de TI y sistemas',             'Gerente TI',          1),
('CC-002', 'Finanzas y Contabilidad',      'Departamento financiero y contable',         'Gerente de Finanzas', 1),
('CC-003', 'Recursos Humanos',             'Gestión del talento humano',                 'Jefe de RRHH',        1),
('CC-004', 'Administración',               'Área administrativa y servicios generales',  'Jefe Administrativo', 1),
('CC-005', 'Marketing',                    'Departamento de marketing y comunicaciones', 'Jefe de Marketing',   1),
('CC-006', 'Operaciones',                  'Logística y operaciones',                    'Jefe de Operaciones', 1);
GO

INSERT INTO configuracion.rangos_renta (desde, hasta, tasa, monto_fijo, activo) VALUES
(0,      20700,  8,  0,     1),
(20700,  41400,  14, 1656,  1),
(41400,  82800,  17, 2484,  1),
(82800,  155400, 20, 4968,  1),
(155400, NULL,   30, 19476, 1);
GO

INSERT INTO configuracion.feriados (fecha, nombre, tipo, recuperable, activo) VALUES
('2025-01-01', 'Año Nuevo',                              'Nacional', 0, 1),
('2025-04-17', 'Jueves Santo',                           'Nacional', 0, 1),
('2025-04-18', 'Viernes Santo',                          'Nacional', 0, 1),
('2025-05-01', 'Día del Trabajo',                        'Nacional', 0, 1),
('2025-06-07', 'Batalla de Arica',                       'Nacional', 0, 1),
('2025-06-29', 'San Pedro y San Pablo',                  'Nacional', 1, 1),
('2025-07-28', 'Fiestas Patrias – Independencia',        'Nacional', 0, 1),
('2025-07-29', 'Fiestas Patrias – día 2',                'Nacional', 0, 1),
('2025-08-30', 'Santa Rosa de Lima',                     'Nacional', 1, 1),
('2025-10-08', 'Combate de Angamos',                     'Nacional', 1, 1),
('2025-11-01', 'Todos los Santos',                       'Nacional', 1, 1),
('2025-12-08', 'Inmaculada Concepción',                  'Nacional', 1, 1),
('2025-12-09', 'Batalla de Ayacucho',                    'Nacional', 0, 1),
('2025-12-25', 'Navidad',                                'Nacional', 0, 1);
GO

-- ============================================================
--  11. SEED – seguridad.usuarios_nomina
-- ============================================================
INSERT INTO seguridad.usuarios_nomina (usuario, nombre, rol, email, activo, emoji) VALUES
('admin', 'Administrador del Sistema', 'Administrador', 'admin@miempresa.com', 1, N'👤');
GO

-- ============================================================
--  12. SEED – nomina.conceptos
-- ============================================================
INSERT INTO nomina.conceptos (codigo, nombre, tipo, afecta_calculo, es_remunerativo, activo) VALUES
('CON-001', 'Sueldo Básico',              'Fijo',     1, 1, 1),
('CON-002', 'Asignación Familiar',        'Fijo',     1, 1, 1),
('CON-003', 'Horas Extras 25%',           'Variable', 1, 1, 1),
('CON-004', 'Horas Extras 35%',           'Variable', 1, 1, 1),
('CON-005', 'Bonificación por Desempeño', 'Variable', 1, 0, 1),
('CON-006', 'Movilidad',                  'Fijo',     0, 0, 1),
('CON-007', 'Refrigerio',                 'Fijo',     0, 0, 1),
('CON-008', 'Otros Ingresos',             'Variable', 1, 0, 1);
GO

-- ============================================================
--  13. SEED – nomina.descuentos
-- ============================================================
INSERT INTO nomina.descuentos (codigo, nombre, tipo, obligatorio, afecta_neto, porcentaje, activo) VALUES
('DES-001', 'AFP Integra – Jubilación',   'Obligatorio', 1, 1, 10.00, 1),
('DES-002', 'AFP Habitat – Jubilación',   'Obligatorio', 1, 1, 10.00, 1),
('DES-003', 'AFP Prima – Jubilación',     'Obligatorio', 1, 1, 10.00, 1),
('DES-004', 'AFP Profuturo – Jubilación', 'Obligatorio', 1, 1, 10.00, 1),
('DES-005', 'ONP',                        'Obligatorio', 1, 1, 13.00, 1),
('DES-006', 'Renta 5.ª Categoría',        'Obligatorio', 1, 1,  0.00, 1),
('DES-007', 'Préstamo Personal',          'Voluntario',  0, 1,  0.00, 1),
('DES-008', 'Adelanto de Sueldo',         'Voluntario',  0, 1,  0.00, 1),
('DES-009', 'Tardanzas y Faltas',         'Obligatorio', 1, 1,  0.00, 1),
('DES-010', 'Seguro de Vida Ley',         'Obligatorio', 1, 1,  0.53, 1);
GO

-- ============================================================
--  14. SEED – nomina.beneficios
-- ============================================================
INSERT INTO nomina.beneficios
    (codigo, nombre, categoria, tipo, periodicidad, monto_cadena, monto_fijo, activo) VALUES
('BEN-001', 'Vale de Alimentación',    'Alimentacion', 'Beneficio',    'Mensual',    'S/ 250.00',   250.00,  1),
('BEN-002', 'Seguro de Salud Privado', 'Salud',        'Beneficio',    'Mensual',    'S/ 180.00',   180.00,  1),
('BEN-003', 'Bono de Transporte',      'Transporte',   'Bonificacion', 'Mensual',    'S/ 150.00',   150.00,  1),
('BEN-004', 'Subsidio Educativo',      'Educacion',    'Subsidio',     'Anual',      'S/ 2,000.00', 2000.00, 1),
('BEN-005', 'Bono por Productividad',  'Otros',        'Bonificacion', 'Trimestral', 'Según Plan',  NULL,    1);
GO

-- ============================================================
--  15. SEED – nomina.empleados
-- ============================================================
INSERT INTO nomina.empleados
    (codigo, nombres, apellido_paterno, apellido_materno, tipo_documento, numero_documento,
     fecha_nacimiento, sexo, cargo, departamento, centro_costo_id,
     fecha_ingreso, tipo_contrato, regimen_laboral, estado,
     sueldo_base, asignacion_familiar, tiene_hijos,
     sistema_previsional, banco_pago, numero_cuenta, cci,
     afecto_renta_5ta, afecto_essalud)
VALUES
('EMP-001','Juan Carlos','Pérez','García','DNI','45123456',
 '1990-03-15','M','Analista de Sistemas','TI',1,
 '2020-01-06','Indeterminado','Regimen728','Activo',
 3500,102.50,1,'AFP_Integra','BCP','19100234561','00219100234561234567',1,1),

('EMP-002','María Elena','Torres','Quispe','DNI','52987654',
 '1988-07-22','F','Contadora Senior','Contabilidad',2,
 '2019-03-01','Indeterminado','Regimen728','Activo',
 4200,102.50,1,'ONP','BBVA','00110012345','01100110012345678901',1,1),

('EMP-003','Carlos Alberto','Mendoza','Ríos','DNI','38456789',
 '1985-11-08','M','Jefe de Recursos Humanos','RRHH',3,
 '2018-06-15','Indeterminado','Regimen728','Activo',
 5800,102.50,1,'AFP_Prima','Interbank','200-3012345','00320020012345678901',1,1),

('EMP-004','Ana Lucía','Vargas','Flores','DNI','70234567',
 '1995-02-14','F','Asistente Administrativo','Administración',4,
 '2022-08-01','PlazoFijo','Regimen728','Activo',
 1800,0,0,'AFP_Habitat','BCP','19200345678','00219200345678234567',1,1),

('EMP-005','Roberto','Castillo','Huamán','DNI','29876543',
 '1980-09-30','M','Gerente de Finanzas','Finanzas',2,
 '2015-01-02','Indeterminado','Regimen728','Activo',
 8500,102.50,1,'AFP_Profuturo','Scotiabank','04100456789','00901040045678901234',1,1),

('EMP-006','Luciana','Morales','Salas','DNI','61345678',
 '1993-06-05','F','Diseñadora Gráfica','Marketing',5,
 '2021-04-12','Indeterminado','Regimen728','Activo',
 2600,0,0,'AFP_Integra','BBVA','00120567890','01100120056789012345',1,1),

('EMP-007','Miguel Ángel','Paredes','Chávez','DNI','47890123',
 '1991-12-18','M','Desarrollador Backend','TI',1,
 '2020-09-01','Indeterminado','Regimen728','Activo',
 4500,0,0,'AFP_Prima','BCP','19300678901','00219300678901345678',1,1),

('EMP-008','Sofía','Reyes','Mamani','DNI','73456789',
 '1997-04-25','F','Practicante de Marketing','Marketing',5,
 '2023-02-06','Practicante','Mype','Activo',
 1025,0,0,'ONP','Interbank','200-0789012','00320020078901234567',1,1),

('EMP-009','Fernando','Gutiérrez','León','DNI','32109876',
 '1978-01-11','M','Supervisor de Logística','Operaciones',6,
 '2012-07-01','Indeterminado','Regimen728','Vacaciones',
 3200,102.50,1,'AFP_Habitat','BCP','19400890123','00219400890123456789',1,1),

('EMP-010','Patricia','Salinas','Condori','DNI','58901234',
 '1986-08-03','F','Jefa de Contabilidad','Contabilidad',2,
 '2017-11-15','Indeterminado','Regimen728','Activo',
 5200,102.50,1,'AFP_Integra','BBVA','00130901234','01100130090123456789',1,1);
GO

-- ============================================================
--  16. VISTAS
-- ============================================================

-- Usuarios con rol y estado (sin contraseña)
CREATE OR ALTER VIEW dbo.vw_Usuarios AS
    SELECT
        u.Id,
        u.Nombre,
        u.Apellido,
        u.Nombre + ' ' + u.Apellido AS NombreCompleto,
        u.Email,
        u.Telefono,
        u.MfaActivo,
        r.Nombre AS Rol,
        e.Nombre AS Estado,
        u.FechaCreacion,
        u.FechaUltimoAcceso
    FROM dbo.Usuarios       u
    JOIN dbo.Roles          r ON r.Id = u.RolId
    JOIN dbo.EstadosUsuario e ON e.Id = u.EstadoId;
GO

-- Matriz de permisos legible
CREATE OR ALTER VIEW dbo.vw_MatrizPermisos AS
    SELECT
        ro.Nombre AS Rol,
        mo.Codigo AS Modulo,
        mo.Nombre AS NombreModulo,
        rp.Ver,
        rp.CrearEditar,
        rp.Eliminar,
        rp.Reportes
    FROM dbo.RolPermisos rp
    JOIN dbo.Roles       ro ON ro.Id = rp.RolId
    JOIN dbo.Modulos     mo ON mo.Id = rp.ModuloId
    ORDER BY ro.Nombre, mo.Codigo
    OFFSET 0 ROWS;
GO

-- Empleados activos con nombre completo
CREATE OR ALTER VIEW nomina.v_empleados_activos AS
    SELECT
        e.id,
        e.codigo,
        CONCAT(e.apellido_paterno,' ',e.apellido_materno,', ',e.nombres) AS nombre_completo,
        e.numero_documento,
        e.cargo,
        e.departamento,
        cc.nombre AS centro_costo,
        e.sueldo_base,
        e.asignacion_familiar,
        CASE WHEN e.tiene_hijos = 1
             THEN e.sueldo_base + e.asignacion_familiar
             ELSE e.sueldo_base
        END AS remuneracion_computable,
        e.sistema_previsional,
        e.banco_pago,
        e.estado,
        e.regimen_laboral,
        DATEDIFF(YEAR, e.fecha_ingreso, GETDATE()) AS anos_servicio
    FROM nomina.empleados e
    JOIN configuracion.centros_costo cc ON cc.id = e.centro_costo_id
    WHERE e.estado <> 'Inactivo';
GO

-- Resumen de planillas con totales
CREATE OR ALTER VIEW nomina.v_resumen_planillas AS
    SELECT
        p.codigo,
        p.periodo,
        p.estado,
        p.fecha_cierre,
        COUNT(d.id)                                  AS empleados_detalle,
        SUM(d.total_bruto)                           AS total_bruto,
        SUM(d.total_descuentos)                      AS total_descuentos,
        SUM(d.total_neto)                            AS total_neto,
        SUM(d.essalud_empleador + d.sctr_empleador)  AS carga_social_empleador
    FROM nomina.planillas p
    LEFT JOIN nomina.detalle_planilla d ON d.codigo_planilla = p.codigo
    GROUP BY p.codigo, p.periodo, p.estado, p.fecha_cierre;
GO

-- ============================================================
--  17. STORED PROCEDURES
-- ============================================================

-- Obtener usuarios (todos o solo activos)
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerUsuarios
    @SoloActivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_Usuarios
    WHERE (@SoloActivos = 0 OR Estado = 'Activo')
    ORDER BY NombreCompleto;
END
GO

-- Alternar estado activo/inactivo de un usuario
CREATE OR ALTER PROCEDURE dbo.sp_CambiarEstadoUsuario
    @UsuarioId    INT,
    @RealizadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EstadoActual TINYINT, @NuevoEstado TINYINT;
    SELECT @EstadoActual = EstadoId FROM dbo.Usuarios WHERE Id = @UsuarioId;
    SET @NuevoEstado = CASE WHEN @EstadoActual = 1 THEN 2 ELSE 1 END;
    UPDATE dbo.Usuarios SET EstadoId = @NuevoEstado WHERE Id = @UsuarioId;
    INSERT INTO dbo.AuditoriaUsuarios (UsuarioId, RealizadoPor, Accion, Detalle)
    VALUES (@UsuarioId, @RealizadoPor, 'CAMBIO_ESTADO',
            'Estado cambiado de ' + CAST(@EstadoActual AS NVARCHAR) +
            ' a '                 + CAST(@NuevoEstado  AS NVARCHAR));
END
GO

-- Guardar (INSERT o UPDATE) permisos de un rol para un módulo
CREATE OR ALTER PROCEDURE dbo.sp_GuardarPermisosRol
    @RolId       INT,
    @ModuloId    INT,
    @Ver         BIT,
    @CrearEditar BIT,
    @Eliminar    BIT,
    @Reportes    BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.RolPermisos WHERE RolId = @RolId AND ModuloId = @ModuloId)
        UPDATE dbo.RolPermisos
        SET Ver=@Ver, CrearEditar=@CrearEditar, Eliminar=@Eliminar, Reportes=@Reportes
        WHERE RolId = @RolId AND ModuloId = @ModuloId;
    ELSE
        INSERT INTO dbo.RolPermisos (RolId,ModuloId,Ver,CrearEditar,Eliminar,Reportes)
        VALUES (@RolId,@ModuloId,@Ver,@CrearEditar,@Eliminar,@Reportes);
END
GO

-- ============================================================
--  18. VERIFICACIÓN FINAL
-- ============================================================

-- Conteo por tabla dbo
SELECT 'dbo.Roles'         AS Tabla, COUNT(*) AS Registros FROM dbo.Roles         UNION ALL
SELECT 'dbo.Modulos',               COUNT(*)              FROM dbo.Modulos        UNION ALL
SELECT 'dbo.RolPermisos',           COUNT(*)              FROM dbo.RolPermisos    UNION ALL
SELECT 'dbo.Usuarios',              COUNT(*)              FROM dbo.Usuarios       UNION ALL
SELECT 'dbo.EstadosUsuario',        COUNT(*)              FROM dbo.EstadosUsuario;
GO

-- Conteo por tabla nomina / configuracion / seguridad
SELECT s.name + '.' + t.name AS tabla, p.rows AS filas_aprox
FROM sys.tables t
JOIN sys.schemas    s ON s.schema_id = t.schema_id
JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
WHERE s.name IN ('configuracion','nomina','seguridad')
ORDER BY s.name, t.name;
GO

SELECT * FROM dbo.vw_MatrizPermisos ORDER BY Rol, Modulo;
GO

PRINT '============================================================';
PRINT ' SGE – Script unificado ejecutado correctamente.';
PRINT ' Nómina + Gestión de Usuarios y Roles cargados.';
PRINT ' Siguiente paso: configurar ConnectionString en appsettings.json';
PRINT '============================================================';
GO
