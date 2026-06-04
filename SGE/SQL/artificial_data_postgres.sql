


-- ==========================================================================================
-- 1. POBLAR: comercial.ubigeos (Sin dependencias)
-- ==========================================================================================
PRINT 'Poblando comercial.ubigeos...';
INSERT INTO comercial.ubigeos (codigoubigeo, departamento, provincia, distrito) VALUES
('150101', 'Lima', 'Lima', 'Lima'),
('150122', 'Lima', 'Lima', 'Miraflores'),
('150131', 'Lima', 'Lima', 'San Isidro'),
('150140', 'Lima', 'Lima', 'Santiago de Surco'),
('070101', 'Callao', 'Callao', 'Callao'),
('040101', 'Arequipa', 'Arequipa', 'Arequipa');


-- ==========================================================================================
-- 2. POBLAR: comercial.productos (Sin dependencias)
-- ==========================================================================================
PRINT 'Poblando comercial.productos...';
SET IDENTITY_INSERT comercial.productos ON;

INSERT INTO comercial.productos (productoid, codigosku, codigosunat, descripcion, unidadmedida, tipoafectacionigv, precioventasugerido, costopromedio, esservicio, sevende, nosevende, sefabrica, estado) VALUES
(1, 'PROD-001', '30102401', 'Fierro Corrugado de 1/2 Pulgada', 'NIO', '10', 42.5000, 31.2000, 0, 1, 0, 0, 1),
(2, 'PROD-002', '30111601', 'Cemento Sol Tipo I (Bolsa 42.5 kg)', 'NIO', '10', 28.0000, 21.5000, 0, 1, 0, 0, 1),
(3, 'PROD-003', '43211501', 'Laptop Corporativa i7 16GB RAM', 'NAR', '10', 3500.0000, 2800.0000, 0, 1, 0, 0, 1),
(4, 'SERV-001', '81101508', 'Servicio de Consultoría en Gestión de Proyectos', 'ZZ', '10', 150.0000, 0.0000, 1, 1, 0, 0, 1),
(5, 'SERV-002', '81141601', 'Servicio de Transporte Logístico Local LIMA', 'ZZ', '10', 450.0000, 320.0000, 1, 1, 0, 0, 1);

SET IDENTITY_INSERT comercial.productos OFF;


-- ==========================================================================================
-- 3. POBLAR: comercial.clientes (Depende de ubigeos)
-- ==========================================================================================
PRINT 'Poblando comercial.clientes...';
SET IDENTITY_INSERT comercial.clientes ON;

INSERT INTO comercial.clientes (clienteid, tipodocumento, numerodocumento, razonsocial, nombrecomercial, direccionfiscal, ubigeo, email, telefono, tipocliente, estado) VALUES
(1, '6', '20100456781', 'CONSTRUCTORA SAN JOSÉ S.A.C.', 'Constructora San José', 'Av. Javier Prado Este 1024', '150131', 'compras@sanjose.com.pe', '014223456', 'cliente', 1),
(2, '6', '20554433221', 'MINERA DEL SUR OPERACIONES S.A.', 'Minera Del Sur', 'Las Begonias 450 Piso 8', '150131', 'logistica@minerasur.pe', '017112000', 'cliente', 1),
(3, '6', '20887766554', 'DESARROLLOS INMOBILIARIOS LIMA S.A.C.', 'DILSA', 'Av. Benavides 2344', '150122', 'proveedores@dilsa.pe', '012445566', 'prospecto', 1),
(4, '1', '44556677', 'CARLOS ALBERTO MENDOZA RUIZ', NULL, 'Jr. Huallaga 451', '150101', 'carlos.mendoza@gmail.com', '999888777', 'prospecto', 1);

SET IDENTITY_INSERT comercial.clientes OFF;


-- ==========================================================================================
-- 4. POBLAR: comercial.contactosclientes (Depende de clientes)
-- ==========================================================================================
PRINT 'Poblando comercial.contactosclientes...';
SET IDENTITY_INSERT comercial.contactosclientes ON;

INSERT INTO comercial.contactosclientes (contactoclienteid, clienteid, nombre, cargo, telefono, email, estado) VALUES
(1, 1, 'Ing. Luis Fernando Gómez', 'Gerente de Proyectos', '987654321', 'lgomez@sanjose.com.pe', 1),
(2, 1, 'Lic. Maria Elena Paz', 'Jefa de Compras', '987112233', 'mpaz@sanjose.com.pe', 1),
(3, 2, 'Ing. Roberto Carlos Arce', 'Supervisor de Logística', '955443322', 'rarce@minerasur.pe', 1),
(4, 3, 'Diana Carolina Torres', 'Analista de Adquisiciones', '933221100', 'dtorres@dilsa.pe', 1);

SET IDENTITY_INSERT comercial.contactosclientes OFF;


-- ==========================================================================================
-- 5. POBLAR: comercial.proveedores (Depende de ubigeos)
-- ==========================================================================================
PRINT 'Poblando comercial.proveedores...';
SET IDENTITY_INSERT comercial.proveedores ON;

INSERT INTO comercial.proveedores (proveedorid, tipodocumento, numerodocumento, razonsocial, direccionfiscal, ubigeo, telefono, email, estado) VALUES
(1, '6', '20334455661', 'ACEROS INDUSTRIALES DEL PERÚ S.A.', 'Av. Argentina 4560', '070101', '014512030', 'ventas@acerosind.com.pe', 1),
(2, '6', '20112233445', 'CORPORACIÓN LOGÍSTICA TRANSVIAL S.A.C.', 'Av. Elmer Faucett 120', '070101', '015748930', 'operaciones@transvial.pe', 1),
(3, '6', '20998877665', 'DISTRIBUIDORA DE MATERIALES AREQUIPA EIRL', 'Calle Mercaderes 115', '040101', '054234567', 'ventas.aqp@distrimat.pe', 1);

SET IDENTITY_INSERT comercial.proveedores OFF;


-- ==========================================================================================
-- 6. POBLAR: comercial.contactosproveedores (Depende de proveedores)
-- ==========================================================================================
PRINT 'Poblando comercial.contactosproveedores...';
SET IDENTITY_INSERT comercial.contactosproveedores ON;

INSERT INTO comercial.contactosproveedores (contactoproveedorid, proveedorid, nombre, cargo, telefono, email, estado) VALUES
(1, 1, 'Juan Carlos Oblitas', 'Asesor Comercial Corporativo', '944556677', 'joblitas@acerosind.com.pe', 1),
(2, 2, 'Sandro Alberto Marini', 'Coordinador de Flotas', '911223344', 'smarini@transvial.pe', 1),
(3, 3, 'Patricia Pilar Zuñiga', 'Administradora General', '954778899', 'pzuniga@distrimat.pe', 1);

SET IDENTITY_INSERT comercial.contactosproveedores OFF;


-- ==========================================================================================
-- 7. POBLAR: comercial.vehiculosproveedores (Depende de proveedores)
-- ==========================================================================================
PRINT 'Poblando comercial.vehiculosproveedores...';
SET IDENTITY_INSERT comercial.vehiculosproveedores ON;

INSERT INTO comercial.vehiculosproveedores (vehiculoid, proveedorid, placa, marca, modelo, tipovehiculo, estado) VALUES
(1, 2, 'F3G-820', 'Volvo', 'FMX 460', 'Tractocamión Volquete', 1),
(2, 2, 'B4W-711', 'Scania', 'P410', 'Camión Plataforma', 1),
(3, 3, 'V1Z-943', 'Hyundai', 'HD78', 'Camión Furgón 5 Tn', 1);

SET IDENTITY_INSERT comercial.vehiculosproveedores OFF;


