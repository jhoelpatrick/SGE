-- ==========================================================================================
-- módulo exclusivo: comercial (maestros, crm y logística de proveedores)
-- base de datos: sge_crm
-- optimizado para entornos windows (todo en lowercase)
-- ==========================================================================================

if exists (select name from sys.databases where name = N'sge_crm')
begin
    alter database sge_crm set single_user with rollback immediate;
    drop database sge_crm;
end
go

create database sge_crm;
go

use sge_crm;
go

alter database sge_crm set read_committed_snapshot on;
go

create schema comercial;
go

-- ==========================================================================================
-- creación de tablas (8 tablas del módulo comercial)
-- ==========================================================================================

create table comercial.ubigeos (
    codigoubigeo char(6) not null constraint pk_comercial_ubigeos primary key,
    departamento varchar(100) not null,
    provincia varchar(100) not null,
    distrito varchar(100) not null
);

create table comercial.clientes (
    clienteid int identity(1,1) constraint pk_comercial_clientes primary key,
    tipodocumento char(1) not null, 
    numerodocumento varchar(15) not null,
    razonsocial varchar(250) not null, 
    nombrecomercial varchar(250) null,
    direccionfiscal varchar(500) null,
    ubigeo char(6) null constraint fk_clientes_ubigeos references comercial.ubigeos(codigoubigeo),              
    email varchar(150) null,
    telefono varchar(50) null,
    tipocliente varchar(20) default 'prospecto', 
    fecharegistro datetime default getdate(),
    estado bit default 1,
    constraint uq_clientes_documento unique (tipodocumento, numerodocumento)
);

create table comercial.contactosclientes (
    contactoclienteid int identity(1,1) constraint pk_comercial_contactosclientes primary key,
    clienteid int not null constraint fk_contactosclientes_clientes references comercial.clientes(clienteid) on delete cascade,
    nombre varchar(150) not null,
    cargo varchar(100) null,
    telefono varchar(50) null,
    email varchar(150) null,
    estado bit default 1
);

create table comercial.proveedores (
    proveedorid int identity(1,1) constraint pk_comercial_proveedores primary key,
    tipodocumento char(1) not null, 
    numerodocumento varchar(15) not null,
    razonsocial varchar(250) not null,
    direccionfiscal varchar(500) null,
    ubigeo char(6) null constraint fk_proveedores_ubigeos references comercial.ubigeos(codigoubigeo),
    telefono varchar(50) null,
    email varchar(150) null,
    estado bit default 1,
    constraint uq_proveedores_documento unique (tipodocumento, numerodocumento)
);

create table comercial.contactosproveedores (
    contactoproveedorid int identity(1,1) constraint pk_comercial_contactosproveedores primary key,
    proveedorid int not null constraint fk_contactosproveedores_proveedores references comercial.proveedores(proveedorid) on delete cascade,
    nombre varchar(150) not null,
    cargo varchar(100) null,
    telefono varchar(50) null,
    email varchar(150) null,
    estado bit default 1
);

create table comercial.vehiculosproveedores (
    vehiculoid int identity(1,1) constraint pk_comercial_vehiculosproveedores primary key,
    proveedorid int not null constraint fk_vehiculosproveedores_proveedores references comercial.proveedores(proveedorid) on delete cascade,
    placa varchar(10) not null constraint uq_vehiculos_placa unique,
    marca varchar(50) null,
    modelo varchar(50) null,
    tipovehiculo varchar(30) null, 
    estado bit default 1
);

create table comercial.conductoresproveedores (
    conductorid int identity(1,1) constraint pk_comercial_conductoresproveedores primary key,
    proveedorid int not null constraint fk_conductoresproveedores_proveedores references comercial.proveedores(proveedorid) on delete cascade,
    nombre varchar(150) not null,
    tipodocumento char(1) not null, 
    numerodocumento varchar(15) not null,
    licenciaconducir varchar(30) null,
    estado bit default 1
);

create table comercial.productos (
    productoid int identity(1,1) constraint pk_comercial_productos primary key,
    codigosku varchar(50) not null constraint uq_productos_sku unique,
    codigosunat char(8) null,          
    descripcion varchar(250) not null,
    unidadmedida char(3) not null,     
    tipoafectacionigv char(2) not null default '10', 
    precioventasugerido decimal(18,4) not null default 0.0000,
    costopromedio decimal(18,4) not null default 0.0000,
    esservicio bit default 0,          
    sevende bit default 1,             
    nosevende bit default 0,           
    sefabrica bit default 0,           
    estado bit default 1
);
go

-- ==========================================================================================
-- índices de rendimiento comercial
-- ==========================================================================================
create nonclustered index ix_clientes_busqueda on comercial.clientes (numerodocumento, tipodocumento) include (razonsocial);
create nonclustered index ix_vehiculos_placabusqueda on comercial.vehiculosproveedores (placa) include (proveedorid, estado);
go

-- ==========================================================================================
-- vistas optimizadas para el módulo comercial
-- ==========================================================================================

-- vista crm: bandeja limpia de clientes con dirección formateada
create or alter view comercial.vw_crm_clientes_bandeja as
select 
    c.clienteid,
    c.tipodocumento,
    case c.tipodocumento when '1' then 'dni' when '6' then 'ruc' else 'otros' end as tipodocumentodesc,
    c.numerodocumento,
    c.razonsocial,
    c.nombrecomercial,
    c.email,
    c.telefono,
    c.tipocliente,
    c.estado,
    concat(c.direccionfiscal, ' - ', u.distrito, ', ', u.provincia, ' (', u.departamento, ')') as direccioncompletaui
from comercial.clientes c
left join comercial.ubigeos u on c.ubigeo = u.codigoubigeo;
go

-- vista logística: consulta rápida de accesos en garita/puerta para camiones proveedores
create or alter view comercial.vw_logistica_checkinflotas as
select 
    v.placa,
    v.marca,
    v.modelo,
    v.tipovehiculo,
    p.razonsocial as proveedornombre,
    p.numerodocumento as proveedorruc,
    v.estado as vehiculoactivo,
    cond.nombre as conductornombre,
    cond.licenciaconducir
from comercial.vehiculosproveedores v
inner join comercial.proveedores p on v.proveedorid = p.proveedorid
left join comercial.conductoresproveedores cond on p.proveedorid = cond.proveedorid and cond.estado = 1;
go

-- ==========================================================================================
-- módulo complementario: operaciones unificadas
-- base de datos: sge_crm (ejecutar sobre la misma base de datos)
-- optimizado para entornos windows (todo en lowercase)
-- ==========================================================================================

create schema operaciones;
go

-- ==========================================================================================
-- 1. sub-módulo: proyectos e hitos
-- ==========================================================================================

create table operaciones.proyectos (
    proyectoid int identity(1,1) constraint pk_operaciones_proyectos primary key,
    clienteid int not null constraint fk_proyectos_clientes references comercial.clientes(clienteid),
    nombreproyecto varchar(250) not null,
    descripcion varchar(max) null,
    presupuestototal decimal(18,4) default 0.0000,
    costoreallogrado decimal(18,4) default 0.0000,
    fechainicio date not null,
    fechafin date null,
    estado varchar(30) default 'planificado', -- planificado, en progreso, suspendido, terminado
    fecharegistro datetime default getdate()
);

create table operaciones.proyectotareas (
    tareaid int identity(1,1) constraint pk_operaciones_proyectotareas primary key,
    proyectoid int not null constraint fk_proyectotareas_proyectos references operaciones.proyectos(proyectoid) on delete cascade,
    nombretarea varchar(250) not null,
    fechainicio date not null,
    fechafin date not null,
    porcentajeprogreso decimal(5,2) default 0.00, -- de 0.00 a 100.00
    costoestimado decimal(18,4) default 0.0000,
    estado varchar(30) default 'pendiente' -- pendiente, en ejecucion, completada, bloqueada
);
go

