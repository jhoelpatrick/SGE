(function () {
    const DEFAULT_DB = {
        clientes: [
            { clienteid: 1, tipodocumento: '6', numerodocumento: '20100456781', razonsocial: 'CONSTRUCTORA SAN JOSÉ S.A.C.', nombrecomercial: 'Constructora San José', direccionfiscal: 'Av. Javier Prado Este 1024', ubigeo: '150131', email: 'compras@sanjose.com.pe', telefono: '014223456', tipocliente: 'cliente', estado: true },
            { clienteid: 2, tipodocumento: '6', numerodocumento: '20554433221', razonsocial: 'MINERA DEL SUR OPERACIONES S.A.', nombrecomercial: 'Minera Del Sur', direccionfiscal: 'Las Begonias 450 Piso 8', ubigeo: '150131', email: 'logistica@minerasur.pe', telefono: '017112000', tipocliente: 'cliente', estado: true },
            { clienteid: 3, tipodocumento: '6', numerodocumento: '20887766554', razonsocial: 'DESARROLLOS INMOBILIARIOS LIMA S.A.C.', nombrecomercial: 'DILSA', direccionfiscal: 'Av. Benavides 2344', ubigeo: '150122', email: 'proveedores@dilsa.pe', telefono: '012445566', tipocliente: 'prospecto', estado: true },
            { clienteid: 4, tipodocumento: '1', numerodocumento: '44556677', razonsocial: 'CARLOS ALBERTO MENDOZA RUIZ', nombrecomercial: null, direccionfiscal: 'Jr. Huallaga 451', ubigeo: '150101', email: 'carlos.mendoza@gmail.com', telefono: '999888777', tipocliente: 'prospecto', estado: true }
        ],
        proveedores: [
            { proveedorid: 1, tipodocumento: '6', numerodocumento: '20334455661', razonsocial: 'ACEROS INDUSTRIALES DEL PERÚ S.A.', direccionfiscal: 'Av. Argentina 4560', ubigeo: '070101', telefono: '014512030', email: 'ventas@acerosind.com.pe', estado: true },
            { proveedorid: 2, tipodocumento: '6', numerodocumento: '20112233445', razonsocial: 'CORPORACIÓN LOGÍSTICA TRANSVIAL S.A.C.', direccionfiscal: 'Av. Elmer Faucett 120', ubigeo: '070101', telefono: '015748930', email: 'operaciones@transvial.pe', estado: true },
            { proveedorid: 3, tipodocumento: '6', numerodocumento: '20998877665', razonsocial: 'DISTRIBUIDORA DE MATERIALES AREQUIPA EIRL', direccionfiscal: 'Calle Mercaderes 115', ubigeo: '040101', telefono: '054234567', email: 'ventas.aqp@distrimat.pe', estado: true }
        ],
        productos: [
            { productoid: 1, codigosku: 'PROD-001', descripcion: 'Fierro Corrugado de 1/2 Pulgada', stockactual: 1500, stockminimo: 100, costopromedio: 31.2, precioventasugerido: 42.5, esservicio: false, estado: true },
            { productoid: 2, codigosku: 'PROD-002', descripcion: 'Cemento Sol Tipo I (Bolsa 42.5 kg)', stockactual: 2300, stockminimo: 200, costopromedio: 21.5, precioventasugerido: 28.0, esservicio: false, estado: true },
            { productoid: 3, codigosku: 'PROD-003', descripcion: 'Laptop Corporativa i7 16GB RAM', stockactual: 45, stockminimo: 5, costopromedio: 2800, precioventasugerido: 3500, esservicio: false, estado: true },
            { productoid: 4, codigosku: 'SERV-001', descripcion: 'Servicio de Consultoría en Gestión de Proyectos', stockactual: 9999, stockminimo: 0, costopromedio: 0, precioventasugerido: 150, esservicio: true, estado: true },
            { productoid: 5, codigosku: 'SERV-002', descripcion: 'Servicio de Transporte Logístico Local LIMA', stockactual: 9999, stockminimo: 0, costopromedio: 320, precioventasugerido: 450, esservicio: true, estado: true }
        ],
        proyectos: [
            { proyectoid: 1, clienteid: 1, nombreproyecto: 'Edificio Residencial San José - Miraflores', descripcion: 'Construcción de un condominio de 15 pisos.', presupuestototal: 1500000.0, costoreallogrado: 35700.0, fechainicio: '2026-01-15', fechafin: '2026-12-20', estado: 'en progreso' },
            { proyectoid: 2, clienteid: 2, nombreproyecto: 'Ampliación Planta de Procesos Sur', descripcion: 'Optimización de fajas transportadoras e infraestructura técnica.', presupuestototal: 850000.0, costoreallogrado: 0.0, fechainicio: '2026-03-01', fechafin: '2026-09-30', estado: 'en progreso' },
            { proyectoid: 3, clienteid: 3, nombreproyecto: 'Habilitación Urbana Lomas del Sol', descripcion: 'Obras de saneamiento y pavimentación estructural.', presupuestototal: 450000.0, costoreallogrado: 0.0, fechainicio: '2026-05-10', fechafin: null, estado: 'planificado' }
        ],
        proyectotareas: [
            { tareaid: 1, proyectoid: 1, nombretarea: 'Excavación y Movimiento de Tierras', fechainicio: '2026-01-15', fechafin: '2026-02-28', porcentajeprogreso: 100.0, costoestimado: 80000.0, estado: 'completada' },
            { tareaid: 2, proyectoid: 1, nombretarea: 'Cimentación y Estructuras Base', fechainicio: '2026-03-01', fechafin: '2026-06-30', porcentajeprogreso: 45.5, costoestimado: 350000.0, estado: 'en ejecucion' },
            { tareaid: 3, proyectoid: 2, nombretarea: 'Ingeniería de Detalle y Planos', fechainicio: '2026-03-01', fechafin: '2026-04-15', porcentajeprogreso: 100.0, costoestimado: 30000.0, estado: 'completada' },
            { tareaid: 4, proyectoid: 2, nombretarea: 'Montaje Electromecánico de Fajas', fechainicio: '2026-04-16', fechafin: '2026-08-15', porcentajeprogreso: 15.0, costoestimado: 500000.0, estado: 'en ejecucion' },
            { tareaid: 5, proyectoid: 3, nombretarea: 'Estudio de Impacto Ambiental', fechainicio: '2026-05-15', fechafin: '2026-07-15', porcentajeprogreso: 0.0, costoestimado: 15000.0, estado: 'pendiente' }
        ],
        pedidosventa: [
            { pedidoid: 1, numeropedido: 'PED-2026-001', clienteid: 1, proyectoid: 1, fechaemision: '2026-02-10T10:00:00Z', moneda: 'PEN', tipocambio: 1.0, metodopago: 'credito', totalneto: 42000.0, estado: 'aprobado' },
            { pedidoid: 2, numeropedido: 'PED-2026-002', clienteid: 2, proyectoid: 2, fechaemision: '2026-03-15T11:00:00Z', moneda: 'USD', tipocambio: 3.75, metodopago: 'transferencia', totalneto: 15000.0, estado: 'aprobado' },
            { pedidoid: 3, numeropedido: 'PED-2026-003', clienteid: 4, proyectoid: null, fechaemision: '2026-05-20T15:30:00Z', moneda: 'PEN', tipocambio: 1.0, metodopago: 'visa', totalneto: 3150.0, estado: 'pendiente' }
        ],
        pedidosventadetalle: [
            { detalledid: 1, pedidoid: 1, productoid: 1, cantidad: 1000, preciounitariocongiv: 42.5, descuento: 500, totalfila: 42000 },
            { detalledid: 2, pedidoid: 2, productoid: 4, cantidad: 100, preciounitariocongiv: 150, descuento: 0, totalfila: 15000 },
            { detalledid: 3, pedidoid: 3, productoid: 3, cantidad: 1, preciounitariocongiv: 3500, descuento: 350, totalfila: 3150 }
        ],
        ordenescompra: [
            { ordenid: 1, numeroorden: 'OC-2026-001', proveedorid: 1, proyectoid: 1, solicitante: 'Ing. Luis Fernando Gómez', fechaemision: '2026-02-15T09:00:00Z', moneda: 'PEN', categoriagasto: 'materiales', monto_total: 31200.0, estado: 'aprobado' },
            { ordenid: 2, numeroorden: 'OC-2026-002', proveedorid: 2, proyectoid: 1, solicitante: 'Lic. Maria Elena Paz', fechaemision: '2026-02-18T14:30:00Z', moneda: 'PEN', categoriagasto: 'logistica', monto_total: 4500.0, estado: 'aprobado' },
            { ordenid: 3, numeroorden: 'OC-2026-003', proveedorid: 3, proyectoid: 2, solicitante: 'Ing. Roberto Carlos Arce', fechaemision: '2026-04-01T10:15:00Z', moneda: 'PEN', categoriagasto: 'materiales', monto_total: 10750.0, estado: 'pendiente' }
        ],
        ordenescompradetalle: [
            { detalledoc: 1, ordenid: 1, productoid: 1, cantidad: 1000, costounitariocongiv: 31.2, totalfila: 31200 },
            { detalledoc: 2, ordenid: 2, productoid: 5, cantidad: 10, costounitariocongiv: 450, totalfila: 4500 },
            { detalledoc: 3, ordenid: 3, productoid: 2, cantidad: 500, costounitariocongiv: 21.5, totalfila: 10750 }
        ],
        comprobantesfacturacion: [
            { comprobanteid: 1, pedidoid: 1, tipocomprobante: 'factura', serie: 'F001', correlativo: '00000001', fechaemision: '2026-02-12T17:00:00Z', importetotalneto: 42000.0, estadosunat: 'aceptado' },
            { comprobanteid: 2, pedidoid: 2, tipocomprobante: 'factura', serie: 'F001', correlativo: '00000002', fechaemision: '2026-03-16T12:00:00Z', importetotalneto: 15000.0, estadosunat: 'aceptado' }
        ],
        guiasremision: [
            { guiaid: 1, serie: 'T001', correlativo: '00000001', fechatraslado: '2026-02-20T08:00:00Z', motivodetraslado: 'Traslado entre almacenes', vehiculoid: 1, conductorid: 1, estadosunat: 'aceptado' },
            { guiaid: 2, serie: 'T001', correlativo: '00000002', fechatraslado: '2026-02-22T09:00:00Z', motivodetraslado: 'Venta', vehiculoid: 2, conductorid: 2, estadosunat: 'aceptado' }
        ],
        kardex: [
            { movimientoid: 1, productoid: 1, fechamovimiento: '2026-02-15T09:00:00Z', tipomovimiento: 'ingreso', motivo: 'Compra según OC-2026-001', cantidad: 1000, saldoposterior: 1500 },
            { movimientoid: 2, productoid: 1, fechamovimiento: '2026-02-22T10:00:00Z', tipomovimiento: 'salida', motivo: 'Venta según PED-2026-001', cantidad: 500, saldoposterior: 1000 }
        ],
        vehiculos: [
            { vehiculoid: 1, proveedorid: 2, placa: 'F3G-820', marca: 'Volvo', modelo: 'FMX 460', tipovehiculo: 'Tractocamión Volquete', estado: true },
            { vehiculoid: 2, proveedorid: 2, placa: 'B4W-711', marca: 'Scania', modelo: 'P410', tipovehiculo: 'Camión Plataforma', estado: true },
            { vehiculoid: 3, proveedorid: 3, placa: 'V1Z-943', marca: 'Hyundai', modelo: 'HD78', tipovehiculo: 'Camión Furgón 5 Tn', estado: true }
        ],
        conductores: [
            { conductorid: 1, proveedorid: 2, nombre: 'Pedro Manuel Flores Quispe', numerodocumento: '10203040', licenciaconducir: 'Q10203040-A3C', estado: true },
            { conductorid: 2, proveedorid: 2, nombre: 'Jorge Washington Cárdenas Vega', numerodocumento: '08123456', licenciaconducir: 'V08123456-A3B', estado: true },
            { conductorid: 3, proveedorid: 3, nombre: 'Aurelio Segundo Condori Mamani', numerodocumento: '29456712', licenciaconducir: 'M29456712-A2B', estado: true }
        ]
    };

    window.SGE_Db = {
        get: function () {
            let data = localStorage.getItem('SGE_Db_Store');
            if (!data) {
                localStorage.setItem('SGE_Db_Store', JSON.stringify(DEFAULT_DB));
                return JSON.parse(JSON.stringify(DEFAULT_DB));
            }
            return JSON.parse(data);
        },
        save: function (db) {
            localStorage.setItem('SGE_Db_Store', JSON.stringify(db));
        },
        aprobarOrdenCompra: function (id) {
            const db = this.get();
            const o = db.ordenescompra.find(x => x.ordenid === id);
            if (o && o.estado === 'pendiente') {
                o.estado = 'aprobado';
                // Registrar ingreso físico al stock
                const details = db.ordenescompradetalle.filter(x => x.ordenid === id);
                details.forEach(d => {
                    const prod = db.productos.find(p => p.productoid == d.productoid);
                    if (prod) {
                        const oldStock = parseFloat(prod.stockactual);
                        const qty = parseFloat(d.cantidad);
                        prod.stockactual = oldStock + qty;
                        // Kardex
                        db.kardex.push({
                            movimientoid: db.kardex.length + 1,
                            productoid: prod.productoid,
                            fechamovimiento: new Date().toISOString(),
                            tipomovimiento: 'ingreso',
                            motivo: 'Compra según ' + o.numeroorden,
                            cantidad: qty,
                            saldoposterior: prod.stockactual
                        });
                    }
                });
                // Cargar costo al proyecto
                const proj = db.proyectos.find(x => x.proyectoid === o.proyectoid);
                if (proj) {
                    proj.costoreallogrado = parseFloat(proj.costoreallogrado) + parseFloat(o.monto_total);
                }
                this.save(db);
                return { success: true };
            }
            return { success: false };
        },
        rechazarOrdenCompra: function (id) {
            const db = this.get();
            const o = db.ordenescompra.find(x => x.ordenid === id);
            if (o && o.estado === 'pendiente') {
                o.estado = 'rechazado';
                this.save(db);
                return { success: true };
            }
            return { success: false };
        },
        registrarOrdenCompra: function (provId, projId, cat, req, items) {
            const db = this.get();
            const nextId = db.ordenescompra.length ? Math.max(...db.ordenescompra.map(x => x.ordenid)) + 1 : 1;
            const num = 'OC-2026-' + String(nextId).padStart(3, '0');
            
            let total = 0;
            items.forEach(i => {
                total += parseFloat(i.cantidad) * parseFloat(i.costounitariocongiv);
            });

            const newOc = {
                ordenid: nextId,
                numeroorden: num,
                proveedorid: parseInt(provId),
                proyectoid: parseInt(projId),
                solicitante: req,
                fechaemision: new Date().toISOString(),
                moneda: 'PEN',
                categoriagasto: cat,
                monto_total: total,
                estado: 'pendiente'
            };
            db.ordenescompra.push(newOc);

            items.forEach((item, idx) => {
                db.ordenescompradetalle.push({
                    detalledoc: db.ordenescompradetalle.length + 1,
                    ordenid: nextId,
                    productoid: parseInt(item.productoid),
                    cantidad: parseFloat(item.cantidad),
                    costounitariocongiv: parseFloat(item.costounitariocongiv),
                    totalfila: parseFloat(item.cantidad) * parseFloat(item.costounitariocongiv)
                });
            });

            this.save(db);
            return { success: true, orden: newOc };
        },
        registrarMovimientoKardex: function (prodId, type, qty, reason, ref) {
            const db = this.get();
            const prod = db.productos.find(p => p.productoid == prodId);
            if (!prod) throw new Error('Producto no encontrado');

            const qVal = parseFloat(qty);
            if (type === 'salida' && prod.stockactual < qVal) {
                throw new Error('Stock insuficiente');
            }

            if (type === 'ingreso') {
                prod.stockactual = parseFloat(prod.stockactual) + qVal;
            } else {
                prod.stockactual = parseFloat(prod.stockactual) - qVal;
            }

            db.kardex.push({
                movimientoid: db.kardex.length + 1,
                productoid: prod.productoid,
                fechamovimiento: new Date().toISOString(),
                tipomovimiento: type,
                motivo: reason + (ref ? ' Ref: ' + ref : ''),
                cantidad: qVal,
                saldoposterior: prod.stockactual
            });

            this.save(db);
            return { success: true, nuevoSaldo: prod.stockactual };
        },
        despacharPedidoVenta: function (pedId, vehId, condId, serie, corr) {
            const db = this.get();
            const p = db.pedidosventa.find(x => x.pedidoid == pedId);
            if (!p) throw new Error('Pedido no encontrado');
            if (p.estado !== 'aprobado') throw new Error('El pedido debe estar aprobado para despacharlo');

            p.estado = 'despachado';

            // Agregar Guía Remisión
            db.guiasremision.push({
                guiaid: db.guiasremision.length + 1,
                serie: serie || 'T001',
                correlativo: corr || String(db.guiasremision.length + 1).padStart(8, '0'),
                fechatraslado: new Date().toISOString(),
                motivodetraslado: 'Venta',
                vehiculoid: parseInt(vehId),
                conductorid: parseInt(condId),
                estadosunat: 'aceptado'
            });

            this.save(db);
            return { success: true };
        },
        aprobarPedidoVenta: function (id) {
            const db = this.get();
            const p = db.pedidosventa.find(x => x.pedidoid == id);
            if (p && p.estado === 'pendiente') {
                p.estado = 'aprobado';
                // Descontar del stock físico
                const details = db.pedidosventadetalle.filter(x => x.pedidoid == id);
                details.forEach(d => {
                    const prod = db.productos.find(pr => pr.productoid == d.productoid);
                    if (prod) {
                        prod.stockactual = parseFloat(prod.stockactual) - parseFloat(d.cantidad);
                        db.kardex.push({
                            movimientoid: db.kardex.length + 1,
                            productoid: prod.productoid,
                            fechamovimiento: new Date().toISOString(),
                            tipomovimiento: 'salida',
                            motivo: 'Despacho Venta ' + p.numeropedido,
                            cantidad: parseFloat(d.cantidad),
                            saldoposterior: prod.stockactual
                        });
                    }
                });
                this.save(db);
                return { success: true };
            }
            return { success: false };
        },
        cancelarPedidoVenta: function (id) {
            const db = this.get();
            const p = db.pedidosventa.find(x => x.pedidoid == id);
            if (p && p.estado === 'pendiente') {
                p.estado = 'cancelado';
                this.save(db);
                return { success: true };
            }
            return { success: false };
        },
        generarFacturaDesdePedido: function (id, type, serie) {
            const db = this.get();
            const p = db.pedidosventa.find(x => x.pedidoid == id);
            if (!p) return { success: false, message: 'Pedido no encontrado' };

            const nextId = db.comprobantesfacturacion.length ? Math.max(...db.comprobantesfacturacion.map(x=>x.comprobanteid))+1 : 1;
            const newCorrelativo = String(nextId).padStart(8, '0');
            const fac = {
                comprobanteid: nextId,
                pedidoid: id,
                tipocomprobante: type || '01',
                serie: serie || 'F001',
                correlativo: newCorrelativo,
                fechaemision: new Date().toISOString(),
                importetotalneto: p.totalneto,
                estadosunat: 'aceptado'
            };
            db.comprobantesfacturacion.push(fac);
            this.save(db);
            return { success: true, factura: fac };
        },
        registrarPedidoVenta: function (clientid, proyectoid, moneda, metodopago, items) {
            const db = this.get();
            const nextId = db.pedidosventa.length ? Math.max(...db.pedidosventa.map(x => x.pedidoid)) + 1 : 1;
            const num = 'PED-2026-' + String(nextId).padStart(3, '0');
            
            let total = 0;
            items.forEach(i => {
                total += (parseFloat(i.cantidad) * parseFloat(i.preciounitariocongiv)) - parseFloat(i.descuento || 0);
            });

            const newPed = {
                pedidoid: nextId,
                numeropedido: num,
                clienteid: parseInt(clientid),
                proyectoid: proyectoid ? parseInt(proyectoid) : null,
                fechaemision: new Date().toISOString(),
                moneda: moneda || 'PEN',
                tipocambio: moneda === 'USD' ? 3.75 : 1.0,
                metodopago: metodopago,
                totalneto: total,
                estado: 'pendiente'
            };
            db.pedidosventa.push(newPed);

            items.forEach(item => {
                db.pedidosventadetalle.push({
                    detalledid: db.pedidosventadetalle.length + 1,
                    pedidoid: nextId,
                    productoid: parseInt(item.productoid),
                    cantidad: parseFloat(item.cantidad),
                    preciounitariocongiv: parseFloat(item.preciounitariocongiv),
                    descuento: parseFloat(item.descuento || 0),
                    totalfila: (parseFloat(item.cantidad) * parseFloat(item.preciounitariocongiv)) - parseFloat(item.descuento || 0)
                });
            });

            this.save(db);
            return { success: true, pedido: newPed };
        },
        crearProyecto: function (name, clientId, desc, budget, start, end) {
            const db = this.get();
            const nextId = db.proyectos.length ? Math.max(...db.proyectos.map(x => x.proyectoid)) + 1 : 1;
            const newProj = {
                proyectoid: nextId,
                clienteid: parseInt(clientId),
                nombreproyecto: name,
                descripcion: desc,
                presupuestototal: parseFloat(budget),
                costoreallogrado: 0.0,
                fechainicio: start,
                fechafin: end || null,
                estado: 'planificado'
            };
            db.proyectos.push(newProj);
            this.save(db);
            return { success: true, proyecto: newProj };
        },
        crearTarea: function (activeProjectId, name, start, end, cost) {
            const db = this.get();
            const nextId = db.proyectotareas.length ? Math.max(...db.proyectotareas.map(x => x.tareaid)) + 1 : 1;
            const newT = {
                tareaid: nextId,
                proyectoid: parseInt(activeProjectId),
                nombretarea: name,
                fechainicio: start,
                fechafin: end,
                porcentajeprogreso: 0.0,
                costoestimado: parseFloat(cost),
                estado: 'pendiente'
            };
            db.proyectotareas.push(newT);
            this.save(db);
            return { success: true, tarea: newT };
        },
        actualizarProgresoTarea: function (tareaId, progress, status) {
            const db = this.get();
            const t = db.proyectotareas.find(x => x.tareaid == tareaId);
            if (t) {
                t.porcentajeprogreso = parseFloat(progress);
                t.estado = status;
                this.save(db);
                return { success: true };
            }
            return { success: false };
        }
    };
})();