-- ==========================================================================================
-- 8. POBLAR: comercial.conductoresproveedores (Depende de proveedores)
-- ==========================================================================================
PRINT 'Poblando comercial.conductoresproveedores...';
SET IDENTITY_INSERT comercial.conductoresproveedores ON;

INSERT INTO comercial.conductoresproveedores (conductorid, proveedorid, nombre, tipodocumento, numerodocumento, licenciaconducir, estado) VALUES
(1, 2, 'Pedro Manuel Flores Quispe', '1', '10203040', 'Q10203040-A3C', 1),
(2, 2, 'Jorge Washington Cárdenas Vega', '1', '08123456', 'V08123456-A3B', 1),
(3, 3, 'Aurelio Segundo Condori Mamani', '1', '29456712', 'M29456712-A2B', 1);

SET IDENTITY_INSERT comercial.conductoresproveedores OFF;


PRINT '¡Módulo Comercial poblado exitosamente sin errores!';

-- ==========================================================================================
-- 1. POBLAR: operaciones.almacenes (Multi-almacén - Referencia a ubigeos)
-- ==========================================================================================
PRINT 'Poblando operaciones.almacenes...';
SET IDENTITY_INSERT operaciones.almacenes ON;

INSERT INTO operaciones.almacenes (almacenid, codigoalmacen, nombre, direccion, ubigeo, estado) VALUES
(1, 'ALM-01', 'Almacén Central Lima', 'Av. Argentina 2450', '150101', 1),
(2, 'ALM-02', 'Almacén Callao Logístico', 'Av. Néstor Gambetta km 3.5', '070101', 1),
(3, 'ALM-03', 'Almacén Regional Arequipa', 'Parque Industrial s/n', '040101', 1);

SET IDENTITY_INSERT operaciones.almacenes OFF;


-- ==========================================================================================
-- 2. POBLAR: operaciones.stockalmacen (Sin Identity - Clave primaria compuesta)
-- ==========================================================================================
PRINT 'Poblando operaciones.stockalmacen...';
INSERT INTO operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido) VALUES
(1, 1, 1500.0000, 200.0000), -- Fierro corrugado en Lima Central
(1, 2, 2300.0000, 500.0000), -- Cemento Sol en Lima Central
(2, 1, 800.0000, 0.0000),   -- Fierro corrugado en Callao
(2, 3, 45.0000, 10.0000),    -- Laptops en Callao
(3, 2, 1200.0000, 150.0000); -- Cemento Sol en Arequipa


-- ==========================================================================================
-- 3. POBLAR: operaciones.proyectos (Depende de comercial.clientes)
-- ==========================================================================================
PRINT 'Poblando operaciones.proyectos...';
SET IDENTITY_INSERT operaciones.proyectos ON;

INSERT INTO operaciones.proyectos (proyectoid, clienteid, nombreproyecto, descripcion, presupuestototal, costoreallogrado, fechainicio, fechafin, estado) VALUES
(1, 1, 'Edificio Residencial San José - Miraflores', 'Construcción de un condominio de 15 pisos.', 1500000.0000, 0.0000, '2026-01-15', '2026-12-20', 'en progreso'),
(2, 2, 'Ampliación Planta de Procesos Sur', 'Optimización de fajas transportadoras e infraestructura técnica.', 850000.0000, 0.0000, '2026-03-01', '2026-09-30', 'en progreso'),
(3, 3, 'Habilitación Urbana Lomas del Sol', 'Obras de saneamiento y pavimentación estructural.', 450000.0000, 0.0000, '2026-05-10', NULL, 'planificado');

SET IDENTITY_INSERT operaciones.proyectos OFF;


-- ==========================================================================================
-- 4. POBLAR: operaciones.proyectotareas (Depende de operaciones.proyectos)
-- ==========================================================================================
PRINT 'Poblando operaciones.proyectotareas...';
SET IDENTITY_INSERT operaciones.proyectotareas ON;

INSERT INTO operaciones.proyectotareas (tareaid, proyectoid, nombretarea, fechainicio, fechafin, porcentajeprogreso, costoestimado, estado) VALUES
(1, 1, 'Excavación y Movimiento de Tierras', '2026-01-15', '2026-02-28', 100.00, 80000.0000, 'completada'),
(2, 1, 'Cimentación y Estructuras Base', '2026-03-01', '2026-06-30', 45.50, 350000.0000, 'en ejecucion'),
(3, 2, 'Ingeniería de Detalle y Planos', '2026-03-01', '2026-04-15', 100.00, 30000.0000, 'completada'),
(4, 2, 'Montaje Electromecánico de Fajas', '2026-04-16', '2026-08-15', 15.00, 500000.0000, 'en ejecucion'),
(5, 3, 'Estudio de Impacto Ambiental', '2026-05-15', '2026-07-15', 0.00, 15000.0000, 'pendiente');

SET IDENTITY_INSERT operaciones.proyectotareas OFF;


-- ==========================================================================================
-- 5. POBLAR: operaciones.pedidosventa (Depende de clientes y proyectos)
-- ==========================================================================================
PRINT 'Poblando operaciones.pedidosventa...';
SET IDENTITY_INSERT operaciones.pedidosventa ON;

INSERT INTO operaciones.pedidosventa (pedidoid, numeropedido, clienteid, proyectoid, fechaemision, moneda, tipocambio, metodopago, cupondescuento, montobruto, montodescuento, totalneto, estado) VALUES
(1, 'PED-2026-001', 1, 1, '2026-02-10', 'pen', 1.0000, 'credito', NULL, 42500.0000, 500.0000, 42000.0000, 'aprobado'),
(2, 'PED-2026-002', 2, 2, '2026-03-15', 'usd', 3.7500, 'transferencia', NULL, 15000.0000, 0.0000, 15000.0000, 'aprobado'),
(3, 'PED-2026-003', 4, NULL, '2026-05-20', 'pen', 1.0000, 'visa', 'DSCTO10', 3500.0000, 350.0000, 3150.0000, 'pendiente');

SET IDENTITY_INSERT operaciones.pedidosventa OFF;


-- ==========================================================================================
-- 6. POBLAR: operaciones.pedidosventadetalle (Depende de pedidosventa y comercial.productos)
-- ==========================================================================================
PRINT 'Poblando operaciones.pedidosventadetalle...';
SET IDENTITY_INSERT operaciones.pedidosventadetalle ON;

INSERT INTO operaciones.pedidosventadetalle (detalledid, pedidoid, productoid, cantidad, preciounitariocongiv, descuento, totalfila) VALUES
(1, 1, 1, 1000.0000, 42.5000, 500.0000, 42000.0000), -- 1000 Fierros para Proyecto San José
(2, 2, 4, 100.0000, 150.0000, 0.0000, 15000.0000),  -- 100 horas de Consultoría (en USD) para Minera del Sur
(3, 3, 3, 1.0000, 3500.0000, 350.0000, 3150.0000);  -- 1 Laptop Corporativa para Cliente Natural

SET IDENTITY_INSERT operaciones.pedidosventadetalle OFF;


-- ==========================================================================================
-- 7. POBLAR: operaciones.ordenescompra (Depende de proveedores y proyectos)
-- ==========================================================================================
PRINT 'Poblando operaciones.ordenescompra...';
SET IDENTITY_INSERT operaciones.ordenescompra ON;

INSERT INTO operaciones.ordenescompra (ordenid, numeroorden, proveedorid, proyectoid, solicitante, fechaemision, moneda, monto_total, categoriagasto, estado) VALUES
(1, 'OC-2026-001', 1, 1, 'Ing. Luis Fernando Gómez', '2026-02-15', 'pen', 31200.0000, 'materiales', 'aprobado'),
(2, 'OC-2026-002', 2, 1, 'Lic. Maria Elena Paz', '2026-02-18', 'pen', 4500.0000, 'logistica', 'aprobado'),
(3, 'OC-2026-003', 3, 2, 'Ing. Roberto Carlos Arce', '2026-04-01', 'pen', 10750.0000, 'materiales', 'pendiente');