-- ==========================================================================================
-- 2. sub-módulo: inventarios y almacenes (multi-almacén)
-- ==========================================================================================

create table operaciones.almacenes (
    almacenid int identity(1,1) constraint pk_operaciones_almacenes primary key,
    codigoalmacen varchar(10) not null constraint uq_almacen_codigo unique,
    nombre varchar(150) not null,
    direccion varchar(500) null,
    ubigeo char(6) null constraint fk_almacenes_ubigeos references comercial.ubigeos(codigoubigeo),
    estado bit default 1
);

create table operaciones.stockalmacen (
    almacenid int not null constraint fk_stock_almacenes references operaciones.almacenes(almacenid),
    productoid int not null constraint fk_stock_productos references comercial.productos(productoid),
    stockactual decimal(18,4) default 0.0000,
    stockcomprometido decimal(18,4) default 0.0000, -- pedidos aprobados pero no despachados
    constraint pk_operaciones_stockalmacen primary key (almacenid, productoid)
);
go

-- ==========================================================================================
-- 3. sub-módulo: transacciones comerciales (ventas y compras)
-- ==========================================================================================

create table operaciones.pedidosventa (
    pedidoid int identity(1,1) constraint pk_operaciones_pedidosventa primary key,
    numeropedido varchar(20) not null constraint uq_pedido_numero unique, -- ped-2024-001
    clienteid int not null constraint fk_pedidos_clientes references comercial.clientes(clienteid),
    proyectoid int null constraint fk_pedidos_proyectos references operaciones.proyectos(proyectoid),
    fechaemision datetime default getdate(),
    moneda char(3) default 'pen', -- pen, usd, eur
    tipocambio decimal(18,4) default 1.0000,
    metodopago varchar(30) null, -- visa, mastercard, paypal, credito
    cupondescuento varchar(20) null,
    montobruto decimal(18,4) default 0.0000,
    montodescuento decimal(18,4) default 0.0000,
    totalneto decimal(18,4) default 0.0000,
    estado varchar(30) default 'pendiente' -- pendiente, aprobado, despachado, cancelado
);

create table operaciones.pedidosventadetalle (
    detalledid int identity(1,1) constraint pk_operaciones_pedidosventadetalle primary key,
    pedidoid int not null constraint fk_pedidodetalle_pedidos references operaciones.pedidosventa(pedidoid) on delete cascade,
    productoid int not null constraint fk_pedidodetalle_productos references comercial.productos(productoid),
    cantidad decimal(18,4) not null,
    preciounitariocongiv decimal(18,4) not null,
    descuento decimal(18,4) default 0.0000,
    totalfila decimal(18,4) not null
);

create table operaciones.ordenescompra (
    ordenid int identity(1,1) constraint pk_operaciones_ordenescompra primary key,
    numeroorden varchar(20) not null constraint uq_orden_numero unique, -- oc-2024-001
    proveedorid int not null constraint fk_ordenes_proveedores references comercial.proveedores(proveedorid),
    proyectoid int null constraint fk_ordenes_proyectos references operaciones.proyectos(proyectoid),
    solicitante varchar(150) null,
    fechaemision datetime default getdate(),
    moneda char(3) default 'pen',
    monto_total decimal(18,4) default 0.0000,
    categoriagasto varchar(50) default 'materiales', -- materiales, servicios, equipos, logistica
    estado varchar(30) default 'pendiente' -- pendiente, aprobado, bloqueado, rechazado
);

create table operaciones.ordenescompradetalle (
    detalledoc int identity(1,1) constraint pk_operaciones_ordenescompradetalle primary key,
    ordenid int not null constraint fk_ordendetalle_ordenes references operaciones.ordenescompra(ordenid) on delete cascade,
    productoid int not null constraint fk_ordendetalle_productos references comercial.productos(productoid),
    cantidad decimal(18,4) not null,
    costounitariocongiv decimal(18,4) not null,
    totalfila decimal(18,4) not null
);
go

-- ==========================================================================================
-- 4. sub-módulo: facturación electrónica y guías sunat
-- ==========================================================================================

create table operaciones.comprobantesfacturacion (
    comprobanteid int identity(1,1) constraint pk_operaciones_comprobantes primary key,
    pedidoid int null constraint fk_comprobantes_pedidos references operaciones.pedidosventa(pedidoid),
    tipocomprobante char(2) not null, -- '01' factura, '03' boleta
    serie char(4) not null, -- f001, b001
    correlativo varchar(8) not null, -- 00000125
    fechaemision datetime default getdate(),
    tipooperacionsunat char(2) default '01', -- '01' venta interna
    clienteid int not null constraint fk_comprobantes_clientes references comercial.clientes(clienteid),
    moneda char(3) default 'pen',
    opgravada decimal(18,4) default 0.0000,
    opinafecta decimal(18,4) default 0.0000,
    opexonerada decimal(18,4) default 0.0000,
    igv_total decimal(18,4) default 0.0000,
    importetotalneto decimal(18,4) default 0.0000,
    tipoimpuestoespecial varchar(30) default 'ninguno', -- detraccion, retencion
    estadosunat varchar(30) default 'enviado sunat', -- enviado sunat, contingencia, anulado
    constraint uq_comprobante_numeracion unique (tipocomprobante, serie, correlativo)
);

create table operaciones.guiasremision (
    guiaid int identity(1,1) constraint pk_operaciones_guias primary key,
    serie char(4) not null, -- t001
    correlativo varchar(8) not null,
    fechaemision datetime default getdate(),
    motivotraslado char(2) not null, -- '01' venta, '02' compra, '04' traslado entre almacenes
    almacenorigenid int not null constraint fk_guias_almacenorigen references operaciones.almacenes(almacenid),
    almacendestinoid int null constraint fk_guias_almacendestino references operaciones.almacenes(almacenid),
    proveedorid int null constraint fk_guias_proveedores references comercial.proveedores(proveedorid), -- si es traslado por compra
    vehiculoid int null constraint fk_guias_vehiculos references comercial.vehiculosproveedores(vehiculoid),
    conductorid int null constraint fk_guias_conductores references comercial.conductoresproveedores(conductorid),
    pesototal decimal(12,2) not null,
    unidadmedidapeso char(3) default 'kgm',
    estadosunat varchar(30) default 'aceptado',
    constraint uq_guia_numeracion unique (serie, correlativo)
);
go

-- ==========================================================================================
-- 5. sub-módulo: motor de movimientos físicos (kardex)
-- ==========================================================================================

create table operaciones.kardexmovimientos (
    movimientoid bigint identity(1,1) constraint pk_operaciones_kardex primary key,
    almacenid int not null constraint fk_kardex_almacenes references operaciones.almacenes(almacenid),
    productoid int not null constraint fk_kardex_productos references comercial.productos(productoid),
    tipomovimiento char(3) not null, -- 'ent' (entrada), 'sal' (salida)
    conceptomovimiento varchar(100) not null, -- venta, compra, ajuste, traslado
    documentoreferencia varchar(50) null, -- ped-2024-004, oc-2024-001, t001-000021
    cantidad decimal(18,4) not null,
    costounitariomovimiento decimal(18,4) not null,
    fechamovimiento datetime default getdate()
);
go

-- ==========================================================================================
-- índices de rendimiento para operaciones rápidas
-- ==========================================================================================
create nonclustered index ix_pedidos_busqueda on operaciones.pedidosventa (numeropedido) include (estado, totalneto);
create nonclustered index ix_kardex_reporte on operaciones.kardexmovimientos (productoid, almacenid, fechamovimiento);
go

-- ==========================================================================================
-- 1. vistas (views) - para alimentar las pantallas y dashboards de la ui
-- ==========================================================================================

