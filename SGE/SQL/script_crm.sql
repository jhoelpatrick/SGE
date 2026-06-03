-- ==========================================================================================
-- módulo exclusivo: comercial (maestros, crm y logística de proveedores)
-- base de datos: sge_crm
-- optimizado para entornos windows (todo en lowercase)
-- ==========================================================================================

if exists (select name from sys.databases where name = 'sge_crm')
begin
    alter database sge_crm set single_user with rollback immediate;
    drop database sge_crm;
end
go

create database sge_crm;
go

USE master;
GO

alter database sge_crm set read_committed_snapshot on;
go

use sge_crm;
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
/*create or alter procedure operaciones.sp_operaciones_registrar_movimiento_kardex
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
go*/

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
-- módulo exclusivo: seguridad
-- ==========================================================================================

CREATE SCHEMA seguridad;
GO

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

-- =============================================================================
-- MÓDULO EXCLUSIVO: RECURSOS HUMANOS (RECURSOS)
-- =============================================================================
-- =============================================================================
-- MÓDULO EXCLUSIVO: RECURSOS HUMANOS (RECURSOS)
-- =============================================================================
CREATE SCHEMA rrhh_recursos;
GO

-- =============================================================================
-- 1. Tablas maestras de configuración de recursos
-- =============================================================================

CREATE TABLE rrhh_recursos.centros_costos (
    centrocostoid INT IDENTITY(1,1),
    codigo VARCHAR(10) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(250) NULL,
    responsable VARCHAR(150) NULL, 
    estaactivo BIT NOT NULL CONSTRAINT df_centros_costos_estaactivo DEFAULT 1, -- Declaración en línea correcta
    CONSTRAINT pk_centros_costos PRIMARY KEY (centrocostoid),
    CONSTRAINT uq_centros_costos_codigo UNIQUE (codigo)
);

CREATE TABLE rrhh_recursos.feriados (
    feriadoid INT IDENTITY(1,1),
    fecha DATE NOT NULL,
    descripcion VARCHAR(150) NOT NULL,
    estaactivo BIT NOT NULL CONSTRAINT df_feriados_estaactivo DEFAULT 1,
    CONSTRAINT pk_feriados PRIMARY KEY (feriadoid),
    CONSTRAINT uq_feriados_fecha UNIQUE (fecha)
);

CREATE TABLE rrhh_recursos.usuarios_nomina (
    usuarionominaid INT IDENTITY(1,1),
    usuario VARCHAR(50) NOT NULL,
    nombrecompleto VARCHAR(150) NOT NULL,
    rol VARCHAR(50) NOT NULL, 
    correo VARCHAR(100) NOT NULL,
    estaactivo BIT NOT NULL CONSTRAINT df_usuarios_nomina_estaactivo DEFAULT 1,
    CONSTRAINT pk_usuarios_nomina PRIMARY KEY (usuarionominaid),
    CONSTRAINT uq_usuarios_nomina_usuario UNIQUE (usuario)
);
GO

-- =============================================================================
-- 2. Entidades core del personal
-- =============================================================================

CREATE TABLE rrhh_recursos.empleados (
    empleadoid INT IDENTITY(1,1),
    tipodocumento VARCHAR(5) NOT NULL, 
    numerodocumento VARCHAR(15) NOT NULL,
    nombres VARCHAR(70) NOT NULL,
    apellidopaterno VARCHAR(70) NOT NULL,
    apellidomaterno VARCHAR(70) NOT NULL,
    fechanacimiento DATE NOT NULL,
    sexo CHAR(1) NOT NULL, 
    correopersonal VARCHAR(100) NULL,
    correocorporativo VARCHAR(100) NULL,
    telefonocelular VARCHAR(20) NULL,
    centrocostoid INT NOT NULL,
    estaactivo BIT NOT NULL CONSTRAINT df_empleados_estaactivo DEFAULT 1,
    CONSTRAINT pk_empleados PRIMARY KEY (empleadoid),
    CONSTRAINT uq_empleados_documento UNIQUE (tipodocumento, numerodocumento),
    CONSTRAINT fk_empleados_centros_costos FOREIGN KEY (centrocostoid) 
        REFERENCES rrhh_recursos.centros_costos (centrocostoid),
    CONSTRAINT chk_empleados_sexo CHECK (sexo IN ('m', 'f'))
);

