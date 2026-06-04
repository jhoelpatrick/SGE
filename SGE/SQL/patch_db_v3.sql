-- SQL PATCH FOR SGE DATABASE SCHEMA - PART 3
-- CREATES TABLES FOR UTILIDADES, REPORTES, DECLARACIONES PDT AND HISTORIAL PAGOS

-- ==========================================
-- 1. TABLA UTILIDADES
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.utilidades (
    utilidadid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    ejerciciofiscal INT NOT NULL,
    porcentajeparticipacion NUMERIC(5,2) NOT NULL,
    utilidadnetadeclarada NUMERIC(12,2) NOT NULL,
    diascomputables INT NOT NULL,
    remuneracioncomputable NUMERIC(12,2) NOT NULL,
    montodistribuido NUMERIC(12,2) NULL,
    fechapagoestimada DATE NOT NULL,
    estado VARCHAR(30) NOT NULL,
    empleadosaplica VARCHAR(100) NOT NULL,
    cantidadempleados INT NOT NULL,
    observacion TEXT NULL,
    CONSTRAINT pk_utilidades PRIMARY KEY (utilidadid)
);

-- Seed Utilidades
INSERT INTO rrhh_nomina.utilidades (codigo, ejerciciofiscal, porcentajeparticipacion, utilidadnetadeclarada, diascomputables, remuneracioncomputable, montodistribuido, fechapagoestimada, estado, empleadosaplica, cantidadempleados, observacion)
VALUES
('UTI-2024-01', 2024, 8.00, 1500000.00, 360, 450000.00, 120000.00, '2025-05-15', 'Pagada', 'Todos', 5, 'Distribución del ejercicio 2024 exitosa.'),
('UTI-2025-01', 2025, 8.00, 2000000.00, 360, 500000.00, NULL, '2026-05-15', 'Pendiente', 'Todos', 5, 'Proyección para el ejercicio 2025.')
ON CONFLICT (codigo) DO NOTHING;

-- ==========================================
-- 2. TABLA REPORTES
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.reportes (
    reporteid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    nombre VARCHAR(150) NOT NULL,
    submodulo VARCHAR(50) NOT NULL,
    periodo VARCHAR(50) NOT NULL,
    formato VARCHAR(30) NOT NULL,
    fechageneracion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    generadopor VARCHAR(100) NOT NULL,
    estado VARCHAR(30) NOT NULL,
    filasgeneradas INT NOT NULL DEFAULT 0,
    tamanokb INT NOT NULL DEFAULT 0,
    CONSTRAINT pk_reportes PRIMARY KEY (reporteid)
);

-- Seed Reportes
INSERT INTO rrhh_nomina.reportes (codigo, nombre, submodulo, periodo, formato, fechageneracion, generadopor, estado, filasgeneradas, tamanokb)
VALUES
('REP-001', 'Resumen de Planilla de Mayo 2025', 'Planillas', 'Mayo 2025', 'PDF', '2025-06-03 18:00:00', 'Jhoel Patrick', 'Completado', 5, 245),
('REP-002', 'Listado de Beneficios Activos', 'Beneficios', 'Mayo 2025', 'CSV', '2025-06-03 18:05:00', 'Jhoel Patrick', 'Completado', 3, 12)
ON CONFLICT (codigo) DO NOTHING;

-- ==========================================
-- 3. TABLA DECLARACIONES PDT
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.declaraciones_pdt (
    declaracionid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    tipo VARCHAR(30) NOT NULL,
    periodo VARCHAR(50) NOT NULL,
    ejercicio INT NOT NULL,
    fechageneracion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fechaenvio TIMESTAMP NULL,
    estado VARCHAR(30) NOT NULL,
    nroorden VARCHAR(50) NULL,
    tieneconstancia BOOLEAN NOT NULL DEFAULT FALSE,
    usuario VARCHAR(100) NOT NULL,
    observacion TEXT NULL,
    CONSTRAINT pk_declaraciones_pdt PRIMARY KEY (declaracionid)
);

-- Seed Declaraciones PDT
INSERT INTO rrhh_nomina.declaraciones_pdt (codigo, tipo, periodo, ejercicio, fechageneracion, fechaenvio, estado, nroorden, tieneconstancia, usuario, observacion)
VALUES
('SUN-001', 'PLAME', 'Abril 2025', 2025, '2025-05-18 11:00:00', '2025-05-18 11:30:00', 'Aceptada', '202500045612', true, 'Jhoel Patrick', 'Declaración mensual aceptada por SUNAT.'),
('SUN-002', 'PDT601', 'Mayo 2025', 2025, '2025-06-03 14:30:00', NULL, 'Pendiente', NULL, false, 'Jhoel Patrick', 'Falta envío de información.')
ON CONFLICT (codigo) DO NOTHING;

-- ==========================================
-- 4. TABLA HISTORIAL PAGOS
-- ==========================================
CREATE TABLE IF NOT EXISTS rrhh_nomina.historial_pagos (
    pagoid INT GENERATED ALWAYS AS IDENTITY,
    codigo VARCHAR(30) NOT NULL UNIQUE,
    planillaconcepto VARCHAR(150) NOT NULL,
    periodo VARCHAR(50) NOT NULL,
    fechapago DATE NOT NULL,
    banco VARCHAR(50) NOT NULL,
    montopagado NUMERIC(12,2) NOT NULL,
    estado VARCHAR(30) NOT NULL,
    empleados INT NOT NULL DEFAULT 0,
    observacion TEXT NULL,
    CONSTRAINT pk_historial_pagos PRIMARY KEY (pagoid)
);

-- Seed Historial Pagos
INSERT INTO rrhh_nomina.historial_pagos (codigo, planillaconcepto, periodo, fechapago, banco, montopagado, estado, empleados, observacion)
VALUES
('PAG-001', 'Planilla Mensual - Abril 2025', 'Abril 2025', '2025-04-30', 'BCP', 17500.00, 'Pagado', 5, 'Transferencias realizadas con éxito.'),
('PAG-002', 'Bono Extraordinario - Mayo 2025', 'Mayo 2025', '2025-05-15', 'BBVA', 1250.00, 'Pagado', 5, 'Bono por desempeño de ventas.'),
('PAG-003', 'Planilla Mensual - Mayo 2025', 'Mayo 2025', '2025-05-30', 'Interbank', 17500.00, 'Pendiente', 5, 'Pendiente de firma del gerente.')
ON CONFLICT (codigo) DO NOTHING;