-- vista para la bandeja principal de proyectos (junta cliente de comercial + avance)
create or alter view operaciones.vw_operaciones_dashboard_proyectos as
select 
    p.proyectoid,
    p.nombreproyecto,
    c.razonsocial as clientenombre,
    c.numerodocumento as clienteruc,
    p.presupuestototal,
    p.costoreallogrado,
    (p.presupuestototal - p.costoreallogrado) as desviacionfinanciera,
    p.fechainicio,
    p.fechafin,
    p.estado as estadoproyecto,
    isnull(avg(t.porcentajeprogreso), 0.00) as progresopromedio,
    count(t.tareaid) as totaltareas
from operaciones.proyectos p
inner join comercial.clientes c on p.clienteid = c.clienteid
left join operaciones.proyectotareas t on p.proyectoid = t.proyectoid
group by 
    p.proyectoid, p.nombreproyecto, c.razonsocial, c.numerodocumento,
    p.presupuestototal, p.costoreallogrado, p.fechainicio, p.fechafin, p.estado;
go

-- vista para la bandeja de facturación electrónica (junta comprobante + datos de sunat)
create or alter view operaciones.vw_operaciones_bandeja_facturacion as
select 
    cf.comprobanteid,
    cf.tipocomprobante,
    cf.serie,
    cf.correlativo,
    (cf.serie + '-' + cf.correlativo) as numerocomprobante,
    cf.fechaemision,
    c.numerodocumento as clientedocumento,
    c.razonsocial as clientenombre,
    cf.moneda,
    cf.opgravada,
    cf.igv_total,
    cf.importetotalneto,
    cf.estadosunat
from operaciones.comprobantesfacturacion cf
inner join comercial.clientes c on cf.clienteid = c.clienteid;
go


-- ==========================================================================================
-- 2. procedimientos almacenados (procedures) - lógica transaccional atómica
-- ==========================================================================================

-- procedure 1: motor de movimientos de almacén (kardex + actualización de stock)
-- este procedimiento asegura que si ingresa o sale stock, impacte en el kardex y en el stock real al mismo tiempo
create or alter procedure operaciones.sp_operaciones_registrar_movimiento_kardex
    @p_almacenid int,
    @p_productoid int,
    @p_tipomovimiento char(3), -- 'ent' (entrada) o 'sal' (salida)
    @p_conceptomovimiento varchar(100), -- compra, venta, traslado, ajuste
    @p_documentoreferencia varchar(50), -- oc-2024-001, ped-2024-002
    @p_cantidad decimal(18,4),
    @p_costounitariomovimiento decimal(18,4)