CREATE TABLE rrhh_recursos.contratos (
    contratoid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    tipocontrato VARCHAR(50) NOT NULL, 
    fechainicio DATE NOT NULL,
    fechafin DATE NULL, 
    sueldobase DECIMAL(12,2) NOT NULL,
    estaactivo BIT NOT NULL CONSTRAINT df_contratos_estaactivo DEFAULT 1,
    CONSTRAINT pk_contratos PRIMARY KEY (contratoid),
    CONSTRAINT fk_contratos_empleados FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT chk_contratos_fechas CHECK (fechafin IS NULL OR fechafin >= fechainicio)
);
GO

-- =============================================================================
-- 3. Tablas maestras del T-Registro (SUNAT) y localización
-- =============================================================================

CREATE TABLE rrhh_recursos.ubigeos (
    ubigeoid CHAR(6) NOT NULL, 
    departamento VARCHAR(50) NOT NULL,
    provincia VARCHAR(50) NOT NULL,
    distrito VARCHAR(50) NOT NULL,
    CONSTRAINT pk_ubigeos PRIMARY KEY (ubigeoid)
);

CREATE TABLE rrhh_recursos.regimenes_laborales (
    regimenlaboralid INT IDENTITY(1,1),
    codigosunat VARCHAR(4) NOT NULL, 
    nombre VARCHAR(100) NOT NULL,    
    estaactivo BIT NOT NULL CONSTRAINT df_regimenes_laborales_estaactivo DEFAULT 1,
    CONSTRAINT pk_regimenes_laborales PRIMARY KEY (regimenlaboralid),
    CONSTRAINT uq_regimenes_laborales_codigo UNIQUE (codigosunat)
);

CREATE TABLE rrhh_recursos.administradoras_pensiones (
    afpid INT IDENTITY(1,1),
    codigosunat VARCHAR(4) NOT NULL, 
    nombre VARCHAR(50) NOT NULL,
    tipo CHAR(3) NOT NULL,           
    estaactivo BIT NOT NULL CONSTRAINT df_administradoras_pensiones_estaactivo DEFAULT 1,
    CONSTRAINT pk_administradoras_pensiones PRIMARY KEY (afpid),
    CONSTRAINT uq_administradoras_pensiones_codigo UNIQUE (codigosunat),
    CONSTRAINT chk_administradoras_pensiones_tipo CHECK (tipo IN ('afp', 'onp'))
);
GO

-- =============================================================================
-- 4. Ampliación del legajo del empleado
-- =============================================================================

CREATE TABLE rrhh_recursos.datos_laborales_empleados (
    empleadoid INT NOT NULL,
    regimenlaboralid INT NOT NULL,
    afpid INT NOT NULL,
    tipocomision VARCHAR(15) NOT NULL, 
    cuspp VARCHAR(20) NULL,            
    ubigeodomicilio CHAR(6) NOT NULL,
    direccion VARCHAR(250) NOT NULL,
    cuentasueldo VARCHAR(30) NULL,
    bancosueldoid INT NULL,            
    cuentacts VARCHAR(30) NULL,
    bancoctsid INT NULL,
    CONSTRAINT pk_datos_laborales_empleados PRIMARY KEY (empleadoid),
    CONSTRAINT fk_datos_laborales_emp FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT fk_datos_laborales_regimen FOREIGN KEY (regimenlaboralid) 
        REFERENCES rrhh_recursos.regimenes_laborales (regimenlaboralid),
    CONSTRAINT fk_datos_laborales_afp FOREIGN KEY (afpid) 
        REFERENCES rrhh_recursos.administradoras_pensiones (afpid),
    CONSTRAINT fk_datos_laborales_ubigeo FOREIGN KEY (ubigeodomicilio) 
        REFERENCES rrhh_recursos.ubigeos (ubigeoid),
    CONSTRAINT chk_datos_laborales_comision CHECK (tipocomision IN ('flujo', 'mixta', 'no_aplica'))
);