SET IDENTITY_INSERT operaciones.ordenescompra OFF;


-- ==========================================================================================
-- 8. POBLAR: operaciones.ordenescompradetalle (Depende de ordenescompra y productos)
-- ==========================================================================================
PRINT 'Poblando operaciones.ordenescompradetalle...';
SET IDENTITY_INSERT operaciones.ordenescompradetalle ON;

INSERT INTO operaciones.ordenescompradetalle (detalledoc, ordenid, productoid, cantidad, costounitariocongiv, totalfila) VALUES
(1, 1, 1, 1000.0000, 31.2000, 31200.0000), -- Compra de fierro a Aceros Industriales
(2, 2, 5, 10.0000, 450.0000, 4500.0000),    -- Servicio de transporte logístico local (Transvial)
(3, 3, 2, 500.0000, 21.5000, 10750.0000);   -- 500 bolsas de Cemento Sol (Distribuidora Arequipa)

SET IDENTITY_INSERT operaciones.ordenescompradetalle OFF;


-- ==========================================================================================
-- 9. POBLAR: operaciones.comprobantesfacturacion (Facturación Electrónica - Depende de pedidos y clientes)
-- ==========================================================================================
PRINT 'Poblando operaciones.comprobantesfacturacion...';
SET IDENTITY_INSERT operaciones.comprobantesfacturacion ON;

INSERT INTO operaciones.comprobantesfacturacion (comprobanteid, pedidoid, tipocomprobante, serie, correlativo, fechaemision, tipooperacionsunat, clienteid, moneda, opgravada, opinafecta, opexonerada, igv_total, importetotalneto, tipoimpuestoespecial, estadosunat) VALUES
(1, 1, '01', 'F001', '00000001', '2026-02-12', '01', 1, 'pen', 35593.2203, 0.0000, 0.0000, 6406.7797, 42000.0000, 'ninguno', 'enviado sunat'),
(2, 2, '01', 'F001', '00000002', '2026-03-16', '01', 2, 'usd', 12711.8644, 0.0000, 0.0000, 2288.1356, 15000.0000, 'ninguno', 'enviado sunat');

SET IDENTITY_INSERT operaciones.comprobantesfacturacion OFF;


-- ==========================================================================================
-- 10. POBLAR: operaciones.guiasremision (Guías SUNAT - Depende de almacenes, vehículos y conductores)
-- ==========================================================================================
PRINT 'Poblando operaciones.guiasremision...';
SET IDENTITY_INSERT operaciones.guiasremision ON;

INSERT INTO operaciones.guiasremision (guiaid, serie, correlativo, fechaemision, motivotraslado, almacenorigenid, almacendestinoid, proveedorid, vehiculoid, conductorid, pesototal, unidadmedidapeso, estadosunat) VALUES
(1, 'T001', '00000001', '2026-02-20', '04', 1, 2, NULL, 1, 1, 5000.00, 'kgm', 'aceptado'), -- Traslado entre almacenes (Lima a Callao)
(2, 'T001', '00000002', '2026-02-22', '01', 1, NULL, NULL, 2, 2, 2500.00, 'kgm', 'aceptado'); -- Guía remitente ligada a despacho de venta

SET IDENTITY_INSERT operaciones.guiasremision OFF;


-- ==========================================================================================
-- 11. POBLAR: operaciones.kardexmovimientos (Motor del Inventario)
-- ==========================================================================================
PRINT 'Poblando operaciones.kardexmovimientos...';
SET IDENTITY_INSERT operaciones.kardexmovimientos ON;

INSERT INTO operaciones.kardexmovimientos (movimientoid, almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento) VALUES
(1, 1, 1, 'ent', 'compra', 'OC-2026-001', 1000.0000, 31.2000, '2026-02-15'), -- Entrada física por abastecimiento de OC
(2, 1, 1, 'sal', 'venta', 'PED-2026-001', 500.0000, 31.2000, '2026-02-22'),  -- Salida física por despacho de pedido
(3, 1, 2, 'ent', 'ajuste', 'AJU-2026-001', 50.0000, 21.5000, '2026-05-10');   -- Entrada por ajuste de inventario (auditoría)

SET IDENTITY_INSERT operaciones.kardexmovimientos OFF;


PRINT '¡Módulo de Operaciones poblado exitosamente sin errores!';

-- ==========================================================================================
-- modulo: finanzas, contabilidad + catalogos y controles sunat
-- objetivo: insercion de datos de prueba coherentes y enlazados dinamicamente
-- base de datos: sge_crm
-- ==========================================================================================




-- ==========================================================================================
-- 1. poblar impuestos basicos de sunat
-- ==========================================================================================
print 'insertando impuestos financieros...';
insert into finanzas.impuestos (codigoimpuestosunat, nombreimpuesto, porcentaje, estado) values
('1000', 'igv - impuesto general a las ventas', 18.00, 1),
('2000', 'isc - impuesto selectivo al consumo', 0.00, 1),
('9997', 'exonerado', 0.00, 1),
('9998', 'inafecto', 0.00, 1);


-- ==========================================================================================
-- 2. plancuentas (plan contable general empresarial - pcge basico operacional)
-- ==========================================================================================
print 'insertando cuentas del plan contable peruano (pcge)...';
insert into finanzas.plancuentas (cuentacodigo, descripcion, tipocuenta, nivelint, aceptaasiento) values
('1041', 'cuentas corrientes en instituciones financieras - moneda nacional', 'activo', 4, 1),
('1042', 'cuentas corrientes en instituciones financieras - moneda extranjera', 'activo', 4, 1),
('1212', 'facturas, boletas y otros comprobantes por cobrar - emitidas en cartera', 'activo', 4, 1),
('40111', 'impuesto general a las ventas - cuenta propia', 'pasivo', 5, 1),
('4212', 'facturas, boletas y otros comprobantes por pagar - emitidas por proveedores', 'pasivo', 4, 1),
('6011', 'mercaderias - adquisiciones locales', 'gastos', 4, 1),
('70111', 'mercaderias - venta local de bienes (cuenta propia)', 'ingresos', 5, 1);


-- ==========================================================================================
-- 3. cuentasbancarias (tesoreria corporativa real)
-- ==========================================================================================
print 'insertando cuentas bancarias de la empresa...';
insert into finanzas.cuentasbancarias (banconombre, numerocuenta, cuentacciexterno, tipocuenta, moneda, saldoactual, estado) values
('bcp', '191-99887766-0-11', '002-191-0099887766011-54', 'corriente', 'pen', 45000.0000, 1),
('bbva', '0011-0123-0100456789', '011-123-000100456789-22', 'corriente', 'usd', 12500.0000, 1),
('interbank', '200-3001234567', '003-200-003001234567-15', 'caja chica', 'pen', 2500.0000, 1);


-- ==========================================================================================
-- 4. asientoscabecera y asientosdetalle (partida doble obligatoria)
-- ==========================================================================================
print 'insertando asientos contables de ejemplo (libro diario)...';
declare @v_asiento1 bigint, @v_asiento2 bigint;

-- asiento a: provision de venta (libro de ventas '14')
insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
values ('as-2026-05-0001', '2026-05-15', '14', 'por la provision de la venta de mercaderia del mes', 'f001-00000125');
set @v_asiento1 = scope_identity();

insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber) values
(@v_asiento1, '1212', 1180.0000, 0.0000),  -- cliente nos debe el total (cargo)
(@v_asiento1, '40111', 0.0000, 180.0000),  -- igv por pagar fiscal (abono)
(@v_asiento1, '70111', 0.0000, 1000.0000); -- ganancia neta o base gravada (abono)

-- asiento b: provision de compra de mercaderia/materiales (libro de compras '08')
insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
values ('as-2026-05-0002', '2026-05-18', '08', 'por la provision de la compra de suministros y mercaderia', 'f003-00004123');
set @v_asiento2 = scope_identity();

insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber) values
(@v_asiento2, '6011', 500.0000, 0.0000),   -- costo o gasto de adquisicion (cargo)
(@v_asiento2, '40111', 90.0000, 0.0000),   -- credito fiscal igv a favor (cargo)
(@v_asiento2, '4212', 0.0000, 590.0000);   -- obligacion por pagar al proveedor (abono)


-- ==========================================================================================
-- 5. movimientostesoreria (enlazado de manera segura con subconsultas)
-- ==========================================================================================
print 'insertando flujos de caja y tesoreria remitiendo a comprobantes u ordenes existentes...';
declare @v_comprobanteid int, @v_ordenid int, @v_cuentabcp int;

-- recuperamos ids de manera dinamica para evitar quiebres de llaves foraneas
select top 1 @v_cuentabcp = cuentabancariaid from finanzas.cuentasbancarias where banconombre = 'bcp';
select top 1 @v_comprobanteid = comprobanteid from operaciones.comprobantesfacturacion;
select top 1 @v_ordenid = ordenid from operaciones.ordenescompra;

insert into finanzas.movimientostesoreria (cuentabancariaid, tipoflujo, mediopagosunat, monto, comprobanteid, ordenid, glosamovimiento, fechamovimiento) values
(@v_cuentabcp, 'ing', '003', 1180.0000, @v_comprobanteid, null, 'cobro total de factura comercial liquidadas por transferencia de fondos bcp', CURRENT_TIMESTAMP),
(@v_cuentabcp, 'egr', '003', 590.0000, null, @v_ordenid, 'pago programado a proveedor logistico por orden de compra autorizada', CURRENT_TIMESTAMP);


-- ==========================================================================================
-- 6. activosfijos y depreciacion calculada
-- ==========================================================================================
print 'insertando control de inventario de activos fijos de la empresa...';
declare @v_productoid int;
select top 1 @v_productoid = productoid from comercial.productos where esservicio = 0;

insert into finanzas.activosfijos (codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada, estado) values
('act-2026-001', 'servidor dell poweredge t350 para desarrollo crm', @v_productoid, '2026-01-10', 12500.0000, 25.00, 1562.5000, 'activo'),
('act-2026-002', 'laptop asus rog zephyrus - estacion de arquitectura de software', null, '2026-02-15', 7800.0000, 25.00, 650.0000, 'activo'),
('act-2026-003', 'muebles modulares de oficina ergonómicos - sala de TI', null, '2026-03-01', 4500.0000, 10.00, 112.5000, 'activo');


-- ==========================================================================================
-- 7. sunat.declaraciones_sire (sistema integrado de registros electronicos)
-- ==========================================================================================
print 'insertando trazas fiscales sire (propuestas sunat)...';
insert into sunat.declaraciones_sire (periodo, tiporegistro, numeroticket, fechaenvio, estado_sire, nombre_archivo_exportado) values
('202604', 'rvie', 'tk-20260430-88471', '2026-05-02 08:30:00', 'aceptado', 'le202604_rvie_propuesta_oficial.zip'),
('202604', 'rce', 'tk-20260430-99124', '2026-05-02 09:15:00', 'aceptado', 'le202604_rce_propuesta_oficial.zip'),
('202605', 'rvie', null, CURRENT_TIMESTAMP, 'propuesta_sunat', null);


-- ==========================================================================================
-- 8. sunat.control_car_sire (codigo de anotacion de registro)
-- ==========================================================================================
print 'viculando comprobantes con codigos car exigidos por la sunat...';
declare @v_sire_comprobanteid int, @v_sire_ordenid int;
select top 1 @v_sire_comprobanteid = comprobanteid from operaciones.comprobantesfacturacion;
select top 1 @v_sire_ordenid = ordenid from operaciones.ordenescompra;

insert into sunat.control_car_sire (comprobanteid, ordenid, codigo_car, periodo_afectacion) values
(@v_sire_comprobanteid, null, 'car620260501000000000125f00100001', '202605'),
(null, @v_sire_ordenid, 'car6202605080000000004123f00300002', '202605');


-- ==========================================================================================
-- 9. sunat.cierres_ple (programa de libros electronicos)
-- ==========================================================================================
print 'insertando logs y hashes de auditoria de libros contables cerrados txt...';
insert into sunat.cierres_ple (periodo, codigolibrosunat, cantidad_filas, codigohash, fecha_generacion, estado_envio) values
('20260400', '050100', 142, 'd41d8cd98f00b204e9800998ecf8427eef6312a4b898ecf8427eef6312a4b123', '2026-05-10 18:00:00', '1'), -- diario
('20260400', '060100', 85, 'e10adc3949ba59abbe56e057f20f883eef6312a4b898ecf8427eef6312a4b456', '2026-05-10 18:15:00', '1'),  -- mayor
('20260400', '130100', 310, 'c33367701511b4f6020ec61ded35205eef6312a4b898ecf8427eef6312a4b789', '2026-05-10 19:00:00', '1'); -- kardex valorizado


print '*** carga completa de finanzas, contabilidad y sunat finalizada exitosamente ***';

-- ==========================================================================================
-- modulo: seguridad avanzada (datos de prueba coherentes para .net core 8)
-- objetivo: simular tokens jwt, sesiones ui, ataques de fuerza bruta y configuraciones mfa
-- base de datos: sge_crm
-- ==========================================================================================
-- ==========================================================================================
-- 1. poblar: seguridad.usuario_tokens (historial de refresh tokens jwt)
-- ==========================================================================================
print 'poblando seguridad.usuario_tokens...';
set identity_insert seguridad.usuario_tokens on;

insert into seguridad.usuario_tokens (tokenid, usuarioid, token_refresco, jwt_id, es_usado, es_revocado, fecha_creacion, fecha_expiracion) values
-- tokens del administrador (id 1): uno expirado/usado y uno activo válido
(1, 1, 'rt_mock_7f8a9b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b2c3d4e5f6a7b8c9d0e1f2a', 'jti-998877-aaaa-bbbb-cccc-111122223333', 1, 0, dateadd(day, -7, CURRENT_TIMESTAMP), dateadd(day, -6, CURRENT_TIMESTAMP)),
(2, 1, 'rt_mock_1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b', 'jti-112233-dddd-eeee-ffff-444455556666', 0, 0, CURRENT_TIMESTAMP, dateadd(day, 7, CURRENT_TIMESTAMP)),

-- tokens del usuario comercial (id 2): token revocado preventivamente por sospecha
(3, 2, 'rt_mock_z1y2x3w4v5u6t7s8r9q0p1o2n3m4l5k6j7i8h9g0f1e2d3c4b5a6z7y8x9w8v7u6', 'jti-445566-xxxx-yyyy-zzzz-777788889999', 0, 1, dateadd(day, -1, CURRENT_TIMESTAMP), dateadd(day, 6, CURRENT_TIMESTAMP)),

-- tokens del usuario operaciones (id 3): activo
(4, 3, 'rt_mock_aa11bb22cc33dd44ee55ff66gg77hh88ii99jj00kk11ll22mm33nn44oo55pp66', 'jti-778899-ffff-gggg-hhhh-000011112222', 0, 0, CURRENT_TIMESTAMP, dateadd(day, 7, CURRENT_TIMESTAMP));

