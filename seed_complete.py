import psycopg2
import sys
from datetime import datetime, date

conn_str = "postgres://postgres.vnqcsiflshietpnohrfk:uXyFMdmZ3xy197Dz@aws-1-us-west-2.pooler.supabase.com:6543/postgres"

def run_seed():
    try:
        conn = psycopg2.connect(conn_str)
        cur = conn.cursor()
        print("Connected to Supabase PostgreSQL.")

        # 1. Truncate all tables from all schemas
        tables_to_truncate = [
            # sistema
            "sistema.historial_descargas_reportes",
            "sistema.reportes_config",
            "sistema.logs_auditoria_datos",
            "sistema.sesiones_usuarios",
            "sistema.parametros",
            # rrhh_nomina
            "rrhh_nomina.essalud_declaraciones",
            "rrhh_nomina.gratificaciones",
            "rrhh_nomina.beneficios",
            "rrhh_nomina.bancos_config",
            "rrhh_nomina.rangos_renta",
            "rrhh_nomina.parametros_generales",
            "rrhh_nomina.planillas_resumen",
            "rrhh_nomina.historial_pagos",
            "rrhh_nomina.declaraciones_pdt",
            "rrhh_nomina.reportes",
            "rrhh_nomina.utilidades",
            "rrhh_nomina.rentas_quinta_acumuladas",
            "rrhh_nomina.planillas_conceptos_detalles",
            "rrhh_nomina.planillas_detalles",
            "rrhh_nomina.planillas_cabeceras",
            "rrhh_nomina.periodos_planillas",
            "rrhh_nomina.conceptos_empleados_fijos",
            "rrhh_nomina.conceptos",
            "rrhh_nomina.tasas_afp",
            # rrhh_recursos
            "rrhh_recursos.programacion_vacaciones",
            "rrhh_recursos.periodos_vacacionales",
            "rrhh_recursos.solicitudes_licencias",
            "rrhh_recursos.tipos_licencias",
            "rrhh_recursos.asistencias_diarias",
            "rrhh_recursos.marcaciones_biometricos",
            "rrhh_recursos.turnos",
            "rrhh_recursos.derechohabientes",
            "rrhh_recursos.datos_laborales_empleados",
            "rrhh_recursos.administradoras_pensiones",
            "rrhh_recursos.regimenes_laborales",
            "rrhh_recursos.ubigeos",
            "rrhh_recursos.contratos",
            "rrhh_recursos.empleados",
            "rrhh_recursos.usuarios_nomina",
            "rrhh_recursos.feriados",
            "rrhh_recursos.centros_costos",
            # seguridad
            "seguridad.usuario_mfa",
            "seguridad.usuario_historial_passwords",
            "seguridad.usuario_intentos_login",
            "seguridad.usuario_sesiones",
            "seguridad.usuario_tokens",
            # sunat
            "sunat.cierres_ple",
            "sunat.control_car_sire",
            "sunat.declaraciones_sire",
            "sunat.catalogo05_afectacionigv",
            "sunat.catalogo02_comprobantes",
            "sunat.catalogo01_identidad",
            # finanzas
            "finanzas.activosfijos",
            "finanzas.movimientostesoreria",
            "finanzas.cuentasbancarias",
            "finanzas.asientosdetalle",
            "finanzas.asientoscabecera",
            "finanzas.plancuentas",
            "finanzas.impuestos",
            # operaciones
            "operaciones.kardexmovimientos",
            "operaciones.stockalmacen",
            "operaciones.guiasremision",
            "operaciones.comprobantesfacturacion",
            "operaciones.ordenescompradetalle",
            "operaciones.ordenescompra",
            "operaciones.pedidosventadetalle",
            "operaciones.pedidosventa",
            "operaciones.proyectotareas",
            "operaciones.proyectos",
            "operaciones.almacenes",
            # comercial
            "comercial.vehiculosproveedores",
            "comercial.conductoresproveedores",
            "comercial.contactosproveedores",
            "comercial.contactosclientes",
            "comercial.productos",
            "comercial.clientes",
            "comercial.proveedores",
            "comercial.ubigeos"
        ]

        print("Truncating tables...")
        for table in tables_to_truncate:
            try:
                cur.execute(f"TRUNCATE TABLE {table} CASCADE;")
            except Exception as te:
                print(f"Skipping truncate on {table} due to: {te}")
                conn.rollback()
        conn.commit()
        print("Truncation successful.")

        # ==========================================
        # SCHEMA comercial
        # ==========================================
        print("Seeding Schema: comercial...")
        
        # comercial.ubigeos
        ubigeos = [
            ('150101', 'Lima', 'Lima', 'Lima'),
            ('150122', 'Lima', 'Lima', 'Miraflores'),
            ('150131', 'Lima', 'Lima', 'San Isidro'),
            ('150140', 'Lima', 'Lima', 'Santiago de Surco'),
            ('070101', 'Callao', 'Callao', 'Callao'),
            ('040101', 'Arequipa', 'Arequipa', 'Arequipa'),
            ('130101', 'La Libertad', 'Trujillo', 'Trujillo'),
            ('140101', 'Lambayeque', 'Chiclayo', 'Chiclayo'),
            ('150132', 'Lima', 'Lima', 'San Juan de Lurigancho'),
            ('150103', 'Lima', 'Lima', 'Ate')
        ]
        cur.executemany("INSERT INTO comercial.ubigeos (codigoubigeo, departamento, provincia, distrito) VALUES (%s, %s, %s, %s);", ubigeos)

        # comercial.clientes
        clientes = [
            (i, '6', f'2010045678{i}', f'CLIENTE OPERACIONAL {i} S.A.C.', f'Comercial {i}', 'Av. Principal 123', '150131', f'contacto{i}@empresa.com.pe', f'99988877{i}', 'cliente', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO comercial.clientes (clienteid, tipodocumento, numerodocumento, razonsocial, nombrecomercial, direccionfiscal, ubigeo, email, telefono, tipocliente, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, clientes)

        # comercial.proveedores
        proveedores = [
            (i, '6', f'2033445566{i}', f'PROVEEDOR LOGÍSTICO {i} S.A.', f'Av. Industrial 45{i}', '070101', f'01451203{i}', f'ventas{i}@proveedor.com.pe', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO comercial.proveedores (proveedorid, tipodocumento, numerodocumento, razonsocial, direccionfiscal, ubigeo, telefono, email, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, proveedores)

        # comercial.contactosclientes
        contactos_clientes = [
            (i, f'Contacto Cliente {i}', f'Cargo {i}', f'98765432{i}', f'contacto{i}@cliente.com.pe', True)
            for i in range(1, 11)
        ]
        cur.executemany("INSERT INTO comercial.contactosclientes (clienteid, nombre, cargo, telefono, email, estado) VALUES (%s, %s, %s, %s, %s, %s);", contactos_clientes)

        # comercial.contactosproveedores
        contactos_proveedores = [
            (i, f'Contacto Prov {i}', f'Ventas {i}', f'94455667{i}', f'contacto{i}@prov.com.pe', True)
            for i in range(1, 11)
        ]
        cur.executemany("INSERT INTO comercial.contactosproveedores (proveedorid, nombre, cargo, telefono, email, estado) VALUES (%s, %s, %s, %s, %s, %s);", contactos_proveedores)

        # comercial.vehiculosproveedores
        vehiculos = [
            (i, (i % 10) + 1, f'ABC-12{i}', 'Volvo', 'FMX 460', 'Tractocamión', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO comercial.vehiculosproveedores (vehiculoid, proveedorid, placa, marca, modelo, tipovehiculo, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, vehiculos)

        # comercial.conductoresproveedores
        conductores = [
            (i, (i % 10) + 1, f'Conductor {i}', '1', f'0812345{i}', f'Q0812345{i}-A3C', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO comercial.conductoresproveedores (conductorid, proveedorid, nombre, tipodocumento, numerodocumento, licenciaconducir, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, conductores)

        # comercial.productos
        productos = [
            (1, 'PROD-001', '30102401', 'Fierro Corrugado de 1/2 Pulgada', 'NIO', '10', 42.50, 31.20, False, True, False, False, True),
            (2, 'PROD-002', '30111601', 'Cemento Sol Tipo I (Bolsa 42.5 kg)', 'NIO', '10', 28.00, 21.50, False, True, False, False, True),
            (3, 'PROD-003', '43211501', 'Laptop Corporativa i7 16GB RAM', 'NAR', '10', 3500.00, 2800.00, False, True, False, False, True),
            (4, 'SERV-001', '81101508', 'Servicio de Consultoría en Gestión de Proyectos', 'ZZ', '10', 150.00, 0.00, True, True, False, False, True),
            (5, 'SERV-002', '81141601', 'Servicio de Transporte Logístico Local LIMA', 'ZZ', '10', 450.00, 320.00, True, True, False, False, True),
            (6, 'PROD-004', '30102402', 'Fierro Corrugado de 3/8 Pulgada', 'NIO', '10', 32.00, 24.50, False, True, False, False, True),
            (7, 'PROD-005', '30111602', 'Cemento Sol Tipo V (Bolsa 42.5 kg)', 'NIO', '10', 34.00, 26.00, False, True, False, False, True),
            (8, 'PROD-006', '31191501', 'Pintura Látex Pato Galón', 'GLI', '10', 48.00, 35.00, False, True, False, False, True),
            (9, 'PROD-007', '39121001', 'Cable Eléctrico 14 AWG Cobre 100m', 'NAR', '10', 120.00, 90.00, False, True, False, False, True),
            (10, 'SERV-003', '81101509', 'Servicio de Supervisión de Obra de Construcción', 'ZZ', '10', 250.00, 0.00, True, True, False, False, True)
        ]
        cur.executemany("""
            INSERT INTO comercial.productos (productoid, codigosku, codigosunat, descripcion, unidadmedida, tipoafectacionigv, precioventasugerido, costopromedio, esservicio, sevende, nosevende, sefabrica, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, productos)


        # ==========================================
        # SCHEMA operaciones
        # ==========================================
        print("Seeding Schema: operaciones...")

        # operaciones.almacenes
        almacenes = [
            (i, f'ALM-0{i}', f'Almacén Operativo {i}', f'Av. Argentina {2000 + i}', '150101', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.almacenes (almacenid, codigoalmacen, nombre, direccion, ubigeo, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, almacenes)

        # operaciones.stockalmacen
        stocks = [
            ((i % 10) + 1, (i % 10) + 1, 1000.0 + i * 50, 100.0 + i)
            for i in range(1, 21)
        ]
        cur.executemany("INSERT INTO operaciones.stockalmacen (almacenid, productoid, stockactual, stockcomprometido) VALUES (%s, %s, %s, %s) ON CONFLICT (almacenid, productoid) DO NOTHING;", stocks)

        # operaciones.proyectos
        proyectos = [
            (i, i, f'PROYECTO EDIFICIO INFRAESTRUCTURA {i}', f'Detalle del proyecto {i}', 100000.0 * i, 10000.0 * i, '2026-01-15', '2026-12-20', 'en progreso')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.proyectos (proyectoid, clienteid, nombreproyecto, descripcion, presupuestototal, costoreallogrado, fechainicio, fechafin, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, proyectos)

        # operaciones.proyectotareas
        tareas = [
            (i, (i % 10) + 1, f'Tarea {i} de Ingeniería', '2026-01-15', '2026-06-30', 50.0, 5000.0 * i, 'en ejecucion')
            for i in range(1, 16)
        ]
        cur.executemany("""
            INSERT INTO operaciones.proyectotareas (tareaid, proyectoid, nombretarea, fechainicio, fechafin, porcentajeprogreso, costoestimado, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s);
        """, tareas)

        # operaciones.pedidosventa
        pedidos = [
            (i, f'PED-2026-00{i}', i, i, '2026-02-10', 'pen', 1.0, 'credito', None, 5000.0 * i, 0.00, 5000.0 * i, 'aprobado')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.pedidosventa (pedidoid, numeropedido, clienteid, proyectoid, fechaemision, moneda, tipocambio, metodopago, cupondescuento, montobruto, montodescuento, totalneto, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, pedidos)

        # operaciones.pedidosventadetalle (detalledid, pedidoid, productoid, cantidad, preciounitariocongiv, descuento, totalfila)
        pedido_detalles = [
            (i, (i % 10) + 1, (i % 10) + 1, 10.0 * i, 100.0, 0.0, 1000.0 * i)
            for i in range(1, 16)
        ]
        cur.executemany("""
            INSERT INTO operaciones.pedidosventadetalle (detalledid, pedidoid, productoid, cantidad, preciounitariocongiv, descuento, totalfila)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, pedido_detalles)

        # operaciones.ordenescompra
        ordenes = [
            (i, f'OC-2026-00{i}', i, i, 'Ing. Residente', '2026-02-15', 'pen', 3000.0 * i, 'materiales', 'aprobado')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.ordenescompra (ordenid, numeroorden, proveedorid, proyectoid, solicitante, fechaemision, moneda, monto_total, categoriagasto, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, ordenes)

        # operaciones.ordenescompradetalle (ordenid, productoid, cantidad, costounitariocongiv, totalfila)
        orden_detalles = []
        for o in range(1, 11):
            orden_detalles.append((o, o, 10.0 * o, 50.0, 500.0 * o))
        for o in range(1, 6):
            p = (o % 10) + 1
            if p == o:
                p = (p % 10) + 1
            orden_detalles.append((o, p, 5.0 * o, 30.0, 150.0 * o))

        cur.executemany("""
            INSERT INTO operaciones.ordenescompradetalle (ordenid, productoid, cantidad, costounitariocongiv, totalfila)
            VALUES (%s, %s, %s, %s, %s);
        """, orden_detalles)

        # operaciones.comprobantesfacturacion
        comprobantes = [
            (i, i, '01', 'F001', f'000000{i:02d}', '2026-02-12', '01', i, 'pen', 4000.0 * i, 0.00, 0.00, 720.0 * i, 4720.0 * i, 'ninguno', 'enviado sunat')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.comprobantesfacturacion (comprobanteid, pedidoid, tipocomprobante, serie, correlativo, fechaemision, tipooperacionsunat, clienteid, moneda, opgravada, opinafecta, opexonerada, igv_total, importetotalneto, tipoimpuestoespecial, estadosunat)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, comprobantes)

        # operaciones.guiasremision
        guias = [
            (i, 'T001', f'000000{i:02d}', '2026-02-20', '04', 1, 2, None, i, i, 1000.0 * i, 'kgm', 'aceptado')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.guiasremision (guiaid, serie, correlativo, fechaemision, motivotraslado, almacenorigenid, almacendestinoid, proveedorid, vehiculoid, conductorid, pesototal, unidadmedidapeso, estadosunat)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, guias)

        # operaciones.kardexmovimientos
        kardex = [
            (i, (i % 10) + 1, (i % 10) + 1, 'ent', 'compra', f'OC-2026-00{i}', 100.0 * i, 15.0, '2026-02-15')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO operaciones.kardexmovimientos (movimientoid, almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, kardex)


        # ==========================================
        # SCHEMA finanzas
        # ==========================================
        print("Seeding Schema: finanzas...")

        # finanzas.impuestos
        impuestos = [
            (i, f'{1000 + i}', f'Impuesto Prueba {i}', 18.0 - i, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO finanzas.impuestos (impuestoid, codigoimpuestosunat, nombreimpuesto, porcentaje, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, impuestos)

        # finanzas.plancuentas
        plancuentas = [
            (f'10411{i}', f'Cuenta Banco BCP Soles {i}', 'activo', 5, True)
            for i in range(0, 10)
        ]
        cur.executemany("INSERT INTO finanzas.plancuentas (cuentacodigo, descripcion, tipocuenta, nivelint, aceptaasiento) VALUES (%s, %s, %s, %s, %s);", plancuentas)

        # finanzas.asientoscabecera
        asientos_cab = [
            (i, f'AS-2026-0000{i}', '2026-06-01', '01', f'Glosa del asiento contable {i}', f'F001-0000{i}')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO finanzas.asientoscabecera (asientoid, numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, asientos_cab)

        # finanzas.asientosdetalle
        asientos_det = []
        for i in range(1, 11):
            # Debe
            asientos_det.append((i * 2 - 1, i, '104110', 1000.0 * i, 0.0000))
            # Haber
            asientos_det.append((i * 2, i, '104111', 0.0000, 1000.0 * i))
        cur.executemany("""
            INSERT INTO finanzas.asientosdetalle (asientodetalleid, asientoid, cuentacodigo, debe, haber)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, asientos_det)

        # finanzas.cuentasbancarias
        cuentas_bancarias = [
            (i, 'BCP SOLES', f'191-3456789-0-1{i}', f'CCI-191-3456789-0-1{i}', 'corriente', 'pen', 10000.0 * i, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO finanzas.cuentasbancarias (cuentabancariaid, banconombre, numerocuenta, cuentacciexterno, tipocuenta, moneda, saldoactual, estado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s);
        """, cuentas_bancarias)

        # finanzas.movimientostesoreria
        movimientos_tesoreria = [
            (i, i, 'ing', '003', 1000.0 * i, i, None, f'Cobro de factura {i}')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO finanzas.movimientostesoreria (movimientotesoreriaid, cuentabancariaid, tipoflujo, mediopagosunat, monto, comprobanteid, ordenid, glosamovimiento)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s);
        """, movimientos_tesoreria)

        # finanzas.activosfijos
        activos_fijos = [
            (i, f'ACT-00{i}', f'Computadora Escritorio Modelo {i}', i, '2026-01-01', 3000.0, 20.00, 600.0)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO finanzas.activosfijos (activoid, codigoactivo, descripcion, productoid, fechadquisicion, valorinicial, tasadepreciacionanual, depreciacionacumulada)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s);
        """, activos_fijos)


        # ==========================================
        # SCHEMA sunat
        # ==========================================
        print("Seeding Schema: sunat...")

        # sunat.catalogo01_identidad
        catalogo_01 = [
            ('0', 'DOC.TRIB.NO.DOM.SIN.RUC', True),
            ('1', 'DNI', True),
            ('4', 'CARNET DE EXTRANJERIA', True),
            ('6', 'RUC', True),
            ('7', 'PASAPORTE', True),
            ('A', 'DUMMY DOC A', True),
            ('B', 'DUMMY DOC B', True),
            ('C', 'DUMMY DOC C', True),
            ('D', 'DUMMY DOC D', True),
            ('E', 'DUMMY DOC E', True)
        ]
        cur.executemany("INSERT INTO sunat.catalogo01_identidad (codigochar, descripcion, estado) VALUES (%s, %s, %s) ON CONFLICT (codigochar) DO NOTHING;", catalogo_01)

        # sunat.catalogo02_comprobantes
        catalogo_02 = [
            ('01', 'FACTURA', 'FAC', True),
            ('03', 'BOLETA DE VENTA', 'BOL', True),
            ('07', 'NOTA DE CREDITO', 'NCR', True),
            ('08', 'NOTA DE DEBITO', 'NDB', True),
            ('09', 'GUIA DE REMISION - REMITENTE', 'GRE', True),
            ('12', 'TICKET MAQUINA REGISTRADORA', 'TCK', True),
            ('13', 'DOCUMENTO EMITIDO POR BANCOS', 'BAN', True),
            ('14', 'RECIBO POR SERVICIOS PUBLICOS', 'PUB', True),
            ('18', 'DOCUMENTO AFP', 'AFP', True),
            ('31', 'GUIA DE REMISION - TRANSPORTISTA', 'GRT', True)
        ]
        cur.executemany("INSERT INTO sunat.catalogo02_comprobantes (codigocharsunat, descripcion, abreviatura, estado) VALUES (%s, %s, %s, %s) ON CONFLICT (codigocharsunat) DO NOTHING;", catalogo_02)

        # sunat.catalogo05_afectacionigv
        catalogo_05 = [
            ('10', 'Gravado - Operación Onerosa', 's', '1000'),
            ('11', 'Gravado - Retiro por premio', 's', '1000'),
            ('12', 'Gravado - Retiro por donación', 's', '1000'),
            ('13', 'Gravado - Retiro por muestra médica', 's', '1000'),
            ('14', 'Gravado - Retiro por publicidad', 's', '1000'),
            ('15', 'Gravado - Retiro por entrega a trabajadores', 's', '1000'),
            ('16', 'Gravado - Retiro por servicio gratuito', 's', '1000'),
            ('20', 'Exonerado - Operación Onerosa', 'e', '9997'),
            ('30', 'Inafecto - Operación Onerosa', 'o', '9998'),
            ('31', 'Inafecto - Retiro por transferencia', 'o', '9998')
        ]
        cur.executemany("INSERT INTO sunat.catalogo05_afectacionigv (codigoafectacion, descripcion, letra_tributo, codigo_tributo_sunat) VALUES (%s, %s, %s, %s) ON CONFLICT (codigoafectacion) DO NOTHING;", catalogo_05)

        # sunat.declaraciones_sire
        declaraciones_sire = [
            (i, f'2026{i:02d}', 'rvie', f'TICKET-SIRE-2026-00{i}', 'propuesta_sunat', f'archivo_sire_2026{i:02d}.zip')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sunat.declaraciones_sire (sireid, periodo, tiporegistro, numeroticket, estado_sire, nombre_archivo_exportado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, declaraciones_sire)

        # sunat.control_car_sire
        car_sire = [
            (i, i, None, f'CAR-2026-0000{i}', '202606')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sunat.control_car_sire (carid, comprobanteid, ordenid, codigo_car, periodo_afectacion)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, car_sire)

        # sunat.cierres_ple
        cierres_ple = [
            (f'2026{i:02d}00', '050100', 100 + i, f'hash_ple_{i}_d41d8cd98f00b204e9800998ecf8427eef6312a4b898e', f'2026-{i:02d}-10 18:00:00', '1')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sunat.cierres_ple (periodo, codigolibrosunat, cantidad_filas, codigohash, fecha_generacion, estado_envio)
            VALUES (%s, %s, %s, %s, %s, %s);
        """, cierres_ple)


        # ==========================================
        # SCHEMA seguridad
        # ==========================================
        print("Seeding Schema: seguridad...")

        # seguridad.usuario_tokens
        tokens = [
            (i, i, f'token_refresco_{i}', f'jwt_id_{i}', False, False, '2026-12-31 23:59:59')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO seguridad.usuario_tokens (tokenid, usuarioid, token_refresco, jwt_id, es_usado, es_revocado, fecha_expiracion)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, tokens)

        # seguridad.usuario_sesiones
        sesiones = [
            (i, i, None, '127.0.0.1', 'Chrome/Windows', 'Desktop PC', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO seguridad.usuario_sesiones (sesionid, usuarioid, tokenid, ip_direccion, navegador, dispositivo, es_activa)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, sesiones)

        # seguridad.usuario_intentos_login
        intentos = [
            (i, f'nomina{i}@empresa.com', '127.0.0.1', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO seguridad.usuario_intentos_login (intentoid, email_ingresado, ip_direccion, exito)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s);
        """, intentos)

        # seguridad.usuario_historial_passwords
        password_hist = [
            (i, i, f'passhash_{i}')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO seguridad.usuario_historial_passwords (historialid, usuarioid, contrasena_hash)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s);
        """, password_hist)

        # seguridad.usuario_mfa
        mfa = [
            (i, i, 'totp', f'secretkey_{i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO seguridad.usuario_mfa (mfaid, usuarioid, proveedor, secreto_mfa, es_activo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, mfa)


        # ==========================================
        # SCHEMA rrhh_recursos
        # ==========================================
        print("Seeding Schema: rrhh_recursos...")

        # rrhh_recursos.centros_costos
        centros_costo = [
            (i, f'CC-0{i}', f'Centro de Costo {i}', f'Área de Operaciones {i}', 'Jhoel Patrick', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.centros_costos (centrocostoid, codigo, nombre, descripcion, responsable, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, centros_costo)

        # rrhh_recursos.feriados
        feriados = [
            (i, f'2026-08-{i:02d}', f'Feriado Nacional {i}', 'Nacional', False, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.feriados (feriadoid, fecha, descripcion, tipo, recuperable, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, feriados)

        # rrhh_recursos.usuarios_nomina
        usuarios_nomina = [
            (1, 'admin', 'Admin User', 'Administrador', 'admin@sge-enterprise.com', True)
        ] + [
            (i, f'usuario_{i}', f'Nomina User {i}', 'Analista', f'nomina{i}@sge-enterprise.com', True)
            for i in range(2, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.usuarios_nomina (usuarionominaid, usuario, nombrecompleto, rol, correo, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, usuarios_nomina)

        # rrhh_recursos.empleados
        empleados = [
            (1, '1', '10203041', 'Admin', 'SGE', 'Enterprise', '1990-05-15', 'm', 'admin@gmail.com', 'admin@sge-enterprise.com', '999111221', 1, 'Administrador', 'Sistemas', False, '2026-01-01', True)
        ] + [
            (i, '1', f'1020304{i}', f'Nombre {i}', f'Paterno {i}', f'Materno {i}', '1990-05-15', 'm', f'personal{i}@gmail.com', f'corporativo{i}@sge-enterprise.com', f'99911122{i}', 1, 'Colaborador', 'Operaciones', False, '2026-01-01', True)
            for i in range(2, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.empleados (empleadoid, tipodocumento, numerodocumento, nombres, apellidopaterno, apellidomaterno, fechanacimiento, sexo, correopersonal, correocorporativo, telefonocelular, centrocostoid, cargo, departamento, tienehijos, fechaingreso, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, empleados)

        # rrhh_recursos.contratos
        contratos = [
            (i, i, 'Plazo Indeterminado', '2026-01-01', None, 2000.00 * i, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.contratos (contratoid, empleadoid, tipocontrato, fechainicio, fechafin, sueldobase, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, contratos)

        # rrhh_recursos.ubigeos
        ubigeos_rrhh = [
            (f'15010{i}', 'Lima', 'Lima', f'Distrito {i}')
            for i in range(0, 10)
        ]
        cur.executemany("INSERT INTO rrhh_recursos.ubigeos (ubigeoid, departamento, provincia, distrito) VALUES (%s, %s, %s, %s) ON CONFLICT (ubigeoid) DO NOTHING;", ubigeos_rrhh)

        # rrhh_recursos.regimenes_laborales
        regimenes = [
            (i, f'0{i}', f'Régimen Laboral Sunat {i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.regimenes_laborales (regimenlaboralid, codigosunat, nombre, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s);
        """, regimenes)

        # rrhh_recursos.administradoras_pensiones
        afps = [
            (i, f'0{i}', f'AFP Prueba {i}', 'afp', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.administradoras_pensiones (afpid, codigosunat, nombre, tipo, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, afps)

        # rrhh_recursos.datos_laborales_empleados
        datos_lab = [
            (i, 1, 1, 'flujo', f'CUSPP-1020304{i}', '150101', 'Av. Las Gardenias 123', f'CTA-SUELDO-12{i}', 1, f'CTA-CTS-12{i}', 1)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.datos_laborales_empleados (empleadoid, regimenlaboralid, afpid, tipocomision, cuspp, ubigeodomicilio, direccion, cuentasueldo, bancosueldoid, cuentacts, bancoctsid)
            VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, datos_lab)

        # rrhh_recursos.derechohabientes
        derechohabientes = [
            (i, i, 'Hijo(a)', '1', f'8765432{i}', f'Hijo {i}', 'Paterno', 'Materno', '2015-05-15')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.derechohabientes (derechohabienteid, empleadoid, vinculofamiliar, tipodocumento, numerodocumento, nombres, apellidopaterno, apellidomaterno, fechanacimiento)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, derechohabientes)

        # rrhh_recursos.turnos
        turnos = [
            (i, f'Turno General {i}', '08:00:00', '17:00:00', 10, 60, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.turnos (turnoid, nombre, horaingreso, horasalida, toleranciaingreso, tiemporefrigerio, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, turnos)

        # rrhh_recursos.marcaciones_biometricos
        marcaciones = [
            (i, i, '2026-06-01 08:00:00', 'ingreso', 'Dispositivo Principal')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.marcaciones_biometricos (marcacionid, empleadoid, fechahora, tipo, dispositivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, marcaciones)

        # rrhh_recursos.asistencias_diarias
        asistencias = [
            (i, i, '2026-06-01', 1, '08:00:00', '17:00:00', 0, 0, 0, 0, 'asistio')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.asistencias_diarias (asistenciadiariaid, empleadoid, fecha, turnoid, horaingresoreal, horasalidareal, minutostardanza, minutosextras25, minutosextras35, minutosnocturnas, estadoasistencia)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, asistencias)

        # rrhh_recursos.tipos_licencias
        licencias_tipos = [
            (i, f'0{i}', f'Licencia Sunat {i}', True, False, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.tipos_licencias (tipolicenciaid, codigosunat, descripcion, congocehaber, essubsidiado, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, licencias_tipos)

        # rrhh_recursos.solicitudes_licencias
        licencias_sol = [
            (i, i, 1, '2026-06-01', '2026-06-05', 'aprobada', 1, 'Sustento médico')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.solicitudes_licencias (solicitudlicenciaid, empleadoid, tipolicenciaid, fechainicio, fechafin, estadosolicitud, usuariosolicitaid, sustento)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s);
        """, licencias_sol)

        # rrhh_recursos.periodos_vacacionales
        periodos_vac = [
            (i, i, 2025 + (i % 2), 30, 0, 0, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.periodos_vacacionales (periodovacacionalid, empleadoid, anioperiodo, diasganados, diasgozados, diasvendidos, estaabierto)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, periodos_vac)

        # rrhh_recursos.programacion_vacaciones
        prog_vac = [
            (i, i, '2026-07-01', '2026-07-15', 'aprobada')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_recursos.programacion_vacaciones (programacionvacacionid, periodovacacionalid, fechainicio, fechafin, estadosolicitud)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, prog_vac)


        # ==========================================
        # SCHEMA rrhh_nomina
        # ==========================================
        print("Seeding Schema: rrhh_nomina...")

        # rrhh_nomina.tasas_afp
        tasas_afp = [
            (i, 1, 2026, i, 10.00, 1.35, 1.50, 1.40, 45000.00)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.tasas_afp (tasasafpid, afpid, anio, mes, porcentajeaporte, porcentajeseguro, porcentajecomisionflujo, porcentajecomisionmixta, topeprimaseguro)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, tasas_afp)

        # rrhh_nomina.conceptos (Seeded previously, let's keep it or insert the standard ones with DO NOTHING)
        conceptos = [
            (1, '0121', 'Sueldo Básico', 'SUELDO_BAS', 'ingreso_remunerativo', True, True, True, True, False, True, 0.00, 'Fijo'),
            (2, '0201', 'Asignación Familiar', 'ASIG_FAM', 'ingreso_remunerativo', True, True, True, True, False, True, 0.00, 'Fijo'),
            (3, '0105', 'Horas Extras 25%', 'HE_25', 'ingreso_remunerativo', False, True, True, True, False, True, 0.00, 'Variable'),
            (4, '0106', 'Horas Extras 35%', 'HE_35', 'ingreso_remunerativo', False, True, True, True, False, True, 0.00, 'Variable'),
            (5, '0402', 'Gratificación Legal', 'GRAT_LEG', 'ingreso_no_remunerativo', False, True, True, False, False, True, 0.00, 'Fijo'),
            (6, '0902', 'Bonificación Extraordinaria', 'BONI_EXT', 'ingreso_no_remunerativo', False, True, True, False, False, True, 0.00, 'Fijo'),
            (7, '0804', 'Essalud', 'ESSALUD_EMP', 'aporte_empleador', True, True, True, False, False, False, 9.00, 'Fijo'),
            (8, '0601', 'ONP', 'ONP', 'descuento', True, True, True, False, True, True, 13.00, 'Obligatorio'),
            (9, '0602', 'AFP Integra', 'AFP_INT_F', 'descuento', True, True, True, False, True, True, 12.80, 'Obligatorio'),
            (10, '0603', 'AFP Hábitat', 'AFP_HAB_F', 'descuento', True, True, True, False, True, True, 12.90, 'Obligatorio'),
            (11, '0604', 'AFP Prima', 'AFP_PRI_F', 'descuento', True, True, True, False, True, True, 12.85, 'Obligatorio'),
            (12, '0605', 'AFP Profuturo', 'AFP_PRO_F', 'descuento', True, True, True, False, True, True, 12.95, 'Obligatorio'),
            (13, '0701', 'Adelanto de Sueldo', 'ADEL_SUEL', 'descuento', False, True, True, False, False, True, 0.00, 'Voluntario'),
            (14, '0702', 'Tardanzas', 'TARD_FALT', 'descuento', False, True, True, False, False, True, 0.00, 'Voluntario')
        ]
        # Pad up to 17 concepts to meet seed/counts
        for i in range(15, 18):
            conceptos.append((i, f'0{i:03d}', f'Concepto Extra {i}', f'EXT_{i}', 'ingreso_remunerativo', False, True, True, True, False, True, 0.00, 'Variable'))

        cur.executemany("""
            INSERT INTO rrhh_nomina.conceptos (conceptoid, codigosunat, nombre, abreviatura, tipoconcepto, esfijo, estaactivo, afectacalculo, esremunerativo, obligatorio, afectaneto, porcentaje, tipo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
            ON CONFLICT (codigosunat) DO NOTHING;
        """, conceptos)

        # rrhh_nomina.conceptos_empleados_fijos
        conceptos_fijos = [
            (i, i, 1, 1000.00 * i, 'Sueldo pactado', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.conceptos_empleados_fijos (conceptoempleadofid, empleadoid, conceptoid, montofijo, explicacion, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, conceptos_fijos)

        # rrhh_nomina.periodos_planillas
        periodos_planillas = [
            (i, 2026, (i % 12) + 1, 'regular_mensual', '2026-06-01', '2026-06-30', 'abierto')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.periodos_planillas (periodoplanillaid, anio, mes, tipoplanilla, fechainicio, fechafin, estadoperiodo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, periodos_planillas)

        # rrhh_nomina.planillas_cabeceras
        planillas_cabeceras = [
            (i, i, '2026-06-03 12:00:00', f'Planilla Mensual {i}', 'borrador', 1)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.planillas_cabeceras (planillacabeceraid, periodoplanillaid, fechacalculo, descripcion, estadoplanilla, usuarioid)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, planillas_cabeceras)

        # rrhh_nomina.planillas_detalles
        planillas_detalles = [
            (i, i, i, 30, 0, 0, 3000.00, 0.00, 300.00, 270.00, 2700.00, f'hashboleta_{i}')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.planillas_detalles (planilladetalleid, planillacabeceraid, empleadoid, diaslaborados, diassubsidiados, diasnolaborados, totalingresosremunerativos, totalingresosnoremunerativos, totaldescuentos, totalaportesempleador, netopagar, codigohashboleta)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, planillas_detalles)

        # rrhh_nomina.planillas_conceptos_detalles
        planillas_con_detalles = [
            (i, (i % 10) + 1, (i % 10) + 1, 3000.00)
            for i in range(1, 16)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.planillas_conceptos_detalles (planillaconceptodetalleid, planilladetalleid, conceptoid, montocalculado)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s);
        """, planillas_con_detalles)

        # rrhh_nomina.rentas_quinta_acumuladas
        rentas_quinta = [
            (i, i, 2026, 12000.00 * i, 120.00 * i, 0.00)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.rentas_quinta_acumuladas (rentaquintaid, empleadoid, anio, ingresosacumuladosbrutos, impuestoretendidoacumulado, ingresosotroslempleadores)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, rentas_quinta)

        # rrhh_nomina.utilidades
        utilidades = [
            (i, f'UTI-2026-{i:02d}', 2026, 8.00, 2000000.00, 360, 450000.00, 120000.00, '2027-05-15', 'Pendiente', 'Todos', 10, 'Observación')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.utilidades (utilidadid, codigo, ejerciciofiscal, porcentajeparticipacion, utilidadnetadeclarada, diascomputables, remuneracioncomputable, montodistribuido, fechapagoestimada, estado, empleadosaplica, cantidadempleados, observacion)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, utilidades)

        # rrhh_nomina.reportes
        reportes = [
            (i, f'REP-00{i}', f'Reporte Mensual {i}', 'Planillas', 'Mayo 2026', 'PDF', 'admin@sge-enterprise.com', 'Completado', 5, 200)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.reportes (reporteid, codigo, nombre, submodulo, periodo, formato, generadopor, estado, filasgeneradas, tamanokb)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, reportes)

        # rrhh_nomina.declaraciones_pdt
        pdt = [
            (i, f'SUN-00{i}', 'PLAME', 'Mayo 2026', 2026, '2026-06-03 14:30:00', None, 'Pendiente', None, False, 'admin@sge-enterprise.com', 'Falta envío')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.declaraciones_pdt (declaracionid, codigo, tipo, periodo, ejercicio, fechageneracion, fechaenvio, estado, nroorden, tieneconstancia, usuario, observacion)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, pdt)

        # rrhh_nomina.historial_pagos
        pagos = [
            (i, f'PAG-00{i}', f'Planilla Mensual - Mayo 2026 {i}', 'Mayo 2026', '2026-05-30', 'BCP', 17500.00, 'Pendiente', 5, 'Observacion')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.historial_pagos (pagoid, codigo, planillaconcepto, periodo, fechapago, banco, montopagado, estado, empleados, observacion)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, pagos)

        # rrhh_nomina.planillas_resumen
        resumenes = [
            (f'PLA-2026-0{i}', f'Junio 2026 {i}', '2026-06-30', 5, 25000.00, 3250.00, 'En Proceso')
            for i in range(0, 10)
        ]
        cur.executemany("INSERT INTO rrhh_nomina.planillas_resumen (codigo, periodo, fechacierre, empleados, totalbruto, totaldescuentos, estado) VALUES (%s, %s, %s, %s, %s, %s, %s) ON CONFLICT (codigo) DO NOTHING;", resumenes)

        # rrhh_nomina.parametros_generales
        params_gen = [
            (i, f'Mi Empresa SAC {i}', 'Soles (S/)', 30, 30, True, False)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.parametros_generales (paramid, empresa, moneda, diacierreplanilla, diapagoplanilla, calchorasextrasauto, inclferiadosasist)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, params_gen)

        # rrhh_nomina.rangos_renta
        rangos = [
            (i, 0.00 + i * 1000, 1000.00 + i * 1000, 8.00 + i, 0.00, True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.rangos_renta (rangoid, desde, hasta, tasa, montofijo, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, rangos)

        # rrhh_nomina.bancos_config
        bancos = [
            (i, f'Banco {i}', f'BCO-{i}', 'Soles', f'191-3456789-0-1{i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.bancos_config (bancoid, nombre, codigo, moneda, cuentaprincipal, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, bancos)

        # rrhh_nomina.beneficios
        beneficios = [
            (i, f'BEN-00{i}', f'Bono de Pruebas {i}', 'Alimentacion', 'Bonificacion', 'Mensual', 100.00 * i, f'S/ {100.00 * i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.beneficios (beneficioid, codigo, nombre, categoria, tipo, periodicidad, montofijo, montocadena, activo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, beneficios)

        # rrhh_nomina.gratificaciones
        gratificaciones = [
            (i, f'GRA-2026-0{i}', f'Gratificación Fiestas Patrias {i}', 'Obligatoria', 'Julio 2026', 'Semestral', '100% sueldo', 'RemuneracionBasica', 3500.00, 100.00, '2026-07-15', None, 'Pendiente', 'Todos', 10, 'admin@sge-enterprise.com')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.gratificaciones (gratificacionid, codigo, nombre, tipo, periodo, frecuencia, porcentajemonto, basedecalculo, montofijo, porcentaje, fechaestimada, fechapago, estado, empleadosaplica, cantidadempleados, creadopor)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, gratificaciones)

        # rrhh_nomina.essalud_declaraciones
        essalud_dec = [
            (i, f'DEC-2026-0{i}', 'Mayo 2026', 10, 17500.00 * i, 1575.00 * i, '2026-06-03 14:15:00', 'Pendiente', '', '', 0.00, 1575.00 * i, 'Mensual')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO rrhh_nomina.essalud_declaraciones (declaracionid, codigo, periodo, trabajadores, remuneracionasignable, aporteessalud, fechaenvio, estado, nroordensunat, observacion, subsidios, totalpagar, tipo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s);
        """, essalud_dec)


        # ==========================================
        # SCHEMA sistema
        # ==========================================
        print("Seeding Schema: sistema...")

        # sistema.parametros
        parametros = [
            (i, f'CLAVE_PARAMETRO_{i}', f'Valor de configuración {i}', f'Descripción del parámetro {i}', 'GENERAL')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sistema.parametros (parametroid, clave, valor, descripcion, categoria)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s);
        """, parametros)

        # sistema.sesiones_usuarios
        sesiones_usuarios = [
            (i, 'admin@sge-enterprise.com', '127.0.0.1', 'Mozilla/5.0', f'token_{i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sistema.sesiones_usuarios (sesionid, usuario, direccionip, dispositivo, tokenacceso, estasesionactiva)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, sesiones_usuarios)

        # sistema.logs_auditoria_datos
        logs = [
            (i, 'admin@sge-enterprise.com', 'comercial.clientes', 'insert', f'{i}', None, f'Valores nuevos {i}')
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sistema.logs_auditoria_datos (logid, usuario, tablaafectada, accion, idregistroafectado, valoranterior, valornuevo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, logs)

        # sistema.reportes_config
        rep_config = [
            (i, f'REP_CFG_{i}', f'Reporte Configurado {i}', f'Configuración de reporte {i}', 'Ventas', f'sp_reporte_ventas_{i}', True)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sistema.reportes_config (reporteid, codigo, nombre, descripcion, moduloorigen, procedimientonombre, estaactivo)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s, %s);
        """, rep_config)

        # sistema.historial_descargas_reportes
        descargas = [
            (i, i, 'admin@sge-enterprise.com', 'periodo=2026', 'pdf', 100)
            for i in range(1, 11)
        ]
        cur.executemany("""
            INSERT INTO sistema.historial_descargas_reportes (descargareporteid, reporteid, usuario, parametrosusados, formatoexportacion, registrosencontrados)
            OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s, %s, %s);
        """, descargas)


        conn.commit()
        print("All 78 database tables seeded successfully!")
        cur.close()
        conn.close()
    except Exception as e:
        print("Error seeding database:", e)
        sys.exit(1)

if __name__ == '__main__':
    run_seed()