CREATE TABLE rrhh_recursos.derechohabientes (
    derechohabienteid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    vinculofamiliar VARCHAR(20) NOT NULL, 
    tipodocumento VARCHAR(5) NOT NULL,
    numerodocumento VARCHAR(15) NOT NULL,
    nombres VARCHAR(70) NOT NULL,
    apellidopaterno VARCHAR(70) NOT NULL,
    apellidomaterno VARCHAR(70) NOT NULL,
    fechanacimiento DATE NOT NULL,
    CONSTRAINT pk_derechohabientes PRIMARY KEY (derechohabienteid),
    CONSTRAINT fk_derechohabientes_empleados FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT uq_derechohabientes_doc UNIQUE (tipodocumento, numerodocumento)
);
GO

-- =============================================================================
-- 5. Control de asistencia, turnos y tareo biométrico
-- =============================================================================

CREATE TABLE rrhh_recursos.turnos (
    turnoid INT IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL, 
    horaingreso TIME NOT NULL,
    horasalida TIME NOT NULL,
    toleranciaingreso INT NOT NULL CONSTRAINT df_turnos_tolerancia DEFAULT 5, 
    tiemporefrigerio INT NOT NULL CONSTRAINT df_turnos_refrigerio DEFAULT 60,   
    estaactivo BIT NOT NULL CONSTRAINT df_turnos_estaactivo DEFAULT 1,
    CONSTRAINT pk_turnos PRIMARY KEY (turnoid)
);

CREATE TABLE rrhh_recursos.marcaciones_biometricos (
    marcacionid BIGINT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    fechahora DATETIME NOT NULL,
    tipo VARCHAR(15) NOT NULL, 
    dispositivo VARCHAR(50) NULL,      
    CONSTRAINT pk_marcaciones_biometricos PRIMARY KEY (marcacionid),
    CONSTRAINT fk_marcaciones_biometricos_emp FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT chk_marcaciones_tipo CHECK (tipo IN ('ingreso', 'salida_ref', 'retorno_ref', 'salida'))
);

CREATE TABLE rrhh_recursos.asistencias_diarias (
    asistenciadiariaid BIGINT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    fecha DATE NOT NULL,
    turnoid INT NOT NULL,
    horaingresoreal TIME NULL,
    horasalidareal TIME NULL,
    minutostardanza INT NOT NULL CONSTRAINT df_asistencias_tardanza DEFAULT 0,
    minutosextras25 INT NOT NULL CONSTRAINT df_asistencias_ex25 DEFAULT 0,
    minutosextras35 INT NOT NULL CONSTRAINT df_asistencias_ex35 DEFAULT 0,
    minutosnocturnas INT NOT NULL CONSTRAINT df_asistencias_nocturnas DEFAULT 0,
    estadoasistencia VARCHAR(20) NOT NULL, 
    CONSTRAINT pk_asistencias_diarias PRIMARY KEY (asistenciadiariaid),
    CONSTRAINT fk_asistencias_diarias_emp FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT fk_asistencias_diarias_turno FOREIGN KEY (turnoid) 
        REFERENCES rrhh_recursos.turnos (turnoid),
    CONSTRAINT uq_empleado_fecha UNIQUE (empleadoid, fecha),
    CONSTRAINT chk_asistencias_estado CHECK (estadoasistencia IN ('asistio', 'falta', 'feriado', 'licencia', 'vacaciones'))
);
GO

-- =============================================================================
-- 6. Vacaciones y licencias (gestión de ausencias)
-- =============================================================================