set identity_insert seguridad.usuario_tokens off;


-- ==========================================================================================
-- 2. poblar: seguridad.usuario_sesiones (monitoreo de dispositivos en tiempo real para la ui)
-- ==========================================================================================
print 'poblando seguridad.usuario_sesiones...';
set identity_insert seguridad.usuario_sesiones on;

insert into seguridad.usuario_sesiones (sesionid, usuarioid, tokenid, ip_direccion, navegador, dispositivo, fecha_inicio, ultima_actividad, es_activa) values
-- admin (id 1): doble sesión activa simultánea (simula control de concurrencia)
(1, 1, 2, '192.168.1.100', 'chrome 125.0.0.0', 'windows 11 pro', dateadd(hour, -4, CURRENT_TIMESTAMP), dateadd(minute, -5, CURRENT_TIMESTAMP), 1),
(2, 1, 2, '172.16.20.45', 'safari mobile 17.4', 'iphone 15 pro', dateadd(hour, -2, CURRENT_TIMESTAMP), dateadd(minute, -15, CURRENT_TIMESTAMP), 1),

-- comercial (id 2): sesión cerrada/inactiva (debido a la revocación de su token)
(3, 2, 3, '190.235.45.12', 'firefox 126.0', 'ubuntu linux 22.04', dateadd(day, -1, CURRENT_TIMESTAMP), dateadd(day, -1, CURRENT_TIMESTAMP), 0),

-- operaciones (id 3): sesión móvil activa en ruta
(4, 3, 4, '100.12.98.174', 'chrome mobile 125.0', 'android 14 (samsung s24)', dateadd(hour, -1, CURRENT_TIMESTAMP), CURRENT_TIMESTAMP, 1);

set identity_insert seguridad.usuario_sesiones off;


-- ==========================================================================================
-- 3. poblar: seguridad.usuario_intentos_login (trazabilidad perimetral / bloqueo por ip)
-- ==========================================================================================
print 'poblando seguridad.usuario_intentos_login...';
set identity_insert seguridad.usuario_intentos_login on;

insert into seguridad.usuario_intentos_login (intentoid, email_ingresado, ip_direccion, exito, motivo_fallo, fecha_hora) values
-- intento exitoso ordinario
(1, 'admin@empresa.com.pe', '192.168.1.100', 1, null, dateadd(hour, -4, CURRENT_TIMESTAMP)),

-- simulación de ataque de fuerza bruta desde una ip externa (mismo atacante errando claves continuamente)
(2, 'admin@empresa.com.pe', '203.0.113.84', 0, 'contrasena_incorrecta', dateadd(minute, -25, CURRENT_TIMESTAMP)),
(3, 'root@empresa.com.pe', '203.0.113.84', 0, 'contrasena_incorrecta', dateadd(minute, -23, CURRENT_TIMESTAMP)),
(4, 'supervisor@empresa.com.pe', '203.0.113.84', 0, 'contrasena_incorrecta', dateadd(minute, -21, CURRENT_TIMESTAMP)),
(5, 'ventas@empresa.com.pe', '203.0.113.84', 0, 'contrasena_incorrecta', dateadd(minute, -20, CURRENT_TIMESTAMP)),

-- simulación de fallo por token mfa caducado o mal digitado
(6, 'lgomez@sanjose.com.pe', '190.235.45.12', 0, 'mfa_expirado', dateadd(minute, -45, CURRENT_TIMESTAMP));

set identity_insert seguridad.usuario_intentos_login off;


-- ==========================================================================================
-- 4. poblar: seguridad.usuario_historial_passwords (política estricta de no repetición)
-- ==========================================================================================
print 'poblando seguridad.usuario_historial_passwords...';
set identity_insert seguridad.usuario_historial_passwords on;

insert into seguridad.usuario_historial_passwords (historialid, usuarioid, contrasena_hash, fecha_cambio) values
-- historial de cambios del administrador (evita que vuelva a usar claves antiguas)
(1, 1, '$2y$12$mcrxexhbyfzknykyq5gupuxyisxemvszh0z9y1aqvbg2j/kyvjtuq', dateadd(month, -6, CURRENT_TIMESTAMP)),
(2, 1, '$2y$12$ktb6gqndv3gkrkdfmvdheuzfuk57huxq6rcc04asw0fvw.hgh4oeq', dateadd(month, -3, CURRENT_TIMESTAMP)),

-- historial de cambios del usuario de finanzas
(3, 4, '$2y$12$fbgswwre112kdaakscvfgexuozbpxmrrk99kqa99vfbvdxxwqqsqs', dateadd(month, -2, CURRENT_TIMESTAMP));

set identity_insert seguridad.usuario_historial_passwords off;


-- ==========================================================================================
-- 5. poblar: seguridad.usuario_mfa (seguridad multi-factor obligatoria para flujos críticos)
-- ==========================================================================================
print 'poblando seguridad.usuario_mfa...';
set identity_insert seguridad.usuario_mfa on;

insert into seguridad.usuario_mfa (mfaid, usuarioid, proveedor, secreto_mfa, es_activo, codigos_respaldo, fecha_configuracion) values
-- administrador: totp activo (google authenticator hmac-sha1 semilla base32 simulada)
(1, 1, 'totp', 'nxw64z3p7x47vuzled2g65zqnuyx2k3z', 1, 'backup_hash_1aef38,backup_hash_4f7e12,backup_hash_99bc87', dateadd(month, -3, CURRENT_TIMESTAMP)),

-- comercial: configurado con sms en modo inactivo o pendiente de validación inicial
(2, 2, 'sms', 'encrypted_seed_string_for_sms_gateway_flow_002', 0, null, CURRENT_TIMESTAMP),

-- finanzas: totp activo (cuenta de alta responsabilidad fiscal)
(3, 4, 'totp', 'mzxw6ytboi2dcmrtguzdgnbvgaydamzq', 1, 'backup_hash_ff4411,backup_hash_ee5522', dateadd(month, -1, CURRENT_TIMESTAMP));

set identity_insert seguridad.usuario_mfa off;


print '*** inicialización del esquema de seguridad avanzada completado ***';

-- ==========================================================================================
-- modulo: capital humano, motor de nominas y transversal sistema
-- objetivo: insercion de maestros sunat, legajos, asistencias biometricas y planillas de mayo 2026
-- base de datos: sge_crm
-- ==========================================================================================
-- ==========================================================================================
-- esquema: rrhh_recursos
-- ==========================================================================================

print 'poblando rrhh_recursos.centros_costos...';
set identity_insert rrhh_recursos.centros_costos on;
insert into rrhh_recursos.centros_costos (centrocostoid, codigo, nombre, descripcion, responsable, estaactivo) values
(1, 'cc01', 'gerencia comercial', 'centro de costos para el area de ventas y crm', 'luis fernando gomez', 1),
(2, 'cc02', 'operaciones y logistica', 'centro de costos de planta, almacenes y distribucion', 'maria elena paz', 1),
(3, 'cc03', 'administracion y finanzas', 'centro de costos administrativo y control fiscal', 'roberto carlos arce', 1);
set identity_insert rrhh_recursos.centros_costos off;

print 'poblando rrhh_recursos.feriados...';
set identity_insert rrhh_recursos.feriados on;
insert into rrhh_recursos.feriados (feriadoid, fecha, descripcion, estaactivo) values
(1, '2026-01-01', 'año nuevo', 1),
(2, '2026-04-02', 'jueves santo', 1),
(3, '2026-04-03', 'viernes santo', 1),
(4, '2026-05-01', 'dia del trabajo', 1);
set identity_insert rrhh_recursos.feriados off;