as
begin
    set nocount on;
    
    begin try
        begin transaction;

        -- 1. insertar el registro histórico e inmutable en el kardex
        insert into operaciones.kardexmovimientos (
            almacenid, productoid, tipomovimiento, conceptomovimiento, 
            documentoreferencia, cantidad, costounitariomovimiento, fechamovimiento
        )
        values (
            @p_almacenid, @p_productoid, @p_tipomovimiento, @p_conceptomovimiento, 
            @p_documentoreferencia, @p_cantidad, @p_costounitariomovimiento, getdate()
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
go

-- procedure 2: vinculación automática de gastos de compras al costo real del proyecto
-- cuando se aprueba una orden de compra de un proyecto, este sp acumula automáticamente el costo al total del proyecto
create or alter procedure operaciones.sp_operaciones_vincular_gasto_proyecto
    @p_ordenid int
as
begin
    set nocount on;
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
go

-- ==========================================================================================
-- módulo final: finanzas, contabilidad y tesorería
-- base de datos: sge_crm (ejecutar sobre la misma base de datos)
-- optimizado para entornos windows (todo en lowercase)
-- ==========================================================================================

create schema finanzas;
go

-- ==========================================================================================
-- 1. sub-módulo: impuestos y tasas sunat
-- ==========================================================================================

create table finanzas.impuestos (
    impuestoid int identity(1,1) constraint pk_finanzas_impuestos primary key,
    codigoimpuestosunat char(4) not null constraint uq_impuesto_codigo unique, -- 1000 (igv), 9997 (exonerado)
    nombreimpuesto varchar(50) not null, -- igv, isc, ivap
    porcentaje decimal(5,2) not null default 18.00,
    estado bit default 1
);

-- ==========================================================================================
-- 2. sub-módulo: contabilidad (plan contable general empresarial y asientos)
-- ==========================================================================================

create table finanzas.plancuentas (
    cuentacodigo varchar(15) not null constraint pk_finanzas_plancuentas primary key, -- 12121 (facturas por cobrar)
    descripcion varchar(250) not null,
    tipocuenta varchar(30) not null, -- activo, pasivo, patrimonio, ingresos, gastos
    nivelint int not null default 5, -- cuenta de balance, registro o divisionaria
    aceptaasiento bit default 1
);

create table finanzas.asientoscabecera (
    asientoid bigint identity(1,1) constraint pk_finanzas_asientos primary key,
    numeroasiento varchar(20) not null constraint uq_asiento_numero unique, -- as-2024-00001
    fechaasiento date not null,
    tipolibrosunat char(2) not null, -- '01' caja y bancos, '08' compras, '14' ventas
    glosa varchar(500) not null, -- por la provision de la venta de mercaderia
    documentoreferencia varchar(50) null, -- f001-000023
    fecharegistro datetime default getdate()
);

create table finanzas.asientosdetalle (
    asientodetalleid bigint identity(1,1) constraint pk_finanzas_asientosdetalle primary key,
    asientoid bigint not null constraint fk_asientosdetalle_cabecera references finanzas.asientoscabecera(asientoid) on delete cascade,
    cuentacodigo varchar(15) not null constraint fk_asientosdetalle_plan references finanzas.plancuentas(cuentacodigo),
    debe decimal(18,4) default 0.0000,
    haber decimal(18,4) default 0.0000,
    constraint ck_partida_doble_valores check (debe >= 0 and haber >= 0)
);
go

-- ==========================================================================================
-- 3. sub-módulo: caja y bancos (tesorería)
-- ==========================================================================================

create table finanzas.cuentasbancarias (
    cuentabancariaid int identity(1,1) constraint pk_finanzas_cuentasbancarias primary key,
    banconombre varchar(100) not null, -- bcp, bbva, interbank
    numerocuenta varchar(50) not null constraint uq_cuenta_numero unique,
    cuentacciexterno varchar(50) null,
    tipocuenta varchar(30) default 'corriente', -- corriente, ahorros, caja chica
    moneda char(3) default 'pen',
    saldoactual decimal(18,4) default 0.0000,
    estado bit default 1
);

create table finanzas.movimientostesoreria (
    movimientotesoreriaid bigint identity(1,1) constraint pk_finanzas_tesoreria primary key,
    cuentabancariaid int not null constraint fk_tesoreria_cuentas references finanzas.cuentasbancarias(cuentabancariaid),
    tipoflujo char(3) not null, -- 'ing' (ingreso), 'egr' (egreso)
    mediopagosunat char(3) not null, -- '001' deposito, '003' transferencia, '008' tarjeta
    monto decimal(18,4) not null,
    comprobanteid int null constraint fk_tesoreria_comprobantes references operaciones.comprobantesfacturacion(comprobanteid), -- si es cobro de venta
    ordenid int null constraint fk_tesoreria_ordenes references operaciones.ordenescompra(ordenid), -- si es pago de compra
    glosamovimiento varchar(250) null,
    fechamovimiento datetime default getdate()
);
go

-- ==========================================================================================
-- 4. sub-módulo: activos fijos y depreciación
-- ==========================================================================================

create table finanzas.activosfijos (
    activoid int identity(1,1) constraint pk_finanzas_activos primary key,
    codigoactivo varchar(50) not null constraint uq_activo_codigo unique,
    descripcion varchar(250) not null,
    productoid int null constraint fk_activos_productos references comercial.productos(productoid), -- por si vino de una compra directa
    fechadquisicion date not null,
    valorinicial decimal(18,4) not null,
    tasadepreciacionanual decimal(5,2) not null, -- ej. 20.00 para computo (5 años)
    depreciacionacumulada decimal(18,4) default 0.0000,
    valornetolibros as (valorinicial - depreciacionacumulada),
    estado varchar(30) default 'activo' -- activo, retirado, vendido, depreciado por completo
);
go

-- ==========================================================================================
-- índices de rendimiento contable
-- ==========================================================================================
create nonclustered index ix_asientos_busquedacuenta on finanzas.asientosdetalle (cuentacodigo) include (debe, haber);
create nonclustered index ix_tesoreria_flujocaja on finanzas.movimientostesoreria (cuentabancariaid, tipoflujo) include (monto, fechamovimiento);
go

-- ==========================================================================================
-- módulo complementario: vistas y procedimientos almacenados de finanzas
-- base de datos: sge_crm
-- entorno: windows (todo en lowercase)
-- ==========================================================================================

-- ==========================================================================================
-- 1. vistas (views) - para dashboards financieros y reportes contables
-- ==========================================================================================

-- vista 1: libro diario general detallado (cruza asientos con el plan de cuentas)
create or alter view finanzas.vw_finanzas_libro_diario as
select 
    ac.asientoid,
    ac.numeroasiento,
    ac.fechaasiento,
    ac.tipolibrosunat,
    ac.glosa,
    ac.documentoreferencia,
    ad.cuentacodigo,
    pc.descripcion as nombrecuenta,
    ad.debe,
    ad.haber
from finanzas.asientoscabecera ac
inner join finanzas.asientosdetalle ad on ac.asientoid = ad.asientoid
inner join finanzas.plancuentas pc on ad.cuentacodigo = pc.cuentacodigo;
go

-- vista 2: resumen de saldos y flujo de caja por cuenta bancaria
create or alter view finanzas.vw_finanzas_resumen_bancos as
select 
    cb.cuentabancariaid,
    cb.banconombre,
    cb.numerocuenta,
    cb.moneda,
    cb.saldoactual,
    isnull(sum(case when mt.tipoflujo = 'ing' then mt.monto else 0 end), 0.0000) as totalingresos,
    isnull(sum(case when mt.tipoflujo = 'egr' then mt.monto else 0 end), 0.0000) as totalegresos
from finanzas.cuentasbancarias cb
left join finanzas.movimientostesoreria mt on cb.cuentabancariaid = mt.cuentabancariaid
where cb.estado = 1
group by cb.cuentabancariaid, cb.banconombre, cb.numerocuenta, cb.moneda, cb.saldoactual;
go


-- ==========================================================================================
-- 2. procedimientos almacenados (procedures) - automatización contable
-- ==========================================================================================

-- procedure 1: procesar cobro de factura (actualiza caja/bancos y genera asiento de pago)
create or alter procedure finanzas.sp_finanzas_procesar_cobro_factura
    @p_cuentabancariaid int,
    @p_comprobanteid int,
    @p_monto decimal(18,4),
    @p_mediopago char(3), -- '003' transferencia, '001' depósito, etc.
    @p_glosa varchar(250)
as
begin
    set nocount on;
    
    declare @v_moneda char(3);
    declare @v_numerodoc varchar(50);
    declare @v_clienteid int;
    declare @v_asientoid bigint;
    declare @v_numero_asiento_generado varchar(20);

    -- 1. obtener datos del comprobante a cobrar
    select 
        @v_moneda = moneda,
        @v_numerodoc = (serie + '-' + correlativo),
        @v_clienteid = clienteid
    from operaciones.comprobantesfacturacion
    where comprobanteid = @p_comprobanteid;

    if @v_numerodoc is null
    begin
        raiserror('error: el comprobante especificado no existe.', 16, 1);
        return;
    end

    begin try
        begin transaction;

        -- 2. registrar el movimiento físico de dinero en la cuenta bancaria
        insert into finanzas.movimientostesoreria (
            cuentabancariaid, tipoflujo, mediopagosunat, monto, comprobanteid, ordenid, glosamovimiento, fechamovimiento
        )
        values (
            @p_cuentabancariaid, 'ing', @p_mediopago, @p_monto, @p_comprobanteid, null, @p_glosa, getdate()
        );

        -- 3. actualizar el saldo disponible de la cuenta de banco
        update finanzas.cuentasbancarias
        set saldoactual = saldoactual + @p_monto
        where cuentabancariaid = @p_cuentabancariaid;

        -- 4. crear la cabecera del asiento contable (libro caja y bancos '01')
        set @v_numero_asiento_generado = 'AS-' + replace(convert(varchar(36), newid()), '-', ''); -- simplificado para el ejemplo
        
        insert into finanzas.asientoscabecera (numeroasiento, fechaasiento, tipolibrosunat, glosa, documentoreferencia)
        values (
            upper(@v_numero_asiento_generado), -- genera un código único temporal para el asiento
            cast(getdate() as date),
            '01', -- caja y bancos
            'por el cobro de la factura ' + @v_numerodoc,
            @v_numerodoc
        );
        
        set @v_asientoid = scope_identity();

        -- 5. partida doble automática (dinámica contable peruana)
        -- cuenta 1041 (bancos) ingresa al debe
        insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber)
        values (@v_asientoid, '1041', @p_monto, 0.0000);

        -- cuenta 1212 (facturas por cobrar) sale por el haber (se cancela la deuda)
        insert into finanzas.asientosdetalle (asientoid, cuentacodigo, debe, haber)
        values (@v_asientoid, '1212', 0.0000, @p_monto);

        -- 6. cambiar el estado del comprobante si se cobró por completo (opcional/simplificado)
        update operaciones.comprobantesfacturacion
        set estadosunat = 'aceptada_sunat' -- o pagada según tu flujo de estados
        where comprobanteid = @p_comprobanteid;

        commit transaction;
    end try
    begin catch
        if @@trancount > 0
            rollback transaction;

        declare @v_errormsg varchar(4000) = error_message();
        raiserror(@v_errormsg, 16, 1);
    end catch
end;
go

-- ==========================================================================================
-- módulo obligatorio: reportes fiscales y catálogos sunat (sire / ple)
-- base de datos: sge_crm
-- optimizado para entornos windows (todo en lowercase)
-- ==========================================================================================

create schema sunat;
go

-- ==========================================================================================
-- 1. tablas de catálogos oficiales sunat (anexos obligatorios)
-- ==========================================================================================

-- catálogo 02: tipo de documento de identidad (ruc, dni, pasaporte)
create table sunat.catalogo01_identidad (
    codigochar char(1) not null constraint pk_sunat_cat01 primary key, -- '1' dni, '6' ruc, '4' carnet extranjería
    descripcion varchar(100) not null,
    estado bit default 1
);

-- catálogo 02: tipo de comprobante de pago (factura, boleta, nota de crédito)
create table sunat.catalogo02_comprobantes (
    codigocharsunat char(2) not null constraint pk_sunat_cat02 primary key, -- '01' factura, '03' boleta, '07' nc, '08' nd
    descripcion varchar(150) not null,
    abreviatura varchar(10) null,
    estado bit default 1
);

-- catálogo 05: tipos de afectación del igv (indispensable para el xml ubl 2.1 y rvie)
create table sunat.catalogo05_afectacionigv (
    codigoafectacion char(2) not null constraint pk_sunat_cat05 primary key, -- '10' gravado, '20' exonerado, '30' inafecto
    descripcion varchar(150) not null,
    letra_tributo char(1) not null, -- 's' (iva/igv), 'e' (exonerado), 'o' (inafecto)
    codigo_tributo_sunat char(4) not null -- '1000' (igv), '9997' (exonerado), '9998' (inafecto)
);
go

