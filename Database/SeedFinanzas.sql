use sge_crm;
go

insert into finanzas.impuestos (codigoimpuestosunat, nombreimpuesto, porcentaje, estado) values
('1000', 'IGV', 18.00, 1),
('2000', 'ISC', 10.00, 1),
('9997', 'Exonerado', 0.00, 1),
('7152', 'Percepcion IGV', 2.00, 1),
('9100', 'Retencion IGV', 3.00, 1);

insert into finanzas.plancuentas (cuentacodigo, descripcion, tipocuenta, nivelint, aceptaasiento) values
('1011', 'Caja', 'activo', 4, 1),
('1041', 'Cuentas corrientes operativas', 'activo', 4, 1),
('1212', 'Facturas por cobrar', 'activo', 4, 1),
('3311', 'Inmuebles, maquinaria y equipo', 'activo', 4, 1),
('3913', 'Depreciacion acumulada', 'activo', 4, 1),
('4011', 'IGV por pagar', 'pasivo', 4, 1),
('4212', 'Facturas por pagar', 'pasivo', 4, 1),
('5011', 'Capital social', 'patrimonio', 4, 1),
('6011', 'Compras', 'gastos', 4, 1),
('7011', 'Ventas', 'ingresos', 4, 1);

insert into comercial.ubigeos (codigoubigeo, departamento, provincia, distrito) values
('150101', 'Lima', 'Lima', 'Lima');

insert into comercial.productos (
    codigosku, codigosunat, descripcion, unidadmedida, tipoafectacionigv,
    precioventasugerido, costopromedio, esservicio, sevende, nosevende, sefabrica, estado
) values
('EQ-CONT-001', '43211503', 'Laptop administrativa Lenovo', 'NIU', '10', 4200, 3600, 0, 0, 1, 0, 1),
('SRV-CONT-001', '43211501', 'Servidor contable local', 'NIU', '10', 18500, 16000, 0, 0, 1, 0, 1);

insert into comercial.clientes (
    tipodocumento, numerodocumento, razonsocial, nombrecomercial, direccionfiscal, ubigeo, email, telefono, tipocliente, estado
) values
('6', '20601234567', 'Cliente Demo SAC', 'Cliente Demo', 'Av. Demo 123', '150101', 'cliente@demo.pe', '999111222', 'cliente', 1);

insert into comercial.proveedores (
    tipodocumento, numerodocumento, razonsocial, direccionfiscal, ubigeo, telefono, email, estado
) values
('6', '20555111222', 'Proveedor Demo SAC', 'Jr. Proveedor 456', '150101', '999333444', 'proveedor@demo.pe', 1);

insert into operaciones.comprobantesfacturacion (
    serie, correlativo, tipocomprobante, clienteid, fechaemision, moneda, opgravada, igv_total, importetotalneto, estadosunat
) values
('B001', '000318', '03', 1, '2026-05-14', 'pen', 10720.34, 1929.66, 12650.00, 'aceptada_sunat'),
('B001', '000319', '03', 1, '2026-05-18', 'pen', 6271.19, 1128.81, 7400.00, 'aceptada_sunat');

insert into operaciones.ordenescompra (
    numeroorden, proveedorid, fechaemision, moneda, monto_total, estado
) values
('OC-2026-0245', 1, '2026-05-08', 'pen', 10030.00, 'pagada');

insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia) values
('AS-2026-00001', '2026-05-02', '01', 'Saldo inicial de caja', 'AP-0001'),
('AS-2026-00002', '2026-05-08', '08', 'Compra de mercaderia', 'F001-000245'),
('AS-2026-00003', '2026-05-14', '14', 'Provision de venta mensual', 'B001-000318');

insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber) values
(1, '1011', 25000, 0),
(1, '5011', 0, 25000),
(2, '6011', 8500, 0),
(2, '4011', 1530, 0),
(2, '4212', 0, 10030),
(3, '1212', 12650, 0),
(3, '7011', 0, 10720.34),
(3, '4011', 0, 1929.66);

insert into finanzas.cuentasbancarias (
    banconombre, numerocuenta, cuentacciexterno, tipocuenta, moneda, saldoactual, estado
) values
('BCP', '191-2445783-0-11', '00219100244578301122', 'corriente', 'pen', 46250, 1),
('BBVA', '0011-0254-0200017835', '01125400020001783577', 'ahorros', 'pen', 18840, 1),
('Caja chica', 'CAJA-ADM-001', null, 'caja chica', 'pen', 2400, 1);

insert into finanzas.movimientostesoreria (
    cuentabancariaid, tipoflujo, mediopagosunat, monto, comprobanteid, ordenid, glosamovimiento, fechamovimiento
) values
(1, 'ing', '003', 12650, 1, null, 'Cobro de factura B001-000318', '2026-05-15T10:30:00'),
(1, 'egr', '003', 10030, null, 1, 'Pago a proveedor F001-000245', '2026-05-16T12:15:00'),
(2, 'ing', '001', 7400, 2, null, 'Deposito de cliente', '2026-05-18T09:10:00'),
(3, 'egr', '009', 380, null, null, 'Gasto administrativo menor', '2026-05-20T14:05:00');

insert into finanzas.activosfijos (
    codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada, estado
) values
('AF-2026-0001', 'Laptop administrativa Lenovo', 1, '2026-01-12', 4200, 25, 437.50, 'activo'),
('AF-2026-0002', 'Servidor contable local', 2, '2025-11-03', 18500, 20, 2160, 'activo'),
('AF-2026-0003', 'Impresora multifuncional', null, '2025-07-22', 2800, 10, 238, 'activo'),
('AF-2026-0004', 'Mobiliario oficina finanzas', null, '2024-03-10', 9800, 10, 2100, 'activo'),
('AF-2026-0005', 'Equipo retirado por obsolescencia', null, '2022-05-19', 3600, 25, 3600, 'retirado');
go