print 'poblando rrhh_recursos.usuarios_nomina...';
set identity_insert rrhh_recursos.usuarios_nomina on;
insert into rrhh_recursos.usuarios_nomina (usuarionominaid, usuario, nombrecompleto, rol, correo, estaactivo) values
(1, 'admin.sge', 'administrador central crm', 'administrador', 'admin@empresa.com.pe', 1),
(2, 'mpaz.operaciones', 'maria elena paz', 'jefe de operaciones', 'mpaz@sanjose.com.pe', 1);
set identity_insert rrhh_recursos.usuarios_nomina off;

print 'poblando rrhh_recursos.empleados...';
set identity_insert rrhh_recursos.empleados on;
insert into rrhh_recursos.empleados (empleadoid, tipodocumento, numerodocumento, nombres, apellidopaterno, apellidomaterno, fechanacimiento, sexo, correopersonal, correocorporativo, telefonocelular, centrocostoid, estaactivo) values
(1, 'dni', '11111111', 'luis fernando', 'gomez', 'silva', '1988-05-12', 'm', 'luis.gomez.personal@gmail.com', 'lgomez@sanjose.com.pe', '999888777', 1, 1),
(2, 'dni', '22222222', 'maria elena', 'paz', 'rodriguez', '1990-08-24', 'f', 'maria.paz.personal@gmail.com', 'mpaz@sanjose.com.pe', '999777666', 2, 1),
(3, 'dni', '33333333', 'roberto carlos', 'arce', 'mendoza', '1985-03-15', 'm', 'roberto.arce.personal@gmail.com', 'rarce@minerasur.pe', '999666555', 3, 1);
set identity_insert rrhh_recursos.empleados off;

print 'poblando rrhh_recursos.contratos...';
set identity_insert rrhh_recursos.contratos on;
insert into rrhh_recursos.contratos (contratoid, empleadoid, tipocontrato, fechainicio, fechafin, sueldobase, estaactivo) values
(1, 1, 'plazo indeterminado', '2024-01-01', null, 5500.00, 1),
(2, 2, 'plazo fijo sujeto a modalidad', '2025-01-01', '2026-12-31', 4800.00, 1),
(3, 3, 'plazo indeterminado', '2023-06-01', null, 6200.00, 1);
set identity_insert rrhh_recursos.contratos off;

print 'poblando rrhh_recursos.ubigeos...';
insert into rrhh_recursos.ubigeos (ubigeoid, departamento, provincia, distrito) values
('150101', 'lima', 'lima', 'lima'),
('150132', 'lima', 'lima', 'san juan de lurigancho'),
('150103', 'lima', 'lima', 'ate');

print 'poblando rrhh_recursos.regimenes_laborales...';
set identity_insert rrhh_recursos.regimenes_laborales on;
insert into rrhh_recursos.regimenes_laborales (regimenlaboralid, codigosunat, nombre, estaactivo) values
(1, '0021', 'regimen general de la actividad privada (dl 728)', 1),
(2, '0022', 'regimen mype - pequeña empresa', 1);
set identity_insert rrhh_recursos.regimenes_laborales off;

print 'poblando rrhh_recursos.administradoras_pensiones...';
set identity_insert rrhh_recursos.administradoras_pensiones on;
insert into rrhh_recursos.administradoras_pensiones (afpid, codigosunat, nombre, tipo, estaactivo) values
(1, '0001', 'afp integra', 'afp', 1),
(2, '0002', 'afp prima', 'afp', 1),
(3, '0003', 'onp - oficina de normalizacion previsional', 'onp', 1);
set identity_insert rrhh_recursos.administradoras_pensiones off;

print 'poblando rrhh_recursos.datos_laborales_empleados...';
insert into rrhh_recursos.datos_laborales_empleados (empleadoid, regimenlaboralid, afpid, tipocomision, cuspp, ubigeodomicilio, direccion, cuentasueldo, bancosueldoid, cuentacts, bancoctsid) values
(1, 1, 1, 'mixta', '111111lgomez', '150101', 'av. larco 123', 'cc-111222333-1', 1, 'cts-111222-1', 1),
(2, 1, 2, 'flujo', '222222mpaz', '150132', 'jr. los jazmines 456', 'cc-444555666-2', 2, 'cts-444555-2', 2),
(3, 1, 3, 'no_aplica', null, '150103', 'av. javier prado 789', 'cc-777888999-3', 1, 'cts-777888-3', 1);

print 'poblando rrhh_recursos.derechohabientes...';
set identity_insert rrhh_recursos.derechohabientes on;
insert into rrhh_recursos.derechohabientes (derechohabienteid, empleadoid, vinculofamiliar, tipodocumento, numerodocumento, nombres, apellidopaterno, apellidomaterno, fechanacimiento) values
(1, 1, 'hijo', 'dni', '44444444', 'mateo', 'gomez', 'castro', '2018-10-05'),
(2, 2, 'conyuge', 'dni', '55555555', 'carlos', 'solis', 'vega', '1989-02-14');
set identity_insert rrhh_recursos.derechohabientes off;

print 'poblando rrhh_recursos.turnos...';
set identity_insert rrhh_recursos.turnos on;
insert into rrhh_recursos.turnos (turnoid, nombre, horaingreso, horasalida, toleranciaingreso, tiemporefrigerio, estaactivo) values
(1, 'turno administrativo central', '08:00:00', '17:00:00', 10, 60, 1),
(2, 'turno operaciones y planta', '07:00:00', '16:00:00', 5, 60, 1);
set identity_insert rrhh_recursos.turnos off;

print 'poblando rrhh_recursos.marcaciones_biometricos...';
set identity_insert rrhh_recursos.marcaciones_biometricos on;
insert into rrhh_recursos.marcaciones_biometricos (marcacionid, empleadoid, fechahora, tipo, dispositivo) values
(1, 1, '2026-05-25 07:58:00', 'ingreso', 'biometrico_puerta_principal'),
(2, 1, '2026-05-25 13:00:00', 'salida_ref', 'biometrico_puerta_principal'),
(3, 1, '2026-05-25 14:02:00', 'retorno_ref', 'biometrico_puerta_principal'),
(4, 1, '2026-05-25 17:01:00', 'salida', 'biometrico_puerta_principal'),
(5, 2, '2026-05-25 06:54:00', 'ingreso', 'biometrico_almacen_planta'),
(6, 2, '2026-05-25 16:02:00', 'salida', 'biometrico_almacen_planta');
set identity_insert rrhh_recursos.marcaciones_biometricos off;

print 'poblando rrhh_recursos.asistencias_diarias...';
set identity_insert rrhh_recursos.asistencias_diarias on;
insert into rrhh_recursos.asistencias_diarias (asistenciadiariaid, empleadoid, fecha, turnoid, horaingresoreal, horasalidareal, minutostardanza, minutosextras25, minutosextras35, minutosnocturnas, estadoasistencia) values
(1, 1, '2026-05-25', 1, '07:58:00', '17:01:00', 0, 0, 0, 0, 'asistio'),
(2, 2, '2026-05-25', 2, '06:54:00', '16:02:00', 0, 0, 0, 0, 'asistio'),
(3, 3, '2026-05-25', 1, '08:15:00', '17:00:00', 5, 0, 0, 0, 'asistio'); -- llego 8:15 con tolerancia de 10 min = 5 de tardanza neta
set identity_insert rrhh_recursos.asistencias_diarias off;

print 'poblando rrhh_recursos.tipos_licencias...';
set identity_insert rrhh_recursos.tipos_licencias on;
insert into rrhh_recursos.tipos_licencias (tipolicenciaid, codigosunat, descripcion, congocehaber, essubsidiado, estaactivo) values
(1, '0001', 'licencia por enfermedad comprobada / descanso medico', 1, 1, 1),
(2, '0002', 'licencia por paternidad de ley', 1, 0, 1);
set identity_insert rrhh_recursos.tipos_licencias off;

