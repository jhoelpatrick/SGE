-- SQL ADD-ON PATCH FOR SGE DATABASE SCHEMA
-- RUN THIS SCRIPT ON TOP OF THE CURRENT POSTGRESQL DATABASE TO ADD MISSING COLUMNS AND TABLES

-- ==========================================
-- 1. MODIFICAR SCHEMA rrhh_recursos
-- ==========================================

-- Agregar columnas necesarias a rrhh_recursos.empleados
ALTER TABLE rrhh_recursos.empleados ADD COLUMN IF NOT EXISTS cargo VARCHAR(100) NULL DEFAULT 'Colaborador';
ALTER TABLE rrhh_recursos.empleados ADD COLUMN IF NOT EXISTS departamento VARCHAR(100) NULL DEFAULT 'Administración';
ALTER TABLE rrhh_recursos.empleados ADD COLUMN IF NOT EXISTS tienehijos BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE rrhh_recursos.empleados ADD COLUMN IF NOT EXISTS fechaingreso DATE NULL DEFAULT CURRENT_DATE;

-- Agregar columnas necesarias a rrhh_recursos.feriados
ALTER TABLE rrhh_recursos.feriados ADD COLUMN IF NOT EXISTS tipo VARCHAR(30) NULL DEFAULT 'Nacional';
ALTER TABLE rrhh_recursos.feriados ADD COLUMN IF NOT EXISTS recuperable BOOLEAN NOT NULL DEFAULT FALSE;

-- ==========================================
-- 2. MODIFICAR SCHEMA rrhh_nomina
-- ==========================================

-- Agregar columnas necesarias a rrhh_nomina.conceptos
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS afectacalculo BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS esremunerativo BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS obligatorio BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS afectaneto BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS porcentaje NUMERIC(5,2) NOT NULL DEFAULT 0.00;
ALTER TABLE rrhh_nomina.conceptos ADD COLUMN IF NOT EXISTS tipo VARCHAR(30) NULL;

-- ==========================================
-- 3. CREAR NUEVAS TABLAS DE CONFIGURACIÓN Y RESUMEN
-- ==========================================

-- Tabla de planillas resumen para la UI
CREATE TABLE IF NOT EXISTS rrhh_nomina.planillas_resumen (
    codigo VARCHAR(30) PRIMARY KEY,
    periodo VARCHAR(50) NOT NULL,
    fechacierre DATE NOT NULL,
    empleados INT NOT NULL DEFAULT 0,
    totalbruto NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    totaldescuentos NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    totalneto NUMERIC(12,2) GENERATED ALWAYS AS (totalbruto - totaldescuentos) STORED,
    estado VARCHAR(30) NOT NULL DEFAULT 'En Proceso'
);

-- Tabla de parámetros generales
CREATE TABLE IF NOT EXISTS rrhh_nomina.parametros_generales (
    paramid INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    empresa VARCHAR(150) NOT NULL DEFAULT 'Mi Empresa SAC',
    moneda VARCHAR(30) NOT NULL DEFAULT 'Soles (S/)',
    diacierreplanilla INT NOT NULL DEFAULT 30,
    diapagoplanilla INT NOT NULL DEFAULT 30,
    calchorasextrasauto BOOLEAN NOT NULL DEFAULT TRUE,
    inclferiadosasist BOOLEAN NOT NULL DEFAULT FALSE
);

-- Tabla de rangos de renta (Quinta Categoría)
CREATE TABLE IF NOT EXISTS rrhh_nomina.rangos_renta (
    rangoid INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    desde NUMERIC(12,2) NOT NULL,
    hasta NUMERIC(12,2) NULL,
    tasa NUMERIC(5,2) NOT NULL,
    montofijo NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    estaactivo BOOLEAN NOT NULL DEFAULT TRUE
);

-- Tabla de bancos
CREATE TABLE IF NOT EXISTS rrhh_nomina.bancos_config (
    bancoid INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    nombre VARCHAR(100) NOT NULL,
    codigo VARCHAR(20) NOT NULL,
    moneda VARCHAR(30) NOT NULL,
    cuentaprincipal VARCHAR(50) NOT NULL,
    estaactivo BOOLEAN NOT NULL DEFAULT TRUE
);