-- ==========================================================================================
-- 2. sub-módulo: sire (sistema integrado de registros electrónicos - rvie / rce)
-- ==========================================================================================

-- tabla para controlar la propuesta, aceptación y reemplazo de los registros mensuales de compras y ventas
create table sunat.declaraciones_sire (
    sireid int identity(1,1) constraint pk_sunat_sire primary key,
    periodo char(6) not null, -- formato: '202605' (año 2026, mes mayo)
    tiporegistro varchar(4) not null, -- 'rvie' (ventas), 'rce' (compras)
    numeroticket varchar(50) null, -- id de ticket devuelto por el api de sunat al enviar la propuesta
    fechaenvio datetime default getdate(),
    estado_sire varchar(50) default 'propuesta_sunat', -- propuesta_sunat, aceptado, reemplazado
    nombre_archivo_exportado varchar(250) null -- nombre del zip/txt oficial generado
);

-- tabla de mapeo car (código de anotación de registro) exigido por sunat para amarrar compras y ventas
create table sunat.control_car_sire (
    carid bigint identity(1,1) constraint pk_sunat_car primary key,
    comprobanteid int null constraint fk_car_comprobantes references operaciones.comprobantesfacturacion(comprobanteid),
    ordenid int null constraint fk_car_ordenes references operaciones.ordenescompra(ordenid),
    codigo_car varchar(50) not null constraint uq_sire_car_unico unique, -- estructura compleja que exige sunat
    periodo_afectacion char(6) not null
);
go

-- ==========================================================================================
-- 3. sub-módulo: ple (programa de libros electrónicos - contabilidad y kardex)
-- ==========================================================================================

-- tabla para el control de cierres e indicadores de contenido de los libros contables txt
create table sunat.cierres_ple (
    pleid int identity(1,1) constraint pk_sunat_ple primary key,
    periodo char(8) not null, -- formato: '20260500' (año, mes, 00 sin día para libros de cierre)
    codigolibrosunat varchar(10) not null, -- '050100' (libro diario), '130100' (kardex valorizado)
    cantidad_filas int default 0,
    codigohash varchar(64) null, -- hash xbrl/md5 generado por el ple para validar que el archivo no fue alterado
    fecha_generacion datetime default getdate(),
    estado_envio char(1) default '1' -- '1' o '0' indicador de libro con información
);
go

-- ==========================================================================================
-- carga de datos maestros iniciales obligatorios de la sunat
-- ==========================================================================================
insert into sunat.catalogo01_identidad (codigochar, descripcion) values 
('0', 'doc.trib.no.dom.sin.ruc'), ('1', 'doc.nacional de identidad (dni)'), 
('4', 'carnet de extranjeria'), ('6', 'registro unico de contribuyentes (ruc)');

insert into sunat.catalogo02_comprobantes (codigocharsunat, descripcion, abreviatura) values 
('01', 'factura', 'ft'), ('03', 'boleta de venta', 'bv'), 
('07', 'nota de credito', 'nc'), ('08', 'nota de debito', 'nd'),
('09', 'guia de remision remitente', 'gr');

insert into sunat.catalogo05_afectacionigv (codigoafectacion, descripcion, letra_tributo, codigo_tributo_sunat) values 
('10', 'gravado - operacion onerosa', 's', '1000'),
('20', 'exonerado - operacion onerosa', 'e', '9997'),
('30', 'inafecto - operacion onerosa', 'o', '9998');
go

-- ==========================================================================================
-- módulo de usuarios: seguridad avanzada, mfa y control de sesiones
-- base de datos: sge_crm
-- diseño: 100% desacoplado (relaciones lógicas a nivel de aplicación / sin llaves foráneas)
-- entorno: todo en lowercase (esquemas, tablas, columnas, restricciones e índices)
-- ==========================================================================================

-- 1. aseguramos la existencia del esquema exclusivo de seguridad
if not exists (select * from sys.schemas where name = 'seguridad')
begin
    exec('create schema seguridad;');
end
go

-- ==========================================================================================
-- limpieza preventiva (al no haber llaves foráneas, el orden de borrado es libre y seguro)
-- ==========================================================================================
if object_id('seguridad.usuario_mfa',                 'u') is not null drop table seguridad.usuario_mfa;
if object_id('seguridad.usuario_historial_passwords', 'u') is not null drop table seguridad.usuario_historial_passwords;
if object_id('seguridad.usuario_intentos_login',     'u') is not null drop table seguridad.usuario_intentos_login;
if object_id('seguridad.usuario_sesiones',           'u') is not null drop table seguridad.usuario_sesiones;
if object_id('seguridad.usuario_tokens',             'u') is not null drop table seguridad.usuario_tokens;
go

-- ==========================================================================================
-- 1. tabla: tokens de refresco (gestión del ciclo de vida de tokens jwt de .net core 8)
-- ==========================================================================================
create table seguridad.usuario_tokens (
    tokenid bigint identity(1,1) constraint pk_seguridad_usuario_tokens primary key,
    usuarioid int not null, -- relación lógica con la tabla de usuarios
    token_refresco varchar(250) not null constraint uq_seguridad_usuario_tokens_token unique,
    jwt_id varchar(100) not null, -- identificador único del access token para mitigar ataques de replicación
    es_usado bit not null default 0,
    es_revocado bit not null default 0,
    fecha_creacion datetime2 not null default getdate(),
    fecha_expiracion datetime2 not null
);

-- ==========================================================================================
-- 2. tabla: sesiones activas (soporte directo para la ui de monitoreo de dispositivos)
-- ==========================================================================================
create table seguridad.usuario_sesiones (
    sesionid bigint identity(1,1) constraint pk_seguridad_usuario_sesiones primary key,
    usuarioid int not null, -- relación lógica con la tabla de usuarios
    tokenid bigint null, -- relación lógica opcional con el token que originó la sesión
    ip_direccion varchar(45) not null, -- longitud 45 para dar soporte nativo a ipv4 e ipv6
    navegador varchar(250) not null, -- cliente web (ej: 'chrome 125.0', 'firefox')
    dispositivo varchar(100) not null, -- entorno (ej: 'windows 11', 'android 14', 'iphone')
    fecha_inicio datetime2 not null default getdate(),
    ultima_actividad datetime2 not null default getdate(),
    es_activa bit not null default 1 -- control rápido para revocación remota de sesiones de la ui
);

-- ==========================================================================================
-- 3. tabla: intentos de login (seguridad perimetral contra ataques de fuerza bruta)
-- ==========================================================================================
create table seguridad.usuario_intentos_login (
    intentoid bigint identity(1,1) constraint pk_seguridad_usuario_intentos_login primary key,
    email_ingresado varchar(150) not null, -- almacena el texto escrito para detectar escaneos de cuentas
    ip_direccion varchar(45) not null, -- ip del cliente atacante o legítimo
    exito bit not null, -- 1 = ingreso correcto, 0 = fallo
    motivo_fallo varchar(100) null, -- ej: 'contrasena_incorrecta', 'mfa_expirado'
    fecha_hora datetime2 not null default getdate()
);

-- ==========================================================================================
-- 4. tabla: historial de contraseñas (políticas corporativas de no-repetición de claves)
-- ==========================================================================================
create table seguridad.usuario_historial_passwords (
    historialid bigint identity(1,1) constraint pk_seguridad_usuario_historial_passwords primary key,
    usuarioid int not null, -- relación lógica con la tabla de usuarios
    contrasena_hash varchar(250) not null, -- hash antiguo para validación previa en el backend
    fecha_cambio datetime2 not null default getdate()
);