CREATE TABLE rrhh_recursos.tipos_licencias (
    tipolicenciaid INT IDENTITY(1,1),
    codigosunat VARCHAR(4) NOT NULL, 
    descripcion VARCHAR(150) NOT NULL,
    congocehaber BIT NOT NULL,
    essubsidiado BIT NOT NULL,      
    estaactivo BIT NOT NULL CONSTRAINT df_tipos_licencias_estaactivo DEFAULT 1,
    CONSTRAINT pk_tipos_licencias PRIMARY KEY (tipolicenciaid),
    CONSTRAINT uq_tipos_licencias_codigo UNIQUE (codigosunat)
);

CREATE TABLE rrhh_recursos.solicitudes_licencias (
    solicitudlicenciaid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    tipolicenciaid INT NOT NULL,
    fechainicio DATE NOT NULL,
    fechafin DATE NOT NULL,
    estadosolicitud VARCHAR(15) NOT NULL, 
    usuariosolicitaid INT NOT NULL,       
    sustento VARCHAR(500) NULL,          
    CONSTRAINT pk_solicitudes_licencias PRIMARY KEY (solicitudlicenciaid),
    CONSTRAINT fk_solicitudes_lic_emp FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT fk_solicitudes_lic_tipo FOREIGN KEY (tipolicenciaid) 
        REFERENCES rrhh_recursos.tipos_licencias (tipolicenciaid),
    CONSTRAINT fk_solicitudes_lic_usr FOREIGN KEY (usuariosolicitaid) 
        REFERENCES rrhh_recursos.usuarios_nomina (usuarionominaid),
    CONSTRAINT chk_solicitudes_lic_fechas CHECK (fechafin >= fechainicio),
    CONSTRAINT chk_solicitudes_lic_estado CHECK (estadosolicitud IN ('pendiente', 'aprobada', 'rechazada'))
);

CREATE TABLE rrhh_recursos.periodos_vacacionales (
    periodovacacionalid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    anioperiodo INT NOT NULL,        
    diasganados INT NOT NULL CONSTRAINT df_periodos_vac_ganados DEFAULT 30,        
    diasgozados INT NOT NULL CONSTRAINT df_periodos_vac_gozados DEFAULT 0,
    diasvendidos INT NOT NULL CONSTRAINT df_periodos_vac_vendidos DEFAULT 0,       
    estaabierto BIT NOT NULL CONSTRAINT df_periodos_vac_estaabierto DEFAULT 1,        
    CONSTRAINT pk_periodos_vacacionales PRIMARY KEY (periodovacacionalid),
    CONSTRAINT fk_periodos_vacacionales_emp FOREIGN KEY (empleadoid) 
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT uq_empleado_anio_vac UNIQUE (empleadoid, anioperiodo)
);

CREATE TABLE rrhh_recursos.programacion_vacaciones (
    programacionvacacionid INT IDENTITY(1,1),
    periodovacacionalid INT NOT NULL,
    fechainicio DATE NOT NULL,
    fechafin DATE NOT NULL,
    estadosolicitud VARCHAR(15) NOT NULL, 
    CONSTRAINT pk_programacion_vacaciones PRIMARY KEY (programacionvacacionid),
    CONSTRAINT fk_programacion_vac_periodo FOREIGN KEY (periodovacacionalid) 
        REFERENCES rrhh_recursos.periodos_vacacionales (periodovacacionalid),
    CONSTRAINT chk_programacion_vac_fechas CHECK (fechafin >= fechainicio),
    CONSTRAINT chk_programacion_vac_estado CHECK (estadosolicitud IN ('pendiente', 'aprobada', 'ejecutada', 'anulada'))
);
GO


-- =============================================================================
-- MÓDULO DE NÓMINAS (MOTOR DE CÁLCULO Y PLANILLAS)
-- =============================================================================
CREATE SCHEMA rrhh_nomina;
GO

-- =============================================================================
-- 1. Tablas maestras del motor de cálculo
-- =============================================================================