print 'poblando rrhh_recursos.solicitudes_licencias...';
set identity_insert rrhh_recursos.solicitudes_licencias on;
insert into rrhh_recursos.solicitudes_licencias (solicitudlicenciaid, empleadoid, tipolicenciaid, fechainicio, fechafin, estadosolicitud, usuariosolicitaid, sustento) values
(1, 1, 1, '2026-05-10', '2026-05-12', 'aprobada', 1, 'descanso medico firmado por colegiado essalud');
set identity_insert rrhh_recursos.solicitudes_licencias off;

print 'poblando rrhh_recursos.periodos_vacacionales...';
set identity_insert rrhh_recursos.periodos_vacacionales on;
insert into rrhh_recursos.periodos_vacacionales (periodovacacionalid, empleadoid, anioperiodo, diasganados, diasgozados, diasvendidos, estaabierto) values
(1, 1, 2025, 30, 15, 0, 0),
(2, 2, 2025, 30, 0, 0, 1),
(3, 3, 2025, 30, 30, 0, 0);
set identity_insert rrhh_recursos.periodos_vacacionales off;

print 'poblando rrhh_recursos.programacion_vacaciones...';
set identity_insert rrhh_recursos.programacion_vacaciones on;
insert into rrhh_recursos.programacion_vacaciones (programacionvacacionid, periodovacacionalid, fechainicio, fechafin, estadosolicitud) values
(1, 1, '2026-02-01', '2026-02-15', 'ejecutada');
set identity_insert rrhh_recursos.programacion_vacaciones off;


-- ==========================================================================================
-- esquema: rrhh_nomina
-- ==========================================================================================

print 'poblando rrhh_nomina.tasas_afp...';
set identity_insert rrhh_nomina.tasas_afp on;
insert into rrhh_nomina.tasas_afp (tasasafpid, afpid, anio, mes, porcentajeaporte, porcentajeseguro, porcentajecomisionflujo, porcentajecomisionmixta, topeprimaseguro) values
(1, 1, 2026, 5, 10.00, 1.84, 1.60, 1.55, 12000.00),
(2, 2, 2026, 5, 10.00, 1.84, 1.47, 1.25, 12000.00),
(3, 3, 2026, 5, 13.00, 0.00, 0.00, 0.00, 0.00);
set identity_insert rrhh_nomina.tasas_afp off;

print 'poblando rrhh_nomina.conceptos...';
set identity_insert rrhh_nomina.conceptos on;
insert into rrhh_nomina.conceptos (conceptoid, codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo) values
(1, '0121', 'sueldo o remuneracion basica', 'sueldo', 'ingreso_remunerativo', 1, 1),
(2, '0201', 'asignacion familiar de ley', 'asig_fam', 'ingreso_remunerativo', 1, 1),
(3, '0601', 'comision y retencion de fondo de pensiones', 'ret_pension', 'descuento', 0, 1),
(4, '0804', 'essalud seguro regular regularizado', 'essalud', 'aporte_empleador', 1, 1);
set identity_insert rrhh_nomina.conceptos off;

print 'poblando rrhh_nomina.conceptos_empleados_fijos...';
set identity_insert rrhh_nomina.conceptos_empleados_fijos on;
insert into rrhh_nomina.conceptos_empleados_fijos (conceptoempleadofid, empleadoid, conceptoid, montofijo, explicacion, estaactivo) values
(1, 1, 1, 5500.00, 'sueldo base contractual del gerente comercial', 1),
(2, 1, 2, 102.50, 'beneficio asignacion familiar por hijo menor', 1),
(3, 2, 1, 4800.00, 'sueldo base de jefe de operaciones planta', 1),
(4, 3, 1, 6200.00, 'sueldo contractual de contador central', 1);
set identity_insert rrhh_nomina.conceptos_empleados_fijos off;

print 'poblando rrhh_nomina.periodos_planillas...';
set identity_insert rrhh_nomina.periodos_planillas on;
insert into rrhh_nomina.periodos_planillas (periodoplanillaid, anio, mes, tipoplanilla, fechainicio, fechafin, estadoperiodo) values
(1, 2026, 5, 'regular_mensual', '2026-05-01', '2026-05-31', 'cerrado');
set identity_insert rrhh_nomina.periodos_planillas off;

print 'poblando rrhh_nomina.planillas_cabeceras...';
set identity_insert rrhh_nomina.planillas_cabeceras on;
insert into rrhh_nomina.planillas_cabeceras (planillacabeceraid, periodoplanillaid, fechacalculo, descripcion, estadoplanilla, usuarioid) values
(1, 1, '2026-05-28 18:30:00', 'planilla ordinaria de haberes correspondientes al mes de mayo 2026', 'cerrada', 1);
set identity_insert rrhh_nomina.planillas_cabeceras off;

print 'poblando rrhh_nomina.planillas_detalles...';
set identity_insert rrhh_nomina.planillas_detalles on;
insert into rrhh_nomina.planillas_detalles (planilladetalleid, planillacabeceraid, empleadoid, diaslaborados, diassubsidiados, diasnolaborados, totalingresosremunerativos, totalingresosnoremunerativos, totaldescuentos, totalaportesempleador, netopagar, codigohashboleta) values
(1, 1, 1, 30, 0, 0, 5602.50, 0.00, 750.17, 504.23, 4852.33, 'd27289ca112e4f55998a6234cc9988ff112233445566778899aabbccddeeff12'),
(2, 1, 2, 30, 0, 0, 4800.00, 0.00, 637.92, 432.00, 4162.08, 'e39485bb223f5a66009b7345dd0011aa2233445566778899aabbccddeeff3456'),
(3, 1, 3, 30, 0, 0, 6200.00, 0.00, 806.00, 558.00, 5394.00, 'f40596cc334a6b77110c8456ee1122bb33445566778899aabbccddeeff567890');
set identity_insert rrhh_nomina.planillas_detalles off;

print 'poblando rrhh_nomina.planillas_conceptos_detalles...';
set identity_insert rrhh_nomina.planillas_conceptos_detalles on;
insert into rrhh_nomina.planillas_conceptos_detalles (planillaconceptodetalleid, planilladetalleid, conceptoid, montocalculado) values
-- desglose de luis gomez (detalle 1)
(1, 1, 1, 5500.00),
(2, 1, 2, 102.50),
(3, 1, 3, 750.17),
(4, 1, 4, 504.23),
-- desglose de maria paz (detalle 2)
(5, 2, 1, 4800.00),
(6, 2, 3, 637.92),
(7, 2, 4, 432.00),
-- desglose de roberto arce (detalle 3)
(8, 3, 1, 6200.00),
(9, 3, 3, 806.00),
(10, 3, 4, 558.00);
set identity_insert rrhh_nomina.planillas_conceptos_detalles off;

print 'poblando rrhh_nomina.rentas_quinta_acumuladas...';
set identity_insert rrhh_nomina.rentas_quinta_acumuladas on;
insert into rrhh_nomina.rentas_quinta_acumuladas (rentaquintaid, empleadoid, anio, ingresosacumuladosbrutos, impuestoretendidoacumulado, ingresosotroslempleadores) values
(1, 1, 2026, 28012.50, 1200.00, 0.00),
(2, 2, 2026, 24000.00, 850.00, 0.00);
set identity_insert rrhh_nomina.rentas_quinta_acumuladas off;


-- ==========================================================================================
-- esquema: sistema (corregido para cumplir con varchar(20) en codigo)
-- ==========================================================================================