-- ==========================================================================================
-- 5. tabla: doble factor de autenticación (mfa / totp para operaciones críticas)
-- ==========================================================================================
create table seguridad.usuario_mfa (
    mfaid int identity(1,1) constraint pk_seguridad_usuario_mfa primary key,
    usuarioid int not null constraint uq_seguridad_usuario_mfa_usuario unique, -- relación lógica unívoca
    proveedor varchar(20) not null default 'totp', -- 'totp' (google/microsoft authenticator), 'sms', 'email'
    secreto_mfa varchar(128) not null, -- llave semilla encriptada para validar los códigos de 6 dígitos
    es_activo bit not null default 0, -- interruptor general de obligatoriedad en el login
    codigos_respaldo varchar(250) null, -- hashes de códigos de un solo uso para contingencias
    fecha_configuracion datetime2 not null default getdate()
);
go

-- ==========================================================================================
-- índices no agrupados de alto rendimiento (optimizados en minúsculas)
-- ==========================================================================================

-- optimiza la validación constante de tokens jwt en cada petición http al api de .net core
create nonclustered index ix_seguridad_usuario_tokens_busqueda 
on seguridad.usuario_tokens (token_refresco) 
include (usuarioid, es_usado, es_revocado);

-- optimiza la carga de la ui para mostrar dispositivos activos por cada usuario
create nonclustered index ix_seguridad_usuario_sesiones_usuario_activa 
on seguridad.usuario_sesiones (usuarioid, es_activa);

-- permite al backend consultar velozmente si una ip debe ser bloqueada por registrar fallos recurrentes
create nonclustered index ix_seguridad_usuario_intentos_ip_recientes 
on seguridad.usuario_intentos_login (ip_direccion, fecha_hora) 
include (exito);

-- acelera la verificación de políticas de contraseñas anteriores cuando el usuario actualiza su perfil
create nonclustered index ix_seguridad_usuario_historial_antiguos 
on seguridad.usuario_historial_passwords (usuarioid, fecha_cambio);
go



-- =============================================================================
-- módulo de recursos humanos (gestión de personal y asistencia)
-- optimizado para sge_crm (entornos windows - todo en lowercase)
-- =============================================================================

create schema rrhh_recursos;
go

-- =============================================================================
-- 1. tablas maestras de configuración de recursos
-- =============================================================================

create table rrhh_recursos.centros_costos (
    centrocostoid int identity(1,1),
    codigo varchar(10) not null,
    nombre varchar(100) not null,
    descripcion varchar(250) null,
    responsable varchar(150) null, 
    estaactivo bit not null,
    constraint pk_centros_costos primary key (centrocostoid),
    constraint uq_centros_costos_codigo unique (codigo)
);

create table rrhh_recursos.feriados (
    feriadoid int identity(1,1),
    fecha date not null,
    descripcion varchar(150) not null,
    estaactivo bit not null,
    constraint pk_feriados primary key (feriadoid),
    constraint uq_feriados_fecha unique (fecha)
);

create table rrhh_recursos.usuarios_nomina (
    usuarionominaid int identity(1,1),
    usuario varchar(50) not null,
    nombrecompleto varchar(150) not null,
    rol varchar(50) not null, 
    correo varchar(100) not null,
    estaactivo bit not null,
    constraint pk_usuarios_nomina primary key (usuarionominaid),
    constraint uq_usuarios_nomina_usuario unique (usuario)
);
go

-- =============================================================================
-- 2. entidades core del personal
-- =============================================================================

create table rrhh_recursos.empleados (
    empleadoid int identity(1,1),
    tipodocumento varchar(5) not null, 
    numerodocumento varchar(15) not null,
    nombres varchar(70) not null,
    apellidopaterno varchar(70) not null,
    apellidomaterno varchar(70) not null,
    fechanacimiento date not null,
    sexo char(1) not null, 
    correopersonal varchar(100) null,
    correocorporativo varchar(100) null,
    telefonocelular varchar(20) null,
    centrocostoid int not null,
    estaactivo bit not null,
    constraint pk_empleados primary key (empleadoid),
    constraint uq_empleados_documento unique (tipodocumento, numerodocumento),
    constraint fk_empleados_centros_costos foreign key (centrocostoid) 
        references rrhh_recursos.centros_costos (centrocostoid),
    constraint chk_empleados_sexo check (sexo in ('m', 'f'))
);

create table rrhh_recursos.contratos (
    contratoid int identity(1,1),
    empleadoid int not null,
    tipocontrato varchar(50) not null, 
    fechainicio date not null,
    fechafin date null, 
    sueldobase decimal(12,2) not null,
    estaactivo bit not null,
    constraint pk_contratos primary key (contratoid),
    constraint fk_contratos_empleados foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint chk_contratos_fechas check (fechafin is null or fechafin >= fechainicio)
);
go

-- =============================================================================
-- 3. tablas maestras del t-registro (sunat) y localización
-- =============================================================================

create table rrhh_recursos.ubigeos (
    ubigeoid char(6) not null, 
    departamento varchar(50) not null,
    provincia varchar(50) not null,
    distrito varchar(50) not null,
    constraint pk_ubigeos primary key (ubigeoid)
);

create table rrhh_recursos.regimenes_laborales (
    regimenlaboralid int identity(1,1),
    codigosunat varchar(4) not null, 
    nombre varchar(100) not null,    
    estaactivo bit not null,
    constraint pk_regimenes_laborales primary key (regimenlaboralid),
    constraint uq_regimenes_laborales_codigo unique (codigosunat)
);

create table rrhh_recursos.administradoras_pensiones (
    afpid int identity(1,1),
    codigosunat varchar(4) not null, 
    nombre varchar(50) not null,
    tipo char(3) not null,           
    estaactivo bit not null,
    constraint pk_administradoras_pensiones primary key (afpid),
    constraint uq_administradoras_pensiones_codigo unique (codigosunat),
    constraint chk_administradoras_pensiones_tipo check (tipo in ('afp', 'onp'))
);
go

-- =============================================================================
-- 4. ampliación del legajo del empleado
-- =============================================================================

create table rrhh_recursos.datos_laborales_empleados (
    empleadoid int not null,
    regimenlaboralid int not null,
    afpid int not null,
    tipocomision varchar(15) not null, 
    cuspp varchar(20) null,            
    ubigeodomicilio char(6) not null,
    direccion varchar(250) not null,
    cuentasueldo varchar(30) null,
    bancosueldoid int null,            
    cuentacts varchar(30) null,
    bancoctsid int null,
    constraint pk_datos_laborales_empleados primary key (empleadoid),
    constraint fk_datos_laborales_emp foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint fk_datos_laborales_regimen foreign key (regimenlaboralid) 
        references rrhh_recursos.regimenes_laborales (regimenlaboralid),
    constraint fk_datos_laborales_afp foreign key (afpid) 
        references rrhh_recursos.administradoras_pensiones (afpid),
    constraint fk_datos_laborales_ubigeo foreign key (ubigeodomicilio) 
        references rrhh_recursos.ubigeos (ubigeoid),
    constraint chk_datos_laborales_comision check (tipocomision in ('flujo', 'mixta', 'no_aplica'))
);

create table rrhh_recursos.derechohabientes (
    derechohabienteid int identity(1,1),
    empleadoid int not null,
    vinculofamiliar varchar(20) not null, 
    tipodocumento varchar(5) not null,
    numerodocumento varchar(15) not null,
    nombres varchar(70) not null,
    apellidopaterno varchar(70) not null,
    apellidomaterno varchar(70) not null,
    fechanacimiento date not null,
    constraint pk_derechohabientes primary key (derechohabienteid),
    constraint fk_derechohabientes_empleados foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint uq_derechohabientes_doc unique (tipodocumento, numerodocumento)
);
go