CREATE TABLE rrhh_nomina.tasas_afp (
    tasasafpid INT IDENTITY(1,1),
    afpid INT NOT NULL,
    anio INT NOT NULL,
    mes INT NOT NULL,
    porcentajeaporte DECIMAL(5,2) NOT NULL,       
    porcentajeseguro DECIMAL(5,2) NOT NULL,       
    porcentajecomisionflujo DECIMAL(5,2) NOT NULL, 
    porcentajecomisionmixta DECIMAL(5,2) NOT NULL, 
    topeprimaseguro DECIMAL(12,2) NOT NULL,       
    CONSTRAINT pk_tasas_afp PRIMARY KEY (tasasafpid),
    CONSTRAINT fk_tasas_afp_administradoras FOREIGN KEY (afpid)
        REFERENCES rrhh_recursos.administradoras_pensiones (afpid),
    CONSTRAINT uq_tasas_afp_periodo UNIQUE (afpid, anio, mes),
    CONSTRAINT chk_tasas_afp_mes CHECK (mes BETWEEN 1 AND 12)
);

CREATE TABLE rrhh_nomina.conceptos (
    conceptoid INT IDENTITY(1,1),
    codigosunat VARCHAR(4) NOT NULL,          
    nombre VARCHAR(120) NOT NULL,
    abreviatura VARCHAR(15) NOT NULL,
    tipoconcepto VARCHAR(30) NOT NULL,        
    esfijo BIT NOT NULL CONSTRAINT df_conceptos_esfijo DEFAULT 0,                      
    estaactivo BIT NOT NULL CONSTRAINT df_conceptos_estaactivo DEFAULT 1,
    CONSTRAINT pk_conceptos PRIMARY KEY (conceptoid),
    CONSTRAINT uq_conceptos_codigo UNIQUE (codigosunat),
    CONSTRAINT chk_conceptos_tipo CHECK (tipoconcepto IN ('ingreso_remunerativo', 'ingreso_no_remunerativo', 'descuento', 'aporte_empleador'))
);

CREATE TABLE rrhh_nomina.conceptos_empleados_fijos (
    conceptoempleadofid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    conceptoid INT NOT NULL,
    montofijo DECIMAL(12,2) NOT NULL,
    explicacion VARCHAR(250) NULL,
    estaactivo BIT NOT NULL CONSTRAINT df_conceptos_emp_fijos_activo DEFAULT 1,
    CONSTRAINT pk_conceptos_empleados_fijos PRIMARY KEY (conceptoempleadofid),
    CONSTRAINT fk_conceptos_emp_fijos_empleado FOREIGN KEY (empleadoid)
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT fk_conceptos_emp_fijos_concepto FOREIGN KEY (conceptoid)
        REFERENCES rrhh_nomina.conceptos (conceptoid), 
    CONSTRAINT uq_empleado_concepto_fijo UNIQUE (empleadoid, conceptoid)
);
GO

-- =============================================================================
-- 2. Periodos y procesamiento de planilla
-- =============================================================================

CREATE TABLE rrhh_nomina.periodos_planillas (
    periodoplanillaid INT IDENTITY(1,1),
    anio INT NOT NULL,
    mes INT NOT NULL,
    tipoplanilla VARCHAR(20) NOT NULL,        
    fechainicio DATE NOT NULL,
    fechafin DATE NOT NULL,
    estadoperiodo VARCHAR(15) NOT NULL,       
    CONSTRAINT pk_periodos_planillas PRIMARY KEY (periodoplanillaid),
    CONSTRAINT uq_periodo_tipo UNIQUE (anio, mes, tipoplanilla),
    CONSTRAINT chk_periodos_planillas_mes CHECK (mes BETWEEN 1 AND 12),
    CONSTRAINT chk_periodos_planillas_tipo CHECK (tipoplanilla IN ('regular_mensual', 'gratificacion', 'cts', 'utilidades')),
    CONSTRAINT chk_periodos_planillas_estado CHECK (estadoperiodo IN ('abierto', 'calculado', 'cerrado')),
    CONSTRAINT chk_periodos_planillas_fechas CHECK (fechafin >= fechainicio)
);