-- ==========================================
-- 4. POBLAR TABLAS CON SEMILLA DE DATOS MAESTROS (ON CONFLICT DO NOTHING)
-- ==========================================

-- Ubigeos en rrhh_recursos (copiar de comercial.ubigeos o insertar por defecto)
INSERT INTO rrhh_recursos.ubigeos (ubigeoid, departamento, provincia, distrito) VALUES
('150101', 'Lima', 'Lima', 'Lima'),
('150122', 'Lima', 'Lima', 'Miraflores'),
('150131', 'Lima', 'Lima', 'San Isidro'),
('150140', 'Lima', 'Lima', 'Santiago de Surco'),
('070101', 'Callao', 'Callao', 'Callao'),
('040101', 'Arequipa', 'Arequipa', 'Arequipa')
ON CONFLICT (ubigeoid) DO NOTHING;

-- Administradoras de pensiones (se correspondencia de afpid 1 a 5)
INSERT INTO rrhh_recursos.administradoras_pensiones (afpid, codigosunat, nombre, tipo, estaactivo) OVERRIDING SYSTEM VALUE VALUES
(1, '21', 'AFP Integra', 'afp', true),
(2, '22', 'AFP Hábitat', 'afp', true),
(3, '23', 'AFP Prima', 'afp', true),
(4, '24', 'AFP Profuturo', 'afp', true),
(5, '11', 'ONP', 'onp', true)
ON CONFLICT (afpid) DO NOTHING;

-- Regímenes laborales
INSERT INTO rrhh_recursos.regimenes_laborales (regimenlaboralid, codigosunat, nombre, estaactivo) OVERRIDING SYSTEM VALUE VALUES
(1, '01', 'Régimen 728 (privado)', true),
(2, '02', 'Régimen 276 (público)', true),
(3, '03', 'MYPE', true),
(4, '04', 'CAS', true)
ON CONFLICT (regimenlaboralid) DO NOTHING;

-- Centros de costos
INSERT INTO rrhh_recursos.centros_costos (centrocostoid, codigo, nombre, descripcion, responsable, estaactivo) OVERRIDING SYSTEM VALUE VALUES
(1, 'TI', 'TI', 'Área de TI y Desarrollo', 'Jhoel Patrick', true),
(2, 'CONT', 'Contabilidad', 'Área de Contabilidad y Finanzas', 'Ana Ramos', true),
(3, 'RRHH', 'RRHH', 'Área de Recursos Humanos', 'Sofía Castro', true),
(4, 'FIN', 'Finanzas', 'Área de Finanzas y Presupuestos', 'Carlos León', true),
(5, 'MKT', 'Marketing', 'Área de Marketing y Ventas', 'Lucía Díaz', true),
(6, 'ADM', 'Administración', 'Dirección Administrativa', 'Jaime Ortiz', true),
(7, 'OPE', 'Operaciones', 'Operaciones y Logística', 'Pedro Solís', true)
ON CONFLICT (centrocostoid) DO NOTHING;

-- Parámetros generales
INSERT INTO rrhh_nomina.parametros_generales (paramid, empresa, moneda, diacierreplanilla, diapagoplanilla, calchorasextrasauto, inclferiadosasist) OVERRIDING SYSTEM VALUE VALUES
(1, 'Mi Empresa SAC', 'Soles (S/)', 30, 30, true, false)
ON CONFLICT (paramid) DO NOTHING;

-- Rangos de renta por defecto
INSERT INTO rrhh_nomina.rangos_renta (desde, hasta, tasa, montofijo, estaactivo) VALUES
(0.00, 27025.00, 8.00, 0.00, true),
(27025.01, 54050.00, 14.00, 2162.00, true),
(54050.01, 94587.50, 17.00, 5945.50, true),
(94587.51, 189175.00, 20.00, 12836.88, true),
(189175.01, NULL, 30.00, 31754.38, true);

-- Bancos por defecto
INSERT INTO rrhh_nomina.bancos_config (nombre, codigo, moneda, cuentaprincipal, estaactivo) VALUES
('Banco de Crédito del Perú', 'BCP', 'Soles (S/)', '191-3456789-0-12', true),
('BBVA Continental', 'BBVA', 'Soles (S/)', '0011-0123-45678901', true),
('Interbank', 'IBK', 'Soles (S/)', '200-3001234567', true),
('Scotiabank', 'SCOTIA', 'Soles (S/)', '100-2003456789', true);