-- =============================================================================
-- 5. control de asistencia, turnos y tareo biométrico
-- =============================================================================

create table rrhh_recursos.turnos (
    turnoid int identity(1,1),
    nombre varchar(50) not null, 
    horaingreso time not null,
    horasalida time not null,
    toleranciaingreso int not null, 
    tiemporefrigerio int not null,   
    estaactivo bit not null,
    constraint pk_turnos primary key (turnoid)
);

create table rrhh_recursos.marcaciones_biometricos (
    marcacionid bigint identity(1,1),
    empleadoid int not null,
    fechahora datetime not null,
    tipo varchar(15) not null, 
    dispositivo varchar(50) null,      
    constraint pk_marcaciones_biometricos primary key (marcacionid),
    constraint fk_marcaciones_biometricos_emp foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint chk_marcaciones_tipo check (tipo in ('ingreso', 'salida_ref', 'retorno_ref', 'salida'))
);

create table rrhh_recursos.asistencias_diarias (
    asistenciadiariaid bigint identity(1,1),
    empleadoid int not null,
    fecha date not null,
    turnoid int not null,
    horaingresoreal time null,
    horasalidareal time null,
    minutostardanza int not null,
    minutosextras25 int not null,
    minutosextras35 int not null,
    minutosnocturnas int not null,
    estadoasistencia varchar(20) not null, 
    constraint pk_asistencias_diarias primary key (asistenciadiariaid),
    constraint fk_asistencias_diarias_emp foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint fk_asistencias_diarias_turno foreign key (turnoid) 
        references rrhh_recursos.turnos (turnoid),
    constraint uq_empleado_fecha unique (empleadoid, fecha),
    constraint chk_asistencias_estado check (estadoasistencia in ('asistio', 'falta', 'feriado', 'licencia', 'vacaciones'))
);
go

-- =============================================================================
-- 6. vacaciones y licencias (gestión de ausencias)
-- =============================================================================

create table rrhh_recursos.tipos_licencias (
    tipolicenciaid int identity(1,1),
    codigosunat varchar(4) not null, 
    descripcion varchar(150) not null,
    congocehaber bit not null,
    essubsidiado bit not null,      
    estaactivo bit not null,
    constraint pk_tipos_licencias primary key (tipolicenciaid),
    constraint uq_tipos_licencias_codigo unique (codigosunat)
);

create table rrhh_recursos.solicitudes_licencias (
    solicitudlicenciaid int identity(1,1),
    empleadoid int not null,
    tipolicenciaid int not null,
    fechainicio date not null,
    fechafin date not null,
    estadosolicitud varchar(15) not null, 
    usuariosolicitaid int not null,       
    sustento varchar(500) null,          
    constraint pk_solicitudes_licencias primary key (solicitudlicenciaid),
    constraint fk_solicitudes_lic_emp foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint fk_solicitudes_lic_tipo foreign key (tipolicenciaid) 
        references rrhh_recursos.tipos_licencias (tipolicenciaid),
    constraint fk_solicitudes_lic_usr foreign key (usuariosolicitaid) 
        references rrhh_recursos.usuarios_nomina (usuarionominaid),
    constraint chk_solicitudes_lic_fechas check (fechafin >= fechainicio),
    constraint chk_solicitudes_lic_estado check (estadosolicitud in ('pendiente', 'aprobada', 'rechazada'))
);

create table rrhh_recursos.periodos_vacacionales (
    periodovacacionalid int identity(1,1),
    empleadoid int not null,
    anioperiodo int not null,        
    diasganados int not null,        
    diasgozados int not null,
    diasvendidos int not null,       
    estaabierto bit not null,        
    constraint pk_periodos_vacacionales primary key (periodovacacionalid),
    constraint fk_periodos_vacacionales_emp foreign key (empleadoid) 
        references rrhh_recursos.empleados (empleadoid),
    constraint uq_empleado_anio_vac unique (empleadoid, anioperiodo)
);

create table rrhh_recursos.programacion_vacaciones (
    programacionvacacionid int identity(1,1),
    periodovacacionalid int not null,
    fechainicio date not null,
    fechafin date not null,
    estadosolicitud varchar(15) not null, 
    constraint pk_programacion_vacaciones primary key (programacionvacacionid),
    constraint fk_programacion_vac_periodo foreign key (periodovacacionalid) 
        references rrhh_recursos.periodos_vacacionales (periodovacacionalid),
    constraint chk_programacion_vac_fechas check (fechafin >= fechainicio),
    constraint chk_programacion_vac_estado check (estadosolicitud in ('pendiente', 'aprobada', 'ejecutada', 'anulada'))
);
go


-- =============================================================================
-- módulo de nóminas (motor de cálculo y planillas procesadas)
-- optimizado para sge_crm (entornos windows - todo en lowercase)
-- =============================================================================

create schema rrhh_nomina;
go

-- =============================================================================
-- 1. tablas maestras del motor de cálculo
-- =============================================================================

create table rrhh_nomina.tasas_afp (
    tasasafpid int identity(1,1),
    afpid int not null,
    anio int not null,
    mes int not null,
    porcentajeaporte decimal(5,2) not null,       
    porcentajeseguro decimal(5,2) not null,       
    porcentajecomisionflujo decimal(5,2) not null, 
    porcentajecomisionmixta decimal(5,2) not null, 
    topeprimaseguro decimal(12,2) not null,       
    constraint pk_tasas_afp primary key (tasasafpid),
    constraint fk_tasas_afp_administradoras foreign key (afpid)
        references rrhh_recursos.administradoras_pensiones (afpid),
    constraint uq_tasas_afp_periodo unique (afpid, anio, mes),
    constraint chk_tasas_afp_mes check (mes between 1 and 12)
);

create table rrhh_nomina.conceptos (
    conceptoid int identity(1,1),
    codigosunat varchar(4) not null,          
    nombre varchar(120) not null,
    abreviatura varchar(15) not null,
    tipoconcepto varchar(30) not null,        
    esfijo bit not null,                      
    estaactivo bit not null,
    constraint pk_conceptos primary key (conceptoid),
    constraint uq_conceptos_codigo unique (codigosunat),
    constraint chk_conceptos_tipo check (tipoconcepto in ('ingreso_remunerativo', 'ingreso_no_remunerativo', 'descuento', 'aporte_empleador'))
);

create table rrhh_nomina.conceptos_empleados_fijos (
    conceptoempleadofid int identity(1,1),
    empleadoid int not null,
    conceptoid int not null,
    montofijo decimal(12,2) not null,
    explicacion varchar(250) null,
    estaactivo bit not null,
    constraint pk_conceptos_empleados_fijos primary key (conceptoempleadofid),
    constraint fk_conceptos_emp_fijos_empleado foreign key (empleadoid)
        references rrhh_recursos.empleados (empleadoid),
    constraint fk_conceptos_emp_fijos_concepto foreign key (conceptoid)
        references rrhh_nomina.conceptos (conceptoid), -- ¡corregido sintaxis foreign key aquí!
    constraint uq_empleado_concepto_fijo unique (empleadoid, conceptoid)
);
go

-- =============================================================================
-- 2. periodos y procesamiento de planilla
-- =============================================================================

create table rrhh_nomina.periodos_planillas (
    periodoplanillaid int identity(1,1),
    anio int not null,
    mes int not null,
    tipoplanilla varchar(20) not null,        
    fechainicio date not null,
    fechafin date not null,
    estadoperiodo varchar(15) not null,       
    constraint pk_periodos_planillas primary key (periodoplanillaid),
    constraint uq_periodo_tipo unique (anio, mes, tipoplanilla),
    constraint chk_periodos_planillas_mes check (mes between 1 and 12),
    constraint chk_periodos_planillas_tipo check (tipoplanilla in ('regular_mensual', 'gratificacion', 'cts', 'utilidades')),
    constraint chk_periodos_planillas_estado check (estadoperiodo in ('abierto', 'calculado', 'cerrado')),
    constraint chk_periodos_planillas_fechas check (fechafin >= fechainicio)
);