CREATE TABLE rrhh_nomina.planillas_cabeceras (
    planillacabeceraid INT IDENTITY(1,1),
    periodoplanillaid INT NOT NULL,
    fechacalculo DATETIME NOT NULL,
    descripcion VARCHAR(150) NOT NULL,
    estadoplanilla VARCHAR(15) NOT NULL,      
    usuarioid INT NOT NULL,                    
    CONSTRAINT pk_planillas_cabeceras PRIMARY KEY (planillacabeceraid),
    CONSTRAINT fk_planillas_cab_periodo FOREIGN KEY (periodoplanillaid)
        REFERENCES rrhh_nomina.periodos_planillas (periodoplanillaid),
    CONSTRAINT fk_planillas_cab_usuario FOREIGN KEY (usuarioid)
        REFERENCES rrhh_recursos.usuarios_nomina (usuarionominaid),
    CONSTRAINT chk_planillas_cab_estado CHECK (estadoplanilla IN ('borrador', 'procesada', 'cerrada'))
);
GO

-- =============================================================================
-- 3. Resultados transaccionales (datos inmutables)
-- =============================================================================

CREATE TABLE rrhh_nomina.planillas_detalles (
    planilladetalleid BIGINT IDENTITY(1,1),
    planillacabeceraid INT NOT NULL,
    empleadoid INT NOT NULL,
    diaslaborados INT NOT NULL,               
    diassubsidiados INT NOT NULL,             
    diasnolaborados INT NOT NULL,             
    totalingresosremunerativos DECIMAL(12,2) NOT NULL,
    totalingresosnoremunerativos DECIMAL(12,2) NOT NULL,
    totaldescuentos DECIMAL(12,2) NOT NULL,
    totalaportesempleador DECIMAL(12,2) NOT NULL, 
    netopagar DECIMAL(12,2) NOT NULL,         
    codigohashboleta VARCHAR(64) NULL,        
    CONSTRAINT pk_planillas_detalles PRIMARY KEY (planilladetalleid),
    CONSTRAINT fk_planillas_det_cabecera FOREIGN KEY (planillacabeceraid)
        REFERENCES rrhh_nomina.planillas_cabeceras (planillacabeceraid),
    CONSTRAINT fk_planillas_det_empleado FOREIGN KEY (empleadoid)
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT uq_cabecera_empleado UNIQUE (planillacabeceraid, empleadoid)
);

CREATE TABLE rrhh_nomina.planillas_conceptos_detalles (
    planillaconceptodetalleid BIGINT IDENTITY(1,1),
    planilladetalleid BIGINT NOT NULL,
    conceptoid INT NOT NULL,
    montocalculado DECIMAL(12,2) NOT NULL,    
    CONSTRAINT pk_planillas_conceptos_detalles PRIMARY KEY (planillaconceptodetalleid),
    CONSTRAINT fk_planillas_con_det_resumen FOREIGN KEY (planilladetalleid)
        REFERENCES rrhh_nomina.planillas_detalles (planilladetalleid) ON DELETE CASCADE,
    CONSTRAINT fk_planillas_con_det_concepto FOREIGN KEY (conceptoid)
        REFERENCES rrhh_nomina.conceptos (conceptoid)
);
GO

-- =============================================================================
-- 4. Histórico y proyección de impuestos tributarios (quinta categoría)
-- =============================================================================

CREATE TABLE rrhh_nomina.rentas_quinta_acumuladas (
    rentaquintaid INT IDENTITY(1,1),
    empleadoid INT NOT NULL,
    anio INT NOT NULL,
    ingresosacumuladosbrutos DECIMAL(12,2) NOT NULL, 
    impuestoretendidoacumulado DECIMAL(12,2) NOT NULL, 
    ingresosotroslempleadores DECIMAL(12,2) NOT NULL, 
    CONSTRAINT pk_rentas_quinta_acumuladas PRIMARY KEY (rentaquintaid),
    CONSTRAINT fk_rentas_quinta_emp FOREIGN KEY (empleadoid)
        REFERENCES rrhh_recursos.empleados (empleadoid),
    CONSTRAINT uq_empleado_anio_quinta UNIQUE (empleadoid, anio)
);
GO


