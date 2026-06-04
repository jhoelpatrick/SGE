-- SQL PATCH FOR SGE DATABASE SCHEMA - PART 2
-- CREATES TABLES FOR BENEFICIOS, GRATIFICACIONES, AND ESSALUD DECLARACIONES

-- ==========================================
-- 1. TABLA BENEFICIOS
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.beneficios (
    beneficioid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    categoria VARCHAR(50) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    periodicidad VARCHAR(50) NOT NULL,
    montofijo NUMERIC(12,2) NULL,
    montocadena VARCHAR(100) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_beneficios PRIMARY KEY (beneficioid)
);

-- Seed Beneficios
INSERT INTO rrhh_nomina.beneficios (codigo, nombre, categoria, tipo, periodicidad, montofijo, montocadena, activo)
VALUES
('BEN-001', 'Bono de Alimentación', 'Alimentacion', 'Bonificacion', 'Mensual', 250.00, 'S/ 250.00', true),
('BEN-002', 'Seguro EPS Salud', 'Salud', 'Beneficio', 'Mensual', 150.00, 'S/ 150.00', true),
('BEN-003', 'Movilidad IT', 'Transporte', 'Subsidio', 'Variable', 100.00, 'S/ 100.00', false)
ON CONFLICT (codigo) DO NOTHING;

-- ==========================================
-- 2. TABLA GRATIFICACIONES
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.gratificaciones (
    gratificacionid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    nombre VARCHAR(120) NOT NULL,
    tipo VARCHAR(30) NOT NULL,
    periodo VARCHAR(50) NOT NULL,
    frecuencia VARCHAR(30) NOT NULL,
    porcentajemonto VARCHAR(100) NOT NULL,
    basedecalculo VARCHAR(50) NOT NULL,
    montofijo NUMERIC(12,2) NULL,
    porcentaje NUMERIC(5,2) NULL,
    fechaestimada DATE NULL,
    fechapago DATE NULL,
    estado VARCHAR(30) NOT NULL,
    empleadosaplica VARCHAR(100) NOT NULL,
    cantidadempleados INT NOT NULL,
    creadopor VARCHAR(100) NOT NULL,
    CONSTRAINT pk_gratificaciones PRIMARY KEY (gratificacionid)
);

-- Seed Gratificaciones
INSERT INTO rrhh_nomina.gratificaciones (codigo, nombre, tipo, periodo, frecuencia, porcentajemonto, basedecalculo, montofijo, porcentaje, fechaestimada, fechapago, estado, empleadosaplica, cantidadempleados, creadopor)
VALUES
('GRA-2025-07', 'Gratificación Fiestas Patrias 2025', 'Obligatoria', 'Julio 2025', 'Semestral', '100% sueldo', 'RemuneracionBasica', 3500.00, 100.00, '2025-07-15', NULL, 'Pendiente', 'Todos', 5, 'Jhoel Patrick'),
('GRA-2024-12', 'Gratificación Navidad 2024', 'Obligatoria', 'Diciembre 2024', 'Semestral', '100% sueldo', 'RemuneracionBasica', 3500.00, 100.00, '2024-12-15', '2024-12-14', 'Pagada', 'Todos', 5, 'Jhoel Patrick')
ON CONFLICT (codigo) DO NOTHING;

-- ==========================================
-- 3. TABLA ESSALUD DECLARACIONES
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.essalud_declaraciones (
    declaracionid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    periodo VARCHAR(50) NOT NULL,
    trabajadores INT NOT NULL,
    remuneracionasignable NUMERIC(12,2) NOT NULL,
    aporteessalud NUMERIC(12,2) NOT NULL,
    fechaenvio TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    estado VARCHAR(30) NOT NULL,
    nroordensunat VARCHAR(50) NULL,
    observacion TEXT NULL,
    subsidios NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    totalpagar NUMERIC(12,2) NOT NULL,
    tipo VARCHAR(30) NOT NULL DEFAULT 'Mensual',
    CONSTRAINT pk_essalud_declaraciones PRIMARY KEY (declaracionid)
);

-- Seed EsSalud Declaraciones
INSERT INTO rrhh_nomina.essalud_declaraciones (codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, fechaenvio, estado, nroordensunat, observacion, subsidios, totalpagar, tipo)
VALUES
('DEC-2025-04', 'Abril 2025', 5, 17500.00, 1575.00, '2025-05-18 10:30:00', 'Aceptada', '102456789', 'Aceptada sin observaciones', 0.00, 1575.00, 'Mensual'),
('DEC-2025-05', 'Mayo 2025', 5, 17500.00, 1575.00, '2025-06-03 14:15:00', 'Pendiente', '', '', 0.00, 1575.00, 'Mensual')
ON CONFLICT (codigo) DO NOTHING;