create table rrhh_nomina.planillas_cabeceras (
    planillacabeceraid int identity(1,1),
    periodoplanillaid int not null,
    fechacalculo datetime not null,
    descripcion varchar(150) not null,
    estadoplanilla varchar(15) not null,      
    usuarioid int not null,                   
    constraint pk_planillas_cabeceras primary key (planillacabeceraid),
    constraint fk_planillas_cab_periodo foreign key (periodoplanillaid)
        references rrhh_nomina.periodos_planillas (periodoplanillaid),
    constraint fk_planillas_cab_usuario foreign key (usuarioid)
        references rrhh_recursos.usuarios_nomina (usuarionominaid),
    constraint chk_planillas_cab_estado check (estadoplanilla in ('borrador', 'procesada', 'cerrada'))
);
go

-- =============================================================================
-- 3. resultados transaccionales (datos inmutables)
-- =============================================================================

create table rrhh_nomina.planillas_detalles (
    planilladetalleid bigint identity(1,1),
    planillacabeceraid int not null,
    empleadoid int not null,
    diaslaborados int not null,               
    diassubsidiados int not null,             
    diasnolaborados int not null,             
    totalingresosremunerativos decimal(12,2) not null,
    totalingresosnoremunerativos decimal(12,2) not null,
    totaldescuentos decimal(12,2) not null,
    totalaportesempleador decimal(12,2) not null, 
    netopagar decimal(12,2) not null,         
    codigohashboleta varchar(64) null,        
    constraint pk_planillas_detalles primary key (planilladetalleid),
    constraint fk_planillas_det_cabecera foreign key (planillacabeceraid)
        references rrhh_nomina.planillas_cabeceras (planillacabeceraid),
    constraint fk_planillas_det_empleado foreign key (empleadoid)
        references rrhh_recursos.empleados (empleadoid),
    constraint uq_cabecera_empleado unique (planillacabeceraid, empleadoid)
);

create table rrhh_nomina.planillas_conceptos_detalles (
    planillaconceptodetalleid bigint identity(1,1),
    planilladetalleid bigint not null,
    conceptoid int not null,
    montocalculado decimal(12,2) not null,    
    constraint pk_planillas_conceptos_detalles primary key (planillaconceptodetalleid),
    constraint fk_planillas_con_det_resumen foreign key (planilladetalleid)
        references rrhh_nomina.planillas_detalles (planilladetalleid) on delete cascade,
    constraint fk_planillas_con_det_concepto foreign key (conceptoid)
        references rrhh_nomina.conceptos (conceptoid)
);
go

-- =============================================================================
-- 4. histórico y proyección de impuestos tributarios (quinta categoría)
-- =============================================================================

create table rrhh_nomina.rentas_quinta_acumuladas (
    rentaquintaid int identity(1,1),
    empleadoid int not null,
    anio int not null,
    ingresosacumuladosbrutos decimal(12,2) not null, 
    impuestoretendidoacumulado decimal(12,2) not null, 
    ingresosotroslempleadores decimal(12,2) not null, 
    constraint pk_rentas_quinta_acumuladas primary key (rentaquintaid),
    constraint fk_rentas_quinta_emp foreign key (empleadoid)
        references rrhh_recursos.empleados (empleadoid),
    constraint uq_empleado_anio_quinta unique (empleadoid, anio)
);
go

-- =============================================================================
-- módulo transversal: sistema (configuración, auditoría y reportes)
-- base de datos: sge_crm (entornos windows - todo en lowercase)
-- =============================================================================

create schema sistema;
go

-- =============================================================================
-- 1. componentes de configuración general
-- =============================================================================

-- Almacena pares clave-valor globales (RUC de la empresa, IGV actual, servidor SMTP, etc.)
create table sistema.parametros (
    parametroid int identity(1,1),
    clave varchar(50) not null,
    valor varchar(max) not null,
    descripcion varchar(250) null,
    categoria varchar(50) not null,             -- 'empresa', 'impuestos', 'correo', 'seguridad'
    fechamodificacion datetime not null,
    constraint pk_parametros primary key (parametroid),
    constraint uq_parametros_clave unique (clave)
);
go

-- =============================================================================
-- 2. componentes de auditoría y seguridad (audit trail)
-- =============================================================================

-- Control estricto de accesos, tokens e IPs (Indispensable para auditorías de seguridad)
create table sistema.sesiones_usuarios (
    sesionid bigint identity(1,1),
    usuario varchar(50) not null,               -- login del usuario que inició sesión
    fechaingreso datetime not null,
    fechasalida datetime null,
    direccionip varchar(45) not null,           -- soporta ipv4 e ipv6
    dispositivo varchar(250) null,              -- user-agent del navegador o app móvil
    tokenacceso varchar(250) null,              -- tracking del jwt o token activo
    estasesionactiva bit not null,
    constraint pk_sesiones_usuarios primary key (sesionid)
);

-- Log transaccional profundo (Registra qué cambió, quién lo cambió, cuándo y los valores)
create table sistema.logs_auditoria_datos (
    logid bigint identity(1,1),
    usuario varchar(50) not null,               -- quién hizo la acción
    tablaafectada varchar(100) not null,        -- ej: 'comercial.clientes', 'rrhh_nomina.planillas_detalles'
    accion varchar(20) not null,                -- 'insert', 'update', 'delete'
    fecharegistro datetime not null,
    idregistroafectado varchar(50) not null,    -- pk del registro modificado (convertido a texto)
    valoranterior varchar(max) null,            -- estructura json con la data antigua (en caso de update/delete)
    valornuevo varchar(max) null,               -- estructura json con la data nueva (en caso de insert/update)
    constraint pk_logs_auditoria_datos primary key (logid),
    constraint chk_logs_auditoria_accion check (accion in ('insert', 'update', 'delete'))
);
go

-- =============================================================================
-- 3. componentes de reportes del sistema
-- =============================================================================

-- Catálogo dinámico de reportes permitidos en el ERP/CRM
create table sistema.reportes_config (
    reporteid int identity(1,1),
    codigo varchar(20) not null,                -- ej: 'rep_vta_001', 'rep_pla_012'
    nombre varchar(100) not null,
    descripcion varchar(250) null,
    moduloorigen varchar(50) not null,          -- 'comercial', 'finanzas', 'operaciones', 'rrhh'
    procedimientonombre varchar(120) not null,  -- nombre del store procedure que ejecuta la lógica
    estaactivo bit not null,
    constraint pk_reportes_config primary key (reporteid),
    constraint uq_reportes_config_codigo unique (codigo)
);

-- Historial de descargas y generación (Clave para protección de datos y fugas de información)
create table sistema.historial_descargas_reportes (
    descargareporteid bigint identity(1,1),
    reporteid int not null,
    usuario varchar(50) not null,               -- usuario que generó el reporte
    fechageneracion datetime not null,
    parametrosusados varchar(max) null,         -- json con los filtros inyectados (ej: '{"fechainicio":"2026-01-01","clienteid":5}')
    formatoexportacion varchar(15) not null,    -- 'pdf', 'excel', 'csv', 'pantalla'
    registrosencontrados int not null,          -- volumen de filas exportadas
    constraint pk_historial_descargas_reportes primary key (descargareporteid),
    constraint fk_historial_rep_config foreign key (reporteid)
        references sistema.reportes_config (reporteid),
    constraint chk_historial_rep_formato check (formatoexportacion in ('pdf', 'excel', 'csv', 'pantalla'))
);
go