print 'poblando sistema.parametros...';
set identity_insert sistema.parametros on;
insert into sistema.parametros (parametroid, clave, valor, descripcion, categoria, fechamodificacion) values
(1, 'sunat_uit_valor', '5150', 'valor oficial de la unidad impositiva tributaria para el año fiscal 2026', 'tributario', CURRENT_TIMESTAMP),
(2, 'app_version', '1.4.2-build-net8', 'version de produccion del backend api rest en .net core 8', 'sistema', CURRENT_TIMESTAMP),
(3, 'mfa_forced_all', 'false', 'interruptor general para forzar doble factor de autenticacion obligatorio', 'seguridad', CURRENT_TIMESTAMP);
set identity_insert sistema.parametros off;

print 'poblando sistema.sesiones_usuarios...';
set identity_insert sistema.sesiones_usuarios on;
insert into sistema.sesiones_usuarios (sesionid, usuario, fechaingreso, fechasalida, direccionip, dispositivo, tokenacceso, estasesionactiva) values
(1, 'admin.sge', dateadd(minute, -60, CURRENT_TIMESTAMP), null, '192.168.1.100', 'windows 11 workstation (chrome 125)', 'jwt_sys_token_mock_001_xyz', 1),
(2, 'lgomez.comercial', dateadd(hour, -5, CURRENT_TIMESTAMP), dateadd(hour, -3, CURRENT_TIMESTAMP), '192.168.1.105', 'android client app v1.4', 'jwt_sys_token_mock_002_abc', 0);
set identity_insert sistema.sesiones_usuarios off;

print 'poblando sistema.logs_auditoria_datos...';
set identity_insert sistema.logs_auditoria_datos on;
insert into sistema.logs_auditoria_datos (logid, usuario, tablaafectada, accion, fecharegistro, idregistroafectado, valoranterior, valornuevo) values
(1, 'admin.sge', 'rrhh_nomina.conceptos_empleados_fijos', 'update', dateadd(day, -1, CURRENT_TIMESTAMP), '1', '{"montofijo":5200.00}', '{"montofijo":5500.00}'),
(2, 'mpaz.operaciones', 'rrhh_recursos.asistencias_diarias', 'insert', dateadd(day, -5, CURRENT_TIMESTAMP), '3', null, '{"empleadoid":3,"estadoasistencia":"asistio"}');
set identity_insert sistema.logs_auditoria_datos off;

print 'poblando sistema.reportes_config...';
set identity_insert sistema.reportes_config on;
insert into sistema.reportes_config (reporteid, codigo, nombre, descripcion, moduloorigen, procedimientonombre, estaactivo) values
-- ajustados para cumplir estrictamente con el constraint varchar(20)
(1, 'rep_planilla_mens', 'reporte consolidado de planilla mensual crm', 'vistas resumidas de ingresos, aportaciones de empleador y descuentos netos', 'rrhh', 'sp_reporte_planilla_mensual', 1),
(2, 'rep_kardex_val', 'kardex valorizado general de existencias', 'movimientos históricos de inventarios y valorización contable de almacenes', 'operaciones', 'sp_reporte_kardex_valorizado', 1);
set identity_insert sistema.reportes_config off;

print 'poblando sistema.historial_descargas_reportes...';
set identity_insert sistema.historial_descargas_reportes on;
insert into sistema.historial_descargas_reportes (descargareporteid, reporteid, usuario, fechageneracion, parametrosusados, formatoexportacion, registrosencontrados) values
(1, 1, 'admin.sge', dateadd(hour, -1, CURRENT_TIMESTAMP), '{"anio":2026,"mes":5,"tipoplanilla":"regular_mensual"}', 'excel', 3);
set identity_insert sistema.historial_descargas_reportes off;


print '*** inicializacion de los modulos rrhh, nomina y sistema completada con exito ***';

-- procedure 1: motor de movimientos de almacén (kardex + actualización de stock)
-- este procedimiento asegura que si ingresa o sale stock, impacte en el kardex y en el stock real al mismo tiempo
CREATE OR REPLACE PROCEDURE operaciones.sp_operaciones_registrar_movimiento_kardex
    @p_almacenid int,
    @p_productoid int,
    @p_tipomovimiento char(3), -- 'ent' (entrada) o 'sal' (salida)
    @p_conceptomovimiento varchar(100), -- compra, venta, traslado, ajuste
    @p_documentoreferencia varchar(50), -- oc-2024-001, ped-2024-002
    @p_cantidad decimal(18,4),
    @p_costounitariomovimiento decimal(18,4)
as
begin
    
    
    begin try
        begin transaction;

        -- 1. insertar el registro histórico e inmutable en el kardex
        insert into operaciones.kardexmovimientos (
            almacenid, productoid, tipomovimiento, conceptomovimiento, 
            documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento
        )
        values (
            @p_almacenid, @p_productoid, @p_tipomovimiento, @p_conceptomovimiento, 
            @p_documentoreferencia, @p_cantidad, @p_costounitariomovimiento, CURRENT_TIMESTAMP
        );

        -- 2. garantizar que el casillero de stock exista para ese producto en ese almacén
        if not exists (select 1 from operaciones.stockalmacen where almacenid = @p_almacenid and productoid = @p_productoid)
        begin
            insert into operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido)
            values (@p_almacenid, @p_productoid, 0.0000, 0.0000);
        end

        -- 3. operar el stock físico según la dirección del movimiento
        if @p_tipomovimiento = 'ent'
        begin
            update operaciones.stockalmacen
            set stockactual = stockactual + @p_cantidad
            where almacenid = @p_almacenid and productoid = @p_productoid;
        end
        else if @p_tipomovimiento = 'sal'
        begin
            -- validación de seguridad: evitar stock negativo antes de procesar
            declare @v_stock_disponible decimal(18,4);
            select @v_stock_disponible = stockactual from operaciones.stockalmacen where almacenid = @p_almacenid and productoid = @p_productoid;

            if @v_stock_disponible < @p_cantidad
            begin
                throw 50001, 'error: no hay stock suficiente en este almacen para procesar la salida.', 1;
            end

            update operaciones.stockalmacen
            set stockactual = stockactual - @p_cantidad
            where almacenid = @p_almacenid and productoid = @p_productoid;
        end

        commit transaction;
    end try
    begin catch
        if @@trancount > 0
            rollback transaction;

        -- propagar el mensaje de error al backend (node.js, .net, python, etc.)
        declare @v_errormsg varchar(4000) = error_message();
        raiserror(@v_errormsg, 16, 1);
    end catch
end;


-- procedure 2: vinculación automática de gastos de compras al costo real del proyecto
-- cuando se aprueba una orden de compra de un proyecto, este sp acumula automáticamente el costo al total del proyecto
CREATE OR REPLACE PROCEDURE operaciones.sp_operaciones_vincular_gasto_proyecto
    @p_ordenid int
as
begin
    
    declare @v_proyectoid int;
    declare @v_monto_total decimal(18,4);

    -- obtener los datos de la orden de compra
    select 
        @v_proyectoid = proyectoid,
        @v_monto_total = monto_total
    from operaciones.ordenescompra
    where ordenid = @p_ordenid and estado = 'aprobado';

    -- si la orden está amarrada a un proyecto, actualizamos el costo real del proyecto
    if @v_proyectoid is not null
    begin
        begin try
            begin transaction;

            update operaciones.proyectos
            set costoreallogrado = costoreallogrado + @v_monto_total
            where proyectoid = @v_proyectoid;

            commit transaction;
        end try
        begin catch
            if @@trancount > 0
                rollback transaction;
            
            declare @v_errormsg varchar(4000) = error_message();
            raiserror(@v_errormsg, 16, 1);
        end catch
    end
end;