-- Feriados por defecto
INSERT INTO rrhh_recursos.feriados (fecha, descripcion, tipo, recuperable, estaactivo) VALUES
('2026-01-01', 'Año Nuevo', 'Nacional', false, true),
('2026-05-01', 'Día del Trabajo', 'Nacional', false, true),
('2026-07-28', 'Fiestas Patrias', 'Nacional', false, true),
('2026-07-29', 'Fiestas Patrias', 'Nacional', false, true),
('2026-12-25', 'Navidad', 'Nacional', false, true)
ON CONFLICT (fecha) DO NOTHING;

-- Conceptos por defecto
INSERT INTO rrhh_nomina.conceptos (codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo, afectacalculo, esremunerativo, obligatorio, afectaneto, porcentaje, tipo) VALUES
('0121', 'Sueldo Básico', 'SUELDO_BAS', 'ingreso_remunerativo', true, true, true, true, false, true, 0.00, 'Fijo'),
('0201', 'Asignación Familiar', 'ASIG_FAM', 'ingreso_remunerativo', true, true, true, true, false, true, 0.00, 'Fijo'),
('0105', 'Horas Extras 25%', 'HE_25', 'ingreso_remunerativo', false, true, true, true, false, true, 0.00, 'Variable'),
('0106', 'Horas Extras 35%', 'HE_35', 'ingreso_remunerativo', false, true, true, true, false, true, 0.00, 'Variable'),
('0402', 'Gratificación Legal', 'GRAT_LEG', 'ingreso_no_remunerativo', false, true, true, false, false, true, 0.00, 'Fijo'),
('0902', 'Bonificación Extraordinaria (Ley 29351)', 'BONI_EXT', 'ingreso_no_remunerativo', false, true, true, false, false, true, 0.00, 'Fijo'),
('0804', 'Essalud (Aporte Empleador)', 'ESSALUD_EMP', 'aporte_empleador', true, true, true, false, false, false, 9.00, 'Fijo')
ON CONFLICT (codigosunat) DO NOTHING;

-- Descuentos por defecto
INSERT INTO rrhh_nomina.conceptos (codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo, afectacalculo, esremunerativo, obligatorio, afectaneto, porcentaje, tipo) VALUES
('0601', 'ONP (Sistema Nacional de Pensiones)', 'ONP', 'descuento', true, true, true, false, true, true, 13.00, 'Obligatorio'),
('0602', 'AFP Integra - Comisión sobre Flujo', 'AFP_INT_F', 'descuento', true, true, true, false, true, true, 12.80, 'Obligatorio'),
('0603', 'AFP Hábitat - Comisión sobre Flujo', 'AFP_HAB_F', 'descuento', true, true, true, false, true, true, 12.90, 'Obligatorio'),
('0604', 'AFP Prima - Comisión sobre Flujo', 'AFP_PRI_F', 'descuento', true, true, true, false, true, true, 12.85, 'Obligatorio'),
('0605', 'AFP Profuturo - Comisión sobre Flujo', 'AFP_PRO_F', 'descuento', true, true, true, false, true, true, 12.95, 'Obligatorio'),
('0701', 'Adelanto de Sueldo', 'ADEL_SUEL', 'descuento', false, true, true, false, false, true, 0.00, 'Voluntario'),
('0702', 'Tardanzas y Faltas', 'TARD_FALT', 'descuento', false, true, true, false, false, true, 0.00, 'Voluntario')
ON CONFLICT (codigosunat) DO NOTHING;

-- Planillas resumen de ejemplo (si no hay ninguna)
INSERT INTO rrhh_nomina.planillas_resumen (codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, estado) VALUES
('PLA-2026-05', 'Mayo 2026', '2026-05-30', 5, 25000.00, 3250.00, 'Pagado'),
('PLA-2026-06', 'Junio 2026', '2026-06-30', 5, 25000.00, 3250.00, 'En Proceso')
ON CONFLICT (codigo) DO NOTHING;