-- =============================================================================
-- MÓDULO TRANSVERSAL: SISTEMA
-- =============================================================================
CREATE SCHEMA sistema;
GO

-- =============================================================================
-- 1. Componentes de configuración general
-- =============================================================================

CREATE TABLE sistema.parametros (
    parametroid INT IDENTITY(1,1),
    clave VARCHAR(50) NOT NULL,
    valor VARCHAR(MAX) NOT NULL,
    descripcion VARCHAR(250) NULL,
    categoria VARCHAR(50) NOT NULL,             
    fechamodificacion DATETIME NOT NULL CONSTRAINT df_parametros_fechamodif DEFAULT GETDATE(),
    CONSTRAINT pk_parametros PRIMARY KEY (parametroid),
    CONSTRAINT uq_parametros_clave UNIQUE (clave)
);
GO

-- =============================================================================
-- 2. Componentes de auditoría y seguridad (audit trail)
-- =============================================================================

CREATE TABLE sistema.sesiones_usuarios (
    sesionid BIGINT IDENTITY(1,1),
    usuario VARCHAR(50) NOT NULL,               
    fechaingreso DATETIME NOT NULL CONSTRAINT df_sesiones_fechaingreso DEFAULT GETDATE(),
    fechasalida DATETIME NULL,
    direccionip VARCHAR(45) NOT NULL,           
    dispositivo VARCHAR(250) NULL,              
    tokenacceso VARCHAR(250) NULL,              
    estasesionactiva BIT NOT NULL CONSTRAINT df_sesiones_activa DEFAULT 1,
    CONSTRAINT pk_sesiones_usuarios PRIMARY KEY (sesionid)
);

CREATE TABLE sistema.logs_auditoria_datos (
    logid BIGINT IDENTITY(1,1),
    usuario VARCHAR(50) NOT NULL,               
    tablaafectada VARCHAR(100) NOT NULL,        
    accion VARCHAR(20) NOT NULL,                
    fecharegistro DATETIME NOT NULL CONSTRAINT df_logs_auditoria_fecha DEFAULT GETDATE(),
    idregistroafectado VARCHAR(50) NOT NULL,    
    valoranterior VARCHAR(MAX) NULL,            
    valornuevo VARCHAR(MAX) NULL,               
    CONSTRAINT pk_logs_auditoria_datos PRIMARY KEY (logid),
    CONSTRAINT chk_logs_auditoria_accion CHECK (accion IN ('insert', 'update', 'delete'))
);
GO

-- =============================================================================
-- 3. Componentes de reportes del sistema
-- =============================================================================

CREATE TABLE sistema.reportes_config (
    reporteid INT IDENTITY(1,1),
    codigo VARCHAR(20) NOT NULL,                
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(250) NULL,
    moduloorigen VARCHAR(50) NOT NULL,          
    procedimientonombre VARCHAR(120) NOT NULL,  
    estaactivo BIT NOT NULL CONSTRAINT df_reportes_config_activo DEFAULT 1,
    CONSTRAINT pk_reportes_config PRIMARY KEY (reporteid),
    CONSTRAINT uq_reportes_config_codigo UNIQUE (codigo)
);

CREATE TABLE sistema.historial_descargas_reportes (
    descargareporteid BIGINT IDENTITY(1,1),
    reporteid INT NOT NULL,
    usuario VARCHAR(50) NOT NULL,               
    fechageneracion DATETIME NOT NULL CONSTRAINT df_historial_rep_fecha DEFAULT GETDATE(),
    parametrosusados VARCHAR(MAX) NULL,         
    formatoexportacion VARCHAR(15) NOT NULL,    
    registrosencontrados INT NOT NULL,          
    CONSTRAINT pk_historial_descargas_reportes PRIMARY KEY (descargareporteid),
    CONSTRAINT fk_historial_rep_config FOREIGN KEY (reporteid)
        REFERENCES sistema.reportes_config (reporteid),
    CONSTRAINT chk_historial_rep_formato CHECK (formatoexportacion IN ('pdf', 'excel', 'csv', 'pantalla'))
);
GO