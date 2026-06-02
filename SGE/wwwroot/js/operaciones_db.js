/**
 * SGE - Simulador de Base de Datos y Lógica Transaccional en el Cliente (UI/UX)
 * Propósito: Emular la base de datos SQL de script_crm.sql y los procedimientos almacenados
 * de operaciones en el navegador para hacer que las pantallas interactúen de forma real.
 */

(function () {
    const DB_KEY = 'SGE_Operaciones_DB';

    // Datos Iniciales por defecto
    const defaultData = {
        clientes: [
            { clienteid: 1, tipodocumento: '6', numerodocumento: '20100078945', razonsocial: 'Aceros Arequipa S.A.', email: 'compras@aceros.com.pe', telefono: '01-518-2000', tipocliente: 'corporativo', ubigeo: '150101', direccionfiscal: 'Av. Enrique Meiggs 270, Chimbote', estado: true },
            { clienteid: 2, tipodocumento: '6', numerodocumento: '20546589321', razonsocial: 'Constructora Graña S.A.C.', email: 'proyectos@grana.pe', telefono: '01-213-9000', tipocliente: 'corporativo', ubigeo: '150140', direccionfiscal: 'Av. Paseo de la República 4675, Surquillo', estado: true },
            { clienteid: 3, tipodocumento: '6', numerodocumento: '20875412589', razonsocial: 'Minera Las Bambas S.A.', email: 'adquisiciones@lasbambas.com', telefono: '01-415-3000', tipocliente: 'corporativo', ubigeo: '030501', direccionfiscal: 'Fundo Santa Rosa, Challhuahuacho, Cotabambas', estado: true },
            { clienteid: 4, tipodocumento: '1', numerodocumento: '45879652', razonsocial: 'Juan Perez Torres', email: 'jperez@gmail.com', telefono: '999888777', tipocliente: 'particular', ubigeo: '150101', direccionfiscal: 'Av. Larco 456, Miraflores', estado: true }
        ],
        proveedores: [
            { proveedorid: 1, tipodocumento: '6', numerodocumento: '20456123987', razonsocial: 'Cemex Perú S.A.', direccionfiscal: 'Av. Víctor Andrés Belaúnde 147, San Isidro', ubigeo: '150131', telefono: '01-610-3300', email: 'ventas@cemex.com.pe', estado: true },
            { proveedorid: 2, tipodocumento: '6', numerodocumento: '20100456321', razonsocial: 'Sodimac Constructor', direccionfiscal: 'Av. Angamos Este 1805, Surquillo', ubigeo: '150140', telefono: '01-615-6000', email: 'ventascorporativas@sodimac.com.pe', estado: true }
        ],
        vehiculos: [
            { vehiculoid: 1, proveedorid: 1, placa: 'D2X-840', marca: 'Volvo', modelo: 'FMX', tipovehiculo: 'Tractocamión', estado: true },
            { vehiculoid: 2, proveedorid: 2, placa: 'F3M-710', marca: 'Hyundai', modelo: 'H100', tipovehiculo: 'Furgón', estado: true }
        ],
        conductores: [
            { conductorid: 1, proveedorid: 1, nombre: 'Carlos Mendoza Silva', tipodocumento: '1', numerodocumento: '09876543', licenciaconducir: 'Q09876543-AIIIc', estado: true },
            { conductorid: 2, proveedorid: 2, nombre: 'Luis Alberto Gomez', tipodocumento: '1', numerodocumento: '41235678', licenciaconducir: 'A41235678-AIIb', estado: true }
        ],
        productos: [
            { productoid: 1, codigosku: 'MAT-CEM-001', codigosunat: '30111501', descripcion: 'Cemento Sol Tipo I - Bolsa 42.5kg', unidadmedida: 'UND', precioventasugerido: 28.50, costopromedio: 21.00, esservicio: false, estado: true },
            { productoid: 2, codigosku: 'MAT-FIER-002', codigosunat: '30111502', descripcion: 'Fierro Corrugado 1/2 pulgada Aceros Arequipa', unidadmedida: 'UND', precioventasugerido: 42.00, costopromedio: 32.50, esservicio: false, estado: true },
            { productoid: 3, codigosku: 'EQ-TAB-004', codigosunat: '39121101', descripcion: 'Tablero Eléctrico Industrial Trifásico 380V', unidadmedida: 'UND', precioventasugerido: 2450.00, costopromedio: 1800.00, esservicio: false, estado: true },
            { productoid: 4, codigosku: 'SERV-INST-003', codigosunat: '72101501', descripcion: 'Servicio de Instalación y Montaje Eléctrico', unidadmedida: 'ZZ', precioventasugerido: 1500.00, costopromedio: 800.00, esservicio: true, estado: true }
        ],
        almacenes: [
            { almacenid: 1, codigoalmacen: 'ALM-PRI', nombre: 'Almacén Principal Lurín', direccion: 'Av. Industrial Sub-Lote 4A, Lurín', ubigeo: '150119', estado: true },
            { almacenid: 2, codigoalmacen: 'ALM-CHI', nombre: 'Almacén Sucursal Chiclayo', direccion: 'Av. Mensajeros de la Paz 450, Chiclayo', ubigeo: '140101', estado: true }
        ],
        stockalmacen: [
            { almacenid: 1, productoid: 1, stockactual: 550, stockcomprometido: 0 },
            { almacenid: 1, productoid: 2, stockactual: 320, stockcomprometido: 0 },
            { almacenid: 1, productoid: 3, stockactual: 8, stockcomprometido: 0 },
            { almacenid: 1, productoid: 4, stockactual: 0, stockcomprometido: 0 }, // Servicio tiene 0 stock siempre
            { almacenid: 2, productoid: 1, stockactual: 120, stockcomprometido: 0 },
            { almacenid: 2, productoid: 2, stockactual: 80, stockcomprometido: 0 },
            { almacenid: 2, productoid: 3, stockactual: 2, stockcomprometido: 0 },
            { almacenid: 2, productoid: 4, stockactual: 0, stockcomprometido: 0 }
        ],
        proyectos: [
            { proyectoid: 1, clienteid: 1, nombreproyecto: 'Implementación Planta Lurín Stage 1', descripcion: 'Obras de cimentación, montaje de estructuras metálicas y conexionado eléctrico de tableros para la nueva nave industrial.', presupuestototal: 150000.00, costoreallogrado: 45000.00, fechainicio: '2026-01-15', fechafin: '2026-09-30', estado: 'en progreso', fecharegistro: '2026-01-10T10:00:00Z' },
            { proyectoid: 2, clienteid: 2, nombreproyecto: 'Ampliación Oficina Principal Surquillo', descripcion: 'Ampliación del segundo piso administrativo de la sede corporativa de Graña en Lima.', presupuestototal: 85000.00, costoreallogrado: 82000.00, fechainicio: '2026-03-01', fechafin: '2026-06-30', estado: 'en progreso', fecharegistro: '2026-02-25T08:30:00Z' },
            { proyectoid: 3, clienteid: 3, nombreproyecto: 'Mantenimiento Sistema Eléctrico Las Bambas', descripcion: 'Parada de planta anual y mantenimiento preventivo de los tableros trifásicos del tajo principal.', presupuestototal: 50000.00, costoreallogrado: 0.00, fechainicio: '2026-07-01', fechafin: '2026-08-30', estado: 'planificado', fecharegistro: '2026-05-15T11:45:00Z' }
        ],
        proyectotareas: [
            { tareaid: 1, proyectoid: 1, nombretarea: 'Cimentación de bases y columnas', fechainicio: '2026-01-20', fechafin: '2026-03-10', porcentajeprogreso: 100.00, costoestimado: 25000.00, estado: 'completada' },
            { tareaid: 2, proyectoid: 1, nombretarea: 'Estructuras metálicas y techado', fechainicio: '2026-03-15', fechafin: '2026-06-20', porcentajeprogreso: 60.00, costoestimado: 15000.00, estado: 'en ejecucion' },
            { tareaid: 3, proyectoid: 1, nombretarea: 'Instalación de tableros eléctricos', fechainicio: '2026-07-01', fechafin: '2026-08-15', porcentajeprogreso: 0.00, costoestimado: 5000.00, estado: 'pendiente' },
            { tareaid: 4, proyectoid: 1, nombretarea: 'Pruebas eléctricas y de carga', fechainicio: '2026-09-01', fechafin: '2026-09-25', porcentajeprogreso: 0.00, costoestimado: 2000.00, estado: 'pendiente' },
            { tareaid: 5, proyectoid: 2, nombretarea: 'Desmontaje y demoliciones', fechainicio: '2026-03-02', fechafin: '2026-03-25', porcentajeprogreso: 100.00, costoestimado: 12000.00, estado: 'completada' },
            { tareaid: 6, proyectoid: 2, nombretarea: 'Tabiquería dry-wall y pintura', fechainicio: '2026-04-01', fechafin: '2026-05-15', porcentajeprogreso: 100.00, costoestimado: 40000.00, estado: 'completada' },
            { tareaid: 7, proyectoid: 2, nombretarea: 'Falsos techos y aire acondicionado', fechainicio: '2026-05-16', fechafin: '2026-06-25', porcentajeprogreso: 85.00, costoestimado: 30000.00, estado: 'en ejecucion' }
        ],
        pedidosventa: [
            { pedidoid: 1, numeropedido: 'PED-2026-001', clienteid: 1, proyectoid: 1, fechaemision: '2026-05-10T14:30:00Z', moneda: 'PEN', tipocambio: 1.0000, metodopago: 'credito', cupondescuento: null, montobruto: 12711.86, montodescuento: 0.00, totalneto: 15000.00, estado: 'aprobado' },
            { pedidoid: 2, numeropedido: 'PED-2026-002', clienteid: 2, proyectoid: 2, fechaemision: '2026-05-20T09:15:00Z', moneda: 'PEN', tipocambio: 1.0000, metodopago: 'transferencia', cupondescuento: null, montobruto: 3559.32, montodescuento: 0.00, totalneto: 4200.00, estado: 'despachado' }
        ],
        pedidosventadetalle: [
            { detalledid: 1, pedidoid: 1, productoid: 1, cantidad: 200, preciounitariocongiv: 28.50, descuento: 0.00, totalfila: 5700.00 },
            { detalledid: 2, pedidoid: 1, productoid: 2, cantidad: 100, preciounitariocongiv: 42.00, descuento: 0.00, totalfila: 4200.00 },
            { detalledid: 3, pedidoid: 1, productoid: 4, cantidad: 3.4, preciounitariocongiv: 1500.00, descuento: 0.00, totalfila: 5100.00 },
            { detalledid: 4, pedidoid: 2, productoid: 1, cantidad: 100, preciounitariocongiv: 28.50, descuento: 0.00, totalfila: 2850.00 },
            { detalledid: 5, pedidoid: 2, productoid: 2, cantidad: 32.14, preciounitariocongiv: 42.00, descuento: 0.00, totalfila: 1350.00 }
        ],
        ordenescompra: [
            { ordenid: 1, numeroorden: 'OC-2026-001', proveedorid: 1, proyectoid: 1, solicitante: 'Ing. Jose Diaz', fechaemision: '2026-05-12T16:00:00Z', moneda: 'PEN', monto_total: 12000.00, categoriagasto: 'materiales', estado: 'aprobado' },
            { ordenid: 2, numeroorden: 'OC-2026-002', proveedorid: 2, proyectoid: 2, solicitante: 'Arq. Maria Silva', fechaemision: '2026-05-25T11:00:00Z', moneda: 'PEN', monto_total: 8500.00, categoriagasto: 'equipos', estado: 'pendiente' }
        ],
        ordenescompradetalle: [
            { detalledoc: 1, ordenid: 1, productoid: 1, cantidad: 500, costounitariocongiv: 24.00, totalfila: 12000.00 },
            { detalledoc: 2, ordenid: 2, productoid: 3, cantidad: 5, costounitariocongiv: 1700.00, totalfila: 8500.00 }
        ],
        comprobantesfacturacion: [
            { comprobanteid: 1, pedidoid: 2, tipocomprobante: '01', serie: 'F001', correlativo: '00000101', fechaemision: '2026-05-21T10:00:00Z', tipooperacionsunat: '01', clienteid: 2, moneda: 'PEN', opgravada: 3559.32, opinafecta: 0.00, opexonerada: 0.00, igv_total: 640.68, importetotalneto: 4200.00, tipoimpuestoespecial: 'ninguno', estadosunat: 'aceptado' }
        ],
        guiasremision: [
            { guiaid: 1, serie: 'T001', correlativo: '00000052', fechaemision: '2026-05-20T12:00:00Z', motivotraslado: '01', almacenorigenid: 1, almacendestinoid: null, proveedorid: null, vehiculoid: 1, conductorid: 1, pesototal: 4250.00, unidadmedidapeso: 'KGM', estadosunat: 'aceptado' }
        ],
        kardexmovimientos: [
            { movimientoid: 1, almacenid: 1, productoid: 1, tipomovimiento: 'ent', conceptomovimiento: 'compra', documentoreferencia: 'OC-2026-001', cantidad: 500, costounitariomovimiento: 20.33, fechamovimiento: '2026-05-12T18:00:00Z' },
            { movimientoid: 2, almacenid: 1, productoid: 1, tipomovimiento: 'sal', conceptomovimiento: 'venta', documentoreferencia: 'PED-2026-002', cantidad: 100, costounitariomovimiento: 21.00, fechamovimiento: '2026-05-20T12:00:00Z' }
        ]
    };

    // Funciones Helper para Guardar / Cargar en LocalStorage
    function loadDB() {
        const stored = localStorage.getItem(DB_KEY);
        if (!stored) {
            localStorage.setItem(DB_KEY, JSON.stringify(defaultData));
            return defaultData;
        }
        return JSON.parse(stored);
    }

    function saveDB(db) {
        localStorage.setItem(DB_KEY, JSON.stringify(db));
    }

    // Inicializar base de datos en window
    const SGE_Db = {
        reset: function () {
            saveDB(defaultData);
            return defaultData;
        },

        get: function () {
            return loadDB();
        },

        // --- MÉTODOS DE TRANSACCIÓN (PROCEDIMIENTOS ALMACENADOS EMULADOS) ---

        /**
         * sp_operaciones_registrar_movimiento_kardex
         */
        registrarMovimientoKardex: function (almacenid, productoid, tipomovimiento, conceptomovimiento, documentoreferencia, cantidad, costounitariomovimiento) {
            const db = loadDB();
            almacenid = parseInt(almacenid);
            productoid = parseInt(productoid);
            cantidad = parseFloat(cantidad);
            costounitariomovimiento = parseFloat(costounitariomovimiento);

            // Validar que exista el casillero de stock
            let stockRow = db.stockalmacen.find(s => s.almacenid === almacenid && s.productoid === productoid);
            if (!stockRow) {
                stockRow = { almacenid, productoid, stockactual: 0, stockcomprometido: 0 };
                db.stockalmacen.push(stockRow);
            }

            // Operar stock según tipo de movimiento
            if (tipomovimiento === 'ent') {
                stockRow.stockactual += cantidad;
            } else if (tipomovimiento === 'sal') {
                if (stockRow.stockactual < cantidad) {
                    throw new Error('Stock insuficiente: No hay existencias necesarias en este almacén para procesar la salida.');
                }
                stockRow.stockactual -= cantidad;
            } else {
                throw new Error('Tipo de movimiento inválido: Debe ser "ent" o "sal".');
            }

            // Insertar registro histórico en kardex
            const newMovId = db.kardexmovimientos.length ? Math.max(...db.kardexmovimientos.map(k => parseInt(k.movimientoid))) + 1 : 1;
            db.kardexmovimientos.push({
                movimientoid: newMovId,
                almacenid,
                productoid,
                tipomovimiento,
                conceptomovimiento,
                documentoreferencia,
                cantidad,
                costounitariomovimiento,
                fechamovimiento: new Date().toISOString()
            });

            saveDB(db);
            return { success: true, stockactual: stockRow.stockactual };
        },

        /**
         * sp_operaciones_vincular_gasto_proyecto
         */
        vincularGastoProyecto: function (ordenid) {
            const db = loadDB();
            ordenid = parseInt(ordenid);
            const orden = db.ordenescompra.find(o => o.ordenid === ordenid);
            
            if (orden && orden.estado === 'aprobado' && orden.proyectoid) {
                const proyecto = db.proyectos.find(p => p.proyectoid === orden.proyectoid);
                if (proyecto) {
                    proyecto.costoreallogrado += parseFloat(orden.monto_total);
                    saveDB(db);
                    return { success: true, costoreallogrado: proyecto.costoreallogrado };
                }
            }
            return { success: false };
        },

        // --- OPERACIONES DE VENTAS ---
        aprobarPedidoVenta: function (pedidoid) {
            const db = loadDB();
            pedidoid = parseInt(pedidoid);
            const pedido = db.pedidosventa.find(p => p.pedidoid === pedidoid);
            if (pedido && pedido.estado === 'pendiente') {
                pedido.estado = 'aprobado';
                
                // Comprometer stock del almacén 1 (Lurín) por defecto para demostración
                const detalles = db.pedidosventadetalle.filter(d => d.pedidoid === pedidoid);
                detalles.forEach(det => {
                    const prod = db.productos.find(p => p.productoid === det.productoid);
                    if (prod && !prod.esservicio) {
                        const stock = db.stockalmacen.find(s => s.almacenid === 1 && s.productoid === det.productoid);
                        if (stock) {
                            stock.stockcomprometido += parseFloat(det.cantidad);
                        }
                    }
                });

                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'Pedido no encontrado o ya aprobado' };
        },

        despacharPedidoVenta: function (pedidoid, vehiculoid, conductorid, serieGuia, correlativoGuia) {
            const db = loadDB();
            pedidoid = parseInt(pedidoid);
            const pedido = db.pedidosventa.find(p => p.pedidoid === pedidoid);
            if (pedido && pedido.estado === 'aprobado') {
                pedido.estado = 'despachado';
                const detalles = db.pedidosventadetalle.filter(d => d.pedidoid === pedidoid);
                
                // Descontar físicamente del stock y liberar el comprometido
                detalles.forEach(det => {
                    const prod = db.productos.find(p => p.productoid === det.productoid);
                    if (prod && !prod.esservicio) {
                        // sp_operaciones_registrar_movimiento_kardex emulado
                        const stockRow = db.stockalmacen.find(s => s.almacenid === 1 && s.productoid === det.productoid);
                        if (stockRow) {
                            if (stockRow.stockactual < det.cantidad) {
                                throw new Error(`Stock insuficiente de ${prod.descripcion} en Almacén Principal para despachar.`);
                            }
                            stockRow.stockactual -= parseFloat(det.cantidad);
                            stockRow.stockcomprometido = Math.max(0, stockRow.stockcomprometido - parseFloat(det.cantidad));

                            // Kardex log
                            const newMovId = db.kardexmovimientos.length ? Math.max(...db.kardexmovimientos.map(k => parseInt(k.movimientoid))) + 1 : 1;
                            db.kardexmovimientos.push({
                                movimientoid: newMovId,
                                almacenid: 1,
                                productoid: det.productoid,
                                tipomovimiento: 'sal',
                                conceptomovimiento: 'venta',
                                documentoreferencia: pedido.numeropedido,
                                cantidad: det.cantidad,
                                costounitariomovimiento: prod.costopromedio,
                                fechamovimiento: new Date().toISOString()
                            });
                        }
                    }
                });

                // Crear Guía de Remisión si se proporcionan datos de despacho
                if (vehiculoid && conductorid) {
                    const newGuiaId = db.guiasremision.length ? Math.max(...db.guiasremision.map(g => g.guiaid)) + 1 : 1;
                    db.guiasremision.push({
                        guiaid: newGuiaId,
                        serie: serieGuia || 'T001',
                        correlativo: correlativoGuia || String(newGuiaId).padStart(8, '0'),
                        fechaemision: new Date().toISOString(),
                        motivotraslado: '01', // Venta
                        almacenorigenid: 1,
                        almacendestinoid: null,
                        proveedorid: null,
                        vehiculoid: parseInt(vehiculoid),
                        conductorid: parseInt(conductorid),
                        pesototal: detalles.reduce((acc, curr) => acc + (parseFloat(curr.cantidad) * 2), 0) || 50, // Peso estimado
                        unidadmedidapeso: 'KGM',
                        estadosunat: 'aceptado'
                    });
                }

                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'El pedido debe estar en estado Aprobado para ser despachado.' };
        },

        cancelarPedidoVenta: function (pedidoid) {
            const db = loadDB();
            pedidoid = parseInt(pedidoid);
            const pedido = db.pedidosventa.find(p => p.pedidoid === pedidoid);
            if (pedido && pedido.estado !== 'despachado' && pedido.estado !== 'cancelado') {
                const oldEstado = pedido.estado;
                pedido.estado = 'cancelado';

                // Liberar comprometido si estaba aprobado
                if (oldEstado === 'aprobado') {
                    const detalles = db.pedidosventadetalle.filter(d => d.pedidoid === pedidoid);
                    detalles.forEach(det => {
                        const prod = db.productos.find(p => p.productoid === det.productoid);
                        if (prod && !prod.esservicio) {
                            const stock = db.stockalmacen.find(s => s.almacenid === 1 && s.productoid === det.productoid);
                            if (stock) {
                                stock.stockcomprometido = Math.max(0, stock.stockcomprometido - parseFloat(det.cantidad));
                            }
                        }
                    });
                }

                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'No se puede cancelar un pedido ya despachado.' };
        },

        registrarPedidoVenta: function (clienteid, proyectoid, moneda, metodopago, items) {
            const db = loadDB();
            clienteid = parseInt(clienteid);
            proyectoid = proyectoid ? parseInt(proyectoid) : null;
            
            const newPedidoId = db.pedidosventa.length ? Math.max(...db.pedidosventa.map(p => p.pedidoid)) + 1 : 1;
            const numeroPedido = `PED-2026-${String(newPedidoId).padStart(3, '0')}`;

            let totalNeto = 0;
            const detItems = [];

            items.forEach((item, index) => {
                const prod = db.productos.find(p => p.productoid === parseInt(item.productoid));
                if (prod) {
                    const preciounit = parseFloat(item.preciounitariocongiv) || prod.precioventasugerido;
                    const cantidad = parseFloat(item.cantidad) || 1;
                    const descuento = parseFloat(item.descuento) || 0;
                    const totalfila = (preciounit * cantidad) - descuento;

                    totalNeto += totalfila;

                    const newDetId = db.pedidosventadetalle.length ? Math.max(...db.pedidosventadetalle.map(d => d.detalledid)) + 1 + index : 1 + index;
                    detItems.push({
                        detalledid: newDetId,
                        pedidoid: newPedidoId,
                        productoid: prod.productoid,
                        cantidad,
                        preciounitariocongiv: preciounit,
                        descuento,
                        totalfila
                    });
                }
            });

            const montoBruto = totalNeto / 1.18;

            const nuevoPedido = {
                pedidoid: newPedidoId,
                numeropedido: numeroPedido,
                clienteid,
                proyectoid,
                fechaemision: new Date().toISOString(),
                moneda: moneda || 'PEN',
                tipocambio: moneda === 'USD' ? 3.75 : 1.0000,
                metodopago: metodopago || 'efectivo',
                cupondescuento: null,
                montobruto: parseFloat(montoBruto.toFixed(4)),
                montodescuento: 0.00,
                totalneto: parseFloat(totalNeto.toFixed(4)),
                estado: 'pendiente'
            };

            db.pedidosventa.push(nuevoPedido);
            db.pedidosventadetalle.push(...detItems);

            saveDB(db);
            return { success: true, pedido: nuevoPedido };
        },

        // --- OPERACIONES DE COMPRAS ---
        aprobarOrdenCompra: function (ordenid) {
            const db = loadDB();
            ordenid = parseInt(ordenid);
            const orden = db.ordenescompra.find(o => o.ordenid === ordenid);
            if (orden && orden.estado === 'pendiente') {
                orden.estado = 'aprobado';
                
                // 1. Vincular gasto a proyecto (sp_operaciones_vincular_gasto_proyecto)
                if (orden.proyectoid) {
                    const proyecto = db.proyectos.find(p => p.proyectoid === orden.proyectoid);
                    if (proyecto) {
                        proyecto.costoreallogrado += parseFloat(orden.monto_total);
                    }
                }

                // 2. Aumentar stock físico de los productos comprados (Ingreso a Almacén Principal)
                const detalles = db.ordenescompradetalle.filter(d => d.ordenid === ordenid);
                detalles.forEach(det => {
                    const prod = db.productos.find(p => p.productoid === det.productoid);
                    if (prod && !prod.esservicio) {
                        // Registrar movimiento kardex
                        const stockRow = db.stockalmacen.find(s => s.almacenid === 1 && s.productoid === det.productoid);
                        if (stockRow) {
                            stockRow.stockactual += parseFloat(det.cantidad);
                        } else {
                            db.stockalmacen.push({
                                almacenid: 1,
                                productoid: det.productoid,
                                stockactual: parseFloat(det.cantidad),
                                stockcomprometido: 0
                            });
                        }

                        // Kardex
                        const newMovId = db.kardexmovimientos.length ? Math.max(...db.kardexmovimientos.map(k => parseInt(k.movimientoid))) + 1 : 1;
                        db.kardexmovimientos.push({
                            movimientoid: newMovId,
                            almacenid: 1,
                            productoid: det.productoid,
                            tipomovimiento: 'ent',
                            conceptomovimiento: 'compra',
                            documentoreferencia: orden.numeroorden,
                            cantidad: det.cantidad,
                            costounitariomovimiento: det.costounitariocongiv / 1.18,
                            fechamovimiento: new Date().toISOString()
                        });
                    }
                });

                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'Orden no encontrada o no pendiente.' };
        },

        rechazarOrdenCompra: function (ordenid) {
            const db = loadDB();
            ordenid = parseInt(ordenid);
            const orden = db.ordenescompra.find(o => o.ordenid === ordenid);
            if (orden && orden.estado === 'pendiente') {
                orden.estado = 'rechazado';
                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'La orden debe estar pendiente para rechazarse.' };
        },

        registrarOrdenCompra: function (proveedorid, proyectoid, categoriagasto, solicitante, items) {
            const db = loadDB();
            proveedorid = parseInt(proveedorid);
            proyectoid = proyectoid ? parseInt(proyectoid) : null;

            const newOrdenId = db.ordenescompra.length ? Math.max(...db.ordenescompra.map(o => o.ordenid)) + 1 : 1;
            const numeroOrden = `OC-2026-${String(newOrdenId).padStart(3, '0')}`;

            let totalNeto = 0;
            const detItems = [];

            items.forEach((item, index) => {
                const prod = db.productos.find(p => p.productoid === parseInt(item.productoid));
                if (prod) {
                    const costounit = parseFloat(item.costounitariocongiv) || prod.costopromedio;
                    const cantidad = parseFloat(item.cantidad) || 1;
                    const totalfila = costounit * cantidad;

                    totalNeto += totalfila;

                    const newDetId = db.ordenescompradetalle.length ? Math.max(...db.ordenescompradetalle.map(d => d.detalledoc)) + 1 + index : 1 + index;
                    detItems.push({
                        detalledoc: newDetId,
                        ordenid: newOrdenId,
                        productoid: prod.productoid,
                        cantidad,
                        costounitariocongiv: costounit,
                        totalfila
                    });
                }
            });

            const nuevaOrden = {
                ordenid: newOrdenId,
                numeroorden: numeroOrden,
                proveedorid,
                proyectoid,
                solicitante: solicitante || 'Admin',
                fechaemision: new Date().toISOString(),
                moneda: 'PEN',
                monto_total: parseFloat(totalNeto.toFixed(4)),
                categoriagasto: categoriagasto || 'materiales',
                estado: 'pendiente'
            };

            db.ordenescompra.push(nuevaOrden);
            db.ordenescompradetalle.push(...detItems);

            saveDB(db);
            return { success: true, orden: nuevaOrden };
        },

        // --- PROYECTOS Y TAREAS ---
        crearProyecto: function (nombre, clienteid, descripcion, presupuestototal, fechainicio, fechafin) {
            const db = loadDB();
            clienteid = parseInt(clienteid);
            presupuestototal = parseFloat(presupuestototal) || 0;

            const newProjId = db.proyectos.length ? Math.max(...db.proyectos.map(p => p.proyectoid)) + 1 : 1;
            const nuevoProyecto = {
                proyectoid: newProjId,
                clienteid,
                nombreproyecto: nombre,
                descripcion: descripcion || '',
                presupuestototal,
                costoreallogrado: 0.00,
                fechainicio,
                fechafin: fechafin || null,
                estado: 'planificado',
                fecharegistro: new Date().toISOString()
            };

            db.proyectos.push(nuevoProyecto);
            saveDB(db);
            return { success: true, proyecto: nuevoProyecto };
        },

        crearTarea: function (proyectoid, nombre, fechainicio, fechafin, costoestimado) {
            const db = loadDB();
            proyectoid = parseInt(proyectoid);
            costoestimado = parseFloat(costoestimado) || 0;

            const newTareaId = db.proyectotareas.length ? Math.max(...db.proyectotareas.map(t => t.tareaid)) + 1 : 1;
            const nuevaTarea = {
                tareaid: newTareaId,
                proyectoid,
                nombretarea: nombre,
                fechainicio,
                fechafin,
                porcentajeprogreso: 0.00,
                costoestimado,
                estado: 'pendiente'
            };

            db.proyectotareas.push(nuevaTarea);
            saveDB(db);
            return { success: true, tarea: nuevaTarea };
        },

        actualizarProgresoTarea: function (tareaid, progreso, estado) {
            const db = loadDB();
            tareaid = parseInt(tareaid);
            progreso = parseFloat(progreso);
            
            const tarea = db.proyectotareas.find(t => t.tareaid === tareaid);
            if (tarea) {
                tarea.porcentajeprogreso = progreso;
                tarea.estado = estado;
                
                // Recalcular estado del proyecto si es necesario
                const proyecto = db.proyectos.find(p => p.proyectoid === tarea.proyectoid);
                if (proyecto) {
                    const tareas = db.proyectotareas.filter(t => t.proyectoid === tarea.proyectoid);
                    const todasCompletadas = tareas.every(t => t.estado === 'completada');
                    const algunaEnProgreso = tareas.some(t => t.estado === 'en ejecucion' || t.porcentajeprogreso > 0);
                    
                    if (todasCompletadas) {
                        proyecto.estado = 'terminado';
                    } else if (algunaEnProgreso) {
                        proyecto.estado = 'en progreso';
                    }
                }

                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'Tarea no encontrada' };
        },

        // --- FACTURACIÓN Y SUNAT ---
        generarFacturaDesdePedido: function (pedidoid, tipocomprobante, serie) {
            const db = loadDB();
            pedidoid = parseInt(pedidoid);
            const pedido = db.pedidosventa.find(p => p.pedidoid === pedidoid);
            if (pedido && (pedido.estado === 'despachado' || pedido.estado === 'aprobado')) {
                // Verificar si ya tiene factura
                const yaFacturado = db.comprobantesfacturacion.some(c => c.pedidoid === pedidoid);
                if (yaFacturado) {
                    throw new Error('Este pedido ya posee un comprobante de facturación emitido.');
                }

                const newCompId = db.comprobantesfacturacion.length ? Math.max(...db.comprobantesfacturacion.map(c => c.comprobanteid)) + 1 : 1;
                
                const serieGenerada = serie || (tipocomprobante === '01' ? 'F001' : 'B001');
                const correlativoGenerado = String(newCompId).padStart(8, '0');

                const total = pedido.totalneto;
                const igv = total - (total / 1.18);
                const gravado = total / 1.18;

                const nuevoComprobante = {
                    comprobanteid: newCompId,
                    pedidoid,
                    tipocomprobante: tipocomprobante || '01',
                    serie: serieGenerada,
                    correlativo: correlativoGenerated = correlativoGenerado,
                    fechaemision: new Date().toISOString(),
                    tipooperacionsunat: '01', // Venta interna
                    clienteid: pedido.clienteid,
                    moneda: pedido.moneda,
                    opgravada: parseFloat(gravado.toFixed(4)),
                    opinafecta: 0.00,
                    opexonerada: 0.00,
                    igv_total: parseFloat(igv.toFixed(4)),
                    importetotalneto: parseFloat(total.toFixed(4)),
                    tipoimpuestoespecial: 'ninguno',
                    estadosunat: 'pendiente' // Listo para enviar a SUNAT
                };

                db.comprobantesfacturacion.push(nuevoComprobante);
                saveDB(db);
                return { success: true, comprobante: nuevoComprobante };
            }
            return { success: false, message: 'El pedido debe estar despachado o aprobado para facturar.' };
        },

        enviarComprobanteSunat: function (comprobanteid) {
            const db = loadDB();
            comprobanteid = parseInt(comprobanteid);
            const comprobante = db.comprobantesfacturacion.find(c => c.comprobanteid === comprobanteid);
            if (comprobante) {
                comprobante.estadosunat = 'aceptado';
                saveDB(db);
                return { success: true };
            }
            return { success: false, message: 'Comprobante no encontrado' };
        }
    };

    // Exponer la Base de Datos simulada de forma global en window
    window.SGE_Db = SGE_Db;
})();
