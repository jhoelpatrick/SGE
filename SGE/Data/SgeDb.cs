using Dapper;
using Microsoft.Data.SqlClient;
using SGE.Models;

namespace SGE.Data
{
    /// <summary>
    /// Capa de acceso a datos con Dapper.
    /// Todas las operaciones usan la misma cadena de conexión de appsettings.json.
    /// </summary>
    public class SgeDb
    {
        private readonly string _conn;
        public SgeDb(IConfiguration cfg) => _conn = cfg.GetConnectionString("SGE")!;
        private SqlConnection Open() => new(_conn);

        // ══════════════════════════════════════════════════════════════
        // EMPLEADOS
        // ══════════════════════════════════════════════════════════════

        public List<Empleado> ObtenerEmpleados(string buscar = "", string estado = "", string dept = "")
        {
            const string sql = @"
                SELECT id, codigo,
                       nombres, apellido_paterno, apellido_materno,
                       tipo_documento, numero_documento,
                       fecha_nacimiento, sexo, telefono, email, direccion,
                       fecha_ingreso, fecha_cese, cargo, departamento, centro_costo_id,
                       tipo_contrato, regimen_laboral, estado,
                       sueldo_base, asignacion_familiar, tiene_hijos,
                       sistema_previsional, codigo_afp, cuspp,
                       banco_pago, numero_cuenta, tipo_cuenta, cci,
                       afecto_renta_5ta, afecto_essalud
                FROM nomina.empleados
                WHERE (@buscar = '' OR nombres LIKE '%'+@buscar+'%'
                               OR apellido_paterno LIKE '%'+@buscar+'%'
                               OR apellido_materno LIKE '%'+@buscar+'%'
                               OR numero_documento LIKE '%'+@buscar+'%'
                               OR cargo LIKE '%'+@buscar+'%')
                  AND (@estado = '' OR estado = @estado)
                  AND (@dept = '' OR departamento = @dept)
                ORDER BY apellido_paterno, apellido_materno, nombres";

            using var db = Open();
            return db.Query<EmpleadoRow>(sql, new { buscar, estado, dept })
                     .Select(MapEmpleado).ToList();
        }
        public bool ExisteNumeroDocumento(string numeroDocumento, int excludeId = 0)
        {
            using var db = Open();
            return db.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM nomina.empleados WHERE numero_documento = @numeroDocumento AND id <> @excludeId",
                new { numeroDocumento, excludeId }) > 0;
        }
        public Empleado? ObtenerEmpleadoPorId(int id)
        {
            const string sql = @"
                SELECT id, codigo, nombres, apellido_paterno, apellido_materno,
                       tipo_documento, numero_documento, fecha_nacimiento, sexo,
                       telefono, email, direccion,
                       fecha_ingreso, fecha_cese, cargo, departamento, centro_costo_id,
                       tipo_contrato, regimen_laboral, estado,
                       sueldo_base, asignacion_familiar, tiene_hijos,
                       sistema_previsional, codigo_afp, cuspp,
                       banco_pago, numero_cuenta, tipo_cuenta, cci,
                       afecto_renta_5ta, afecto_essalud
                FROM nomina.empleados WHERE id = @id";
            using var db = Open();
            var row = db.QueryFirstOrDefault<EmpleadoRow>(sql, new { id });
            return row == null ? null : MapEmpleado(row);
        }

        public int InsertarEmpleado(Empleado e)
        {
            const string sql = @"
                INSERT INTO nomina.empleados
                    (codigo, nombres, apellido_paterno, apellido_materno,
                     tipo_documento, numero_documento, fecha_nacimiento, sexo,
                     telefono, email, direccion,
                     fecha_ingreso, cargo, departamento, centro_costo_id,
                     tipo_contrato, regimen_laboral, estado,
                     sueldo_base, asignacion_familiar, tiene_hijos,
                     sistema_previsional, banco_pago, numero_cuenta, cci,
                     afecto_renta_5ta, afecto_essalud)
                OUTPUT INSERTED.id
                VALUES
                    (@Codigo, @Nombres, @ApellidoPaterno, @ApellidoMaterno,
                     @TipoDocumento, @NumeroDocumento, @FechaNacimiento, @Sexo,
                     @Telefono, @Email, @Direccion,
                     @FechaIngreso, @Cargo, @Departamento, @CentroCostoId,
                     @TipoContrato, @RegimeLaboral, @Estado,
                     @SueldoBase, @AsignacionFamiliar, @TieneHijos,
                     @SistemaPrevisional, @BancoPago, @NumeroCuenta, @CCI,
                     @AfectoRenta5ta, @AfectoEssalud)";
            using var db = Open();
            return db.ExecuteScalar<int>(sql, new
            {
                e.Codigo,
                e.Nombres,
                e.ApellidoPaterno,
                e.ApellidoMaterno,
                TipoDocumento = e.TipoDocumento.ToString(),
                e.NumeroDocumento,
                e.FechaNacimiento,
                e.Sexo,
                e.Telefono,
                e.Email,
                e.Direccion,
                e.FechaIngreso,
                e.Cargo,
                e.Departamento,
                e.CentroCostoId,
                TipoContrato = e.TipoContrato.ToString(),
                RegimeLaboral = e.RegimeLaboral.ToString(),
                Estado = e.Estado.ToString(),
                e.SueldoBase,
                e.AsignacionFamiliar,
                e.TieneHijos,
                SistemaPrevisional = e.SistemaPrevisional.ToString(),
                BancoPago = e.BancoPago.ToString(),
                e.NumeroCuenta,
                e.CCI,
                e.AfectoRenta5ta,
                e.AfectoEssalud
            });
        }

        public void ActualizarEmpleado(Empleado e)
        {
            const string sql = @"
                UPDATE nomina.empleados SET
                    nombres=@Nombres, apellido_paterno=@ApellidoPaterno,
                    apellido_materno=@ApellidoMaterno, tipo_documento=@TipoDocumento,
                    numero_documento=@NumeroDocumento, fecha_nacimiento=@FechaNacimiento,
                    sexo=@Sexo, telefono=@Telefono, email=@Email, direccion=@Direccion,
                    cargo=@Cargo, departamento=@Departamento, centro_costo_id=@CentroCostoId,
                    tipo_contrato=@TipoContrato, regimen_laboral=@RegimeLaboral,
                    estado=@Estado, sueldo_base=@SueldoBase,
                    asignacion_familiar=@AsignacionFamiliar, tiene_hijos=@TieneHijos,
                    sistema_previsional=@SistemaPrevisional,
                    banco_pago=@BancoPago, numero_cuenta=@NumeroCuenta, cci=@CCI,
                    afecto_renta_5ta=@AfectoRenta5ta, afecto_essalud=@AfectoEssalud
                WHERE id = @Id";
            using var db = Open();
            db.Execute(sql, new
            {
                e.Id,
                e.Nombres,
                e.ApellidoPaterno,
                e.ApellidoMaterno,
                TipoDocumento = e.TipoDocumento.ToString(),
                e.NumeroDocumento,
                e.FechaNacimiento,
                e.Sexo,
                e.Telefono,
                e.Email,
                e.Direccion,
                e.Cargo,
                e.Departamento,
                e.CentroCostoId,
                TipoContrato = e.TipoContrato.ToString(),
                RegimeLaboral = e.RegimeLaboral.ToString(),
                Estado = e.Estado.ToString(),
                e.SueldoBase,
                e.AsignacionFamiliar,
                e.TieneHijos,
                SistemaPrevisional = e.SistemaPrevisional.ToString(),
                BancoPago = e.BancoPago.ToString(),
                e.NumeroCuenta,
                e.CCI,
                e.AfectoRenta5ta,
                e.AfectoEssalud
            });
        }

        public void EliminarEmpleado(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.empleados WHERE id = @id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // PLANILLAS
        // ══════════════════════════════════════════════════════════════

        public List<Planilla> ObtenerPlanillas(string buscar = "", string estado = "")
        {
            const string sql = @"
                SELECT
                    codigo          AS Codigo,
                    periodo         AS Periodo,
                    empleados       AS Empleados,
                    total_bruto     AS TotalBruto,
                    descuentos      AS Descuentos,
                    total_neto      AS TotalNeto,
                    estado          AS Estado,
                    fecha_cierre    AS FechaCierre,
                    total_descuentos AS TotalDescuentos
                FROM nomina.planillas
                WHERE (@buscar = '' OR codigo LIKE '%'+@buscar+'%' OR periodo LIKE '%'+@buscar+'%')
                  AND (@estado = '' OR estado = @estado)
                ORDER BY fecha_registro DESC";
            using var db = Open();
            return db.Query<Planilla>(sql, new { buscar, estado }).ToList();
        }

        public Planilla? ObtenerPlanillaPorCodigo(string codigo)
        {
            const string sql = @"
                SELECT
                    codigo          AS Codigo,
                    periodo         AS Periodo,
                    empleados       AS Empleados,
                    total_bruto     AS TotalBruto,
                    descuentos      AS Descuentos,
                    total_neto      AS TotalNeto,
                    estado          AS Estado,
                    fecha_cierre    AS FechaCierre,
                    total_descuentos AS TotalDescuentos
                FROM nomina.planillas
                WHERE codigo = @codigo";
            using var db = Open();
            return db.QueryFirstOrDefault<Planilla>(sql, new { codigo });
        }

        public void InsertarPlanilla(Planilla p)
        {
            const string sql = @"
                INSERT INTO nomina.planillas (codigo, periodo, empleados, total_bruto, descuentos, total_neto, estado, fecha_cierre, total_descuentos)
                VALUES (@Codigo, @Periodo, @Empleados, @TotalBruto, @Descuentos, @TotalNeto, @Estado, @FechaCierre, @TotalDescuentos)";
            using var db = Open();
            db.Execute(sql, p);
        }

        public void ActualizarEstadoPlanilla(string codigo, string estado)
        {
            using var db = Open();
            db.Execute("UPDATE nomina.planillas SET estado=@estado WHERE codigo=@codigo", new { codigo, estado });

        }
        public void ActualizarPlanilla(Planilla p)
        {
            const string sql = @"
                UPDATE nomina.planillas SET
                    periodo           = @Periodo,
                    empleados         = @Empleados,
                    total_bruto       = @TotalBruto,
                    descuentos        = @Descuentos,
                    total_neto        = @TotalNeto,
                    total_descuentos  = @TotalDescuentos,
                    estado            = @Estado,
                    fecha_cierre      = CASE WHEN @FechaCierre < '1900-01-01' THEN NULL ELSE @FechaCierre END
                WHERE codigo = @Codigo";
            using var db = Open();
            db.Execute(sql, new
            {
                p.Codigo,
                p.Periodo,
                p.Empleados,
                p.TotalBruto,
                p.Descuentos,
                p.TotalNeto,
                p.TotalDescuentos,
                p.Estado,
                FechaCierre = p.FechaCierre == DateTime.MinValue ? (DateTime?)null : p.FechaCierre
            });
        }

        public void EliminarPlanilla(string codigo)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.detalle_planilla WHERE codigo_planilla = @codigo", new { codigo });
            db.Execute("DELETE FROM nomina.planillas WHERE codigo = @codigo", new { codigo });
        }

        // ══════════════════════════════════════════════════════════════
        // DETALLE PLANILLA
        // ══════════════════════════════════════════════════════════════

        public List<DetallePlanilla> ObtenerDetallePlanilla(string codigoPlanilla)
        {
            const string sql = @"
                SELECT d.id, d.codigo_planilla, d.empleado_id, d.periodo,
                       d.sueldo_base, d.asignacion_familiar, d.horas_extras,
                       d.movilidad, d.refrigerio, d.bonif_desempenio,
                       d.otros_ingresos, d.total_bruto,
                       d.descuento_afp_onp, d.comision_afp, d.seguro_afp,
                       d.essalud_trabajador, d.renta_5ta_categoria, d.sctr,
                       d.prestamos, d.adelantos, d.tardanzas_faltas,
                       d.otros_descuentos, d.total_descuentos,
                       d.essalud_empleador, d.sctr_empleador, d.total_neto,
                       d.estado, d.calculado_por,
                       e.nombres + ' ' + e.apellido_paterno AS nombre_empleado
                FROM nomina.detalle_planilla d
                JOIN nomina.empleados e ON e.id = d.empleado_id
                WHERE d.codigo_planilla = @codigoPlanilla
                ORDER BY e.apellido_paterno";
            using var db = Open();
            return db.Query<DetallePlanilla>(sql, new { codigoPlanilla }).ToList();
        }

        public void InsertarDetallePlanilla(DetallePlanilla d)
        {
            const string sql = @"
                MERGE nomina.detalle_planilla AS target
                USING (VALUES (@CodigoPlanilla, @EmpleadoId)) AS src(codigo_planilla, empleado_id)
                ON target.codigo_planilla = src.codigo_planilla AND target.empleado_id = src.empleado_id
                WHEN MATCHED THEN UPDATE SET
                    sueldo_base=@SueldoBase, asignacion_familiar=@AsignacionFamiliar,
                    horas_extras=@HorasExtras, movilidad=@Movilidad, refrigerio=@Refrigerio,
                    bonif_desempenio=@BonifDesempenio, otros_ingresos=@OtrosIngresos,
                    total_bruto=@TotalBruto, descuento_afp_onp=@DescuentoAfpOnp,
                    comision_afp=@ComisionAfp, seguro_afp=@SeguroAfp,
                    essalud_trabajador=@EssaludTrabajador, renta_5ta_categoria=@Renta5taCategoria,
                    sctr=@Sctr, prestamos=@Prestamos, adelantos=@Adelantos,
                    tardanzas_faltas=@TardanzasFaltas, otros_descuentos=@OtrosDescuentos,
                    total_descuentos=@TotalDescuentos, essalud_empleador=@EssaludEmpleador,
                    sctr_empleador=@SctrEmpleador, total_neto=@TotalNeto, estado=@Estado
                WHEN NOT MATCHED THEN INSERT
                    (codigo_planilla, empleado_id, periodo,
                     sueldo_base, asignacion_familiar, horas_extras, movilidad, refrigerio,
                     bonif_desempenio, otros_ingresos, total_bruto,
                     descuento_afp_onp, comision_afp, seguro_afp,
                     essalud_trabajador, renta_5ta_categoria, sctr,
                     prestamos, adelantos, tardanzas_faltas, otros_descuentos, total_descuentos,
                     essalud_empleador, sctr_empleador, total_neto, estado)
                VALUES
                    (@CodigoPlanilla, @EmpleadoId, @Periodo,
                     @SueldoBase, @AsignacionFamiliar, @HorasExtras, @Movilidad, @Refrigerio,
                     @BonifDesempenio, @OtrosIngresos, @TotalBruto,
                     @DescuentoAfpOnp, @ComisionAfp, @SeguroAfp,
                     @EssaludTrabajador, @Renta5taCategoria, @Sctr,
                     @Prestamos, @Adelantos, @TardanzasFaltas, @OtrosDescuentos, @TotalDescuentos,
                     @EssaludEmpleador, @SctrEmpleador, @TotalNeto, @Estado);";
            using var db = Open();
            db.Execute(sql, d);
        }

        // ══════════════════════════════════════════════════════════════
        // CONCEPTOS
        // ══════════════════════════════════════════════════════════════

        public List<ConceptoNomina> ObtenerConceptos(string buscar = "", string tipo = "", string estado = "")
        {
            const string sql = @"
                SELECT id, codigo, nombre, tipo, afecta_calculo, es_remunerativo, activo, fecha_creacion
                FROM nomina.conceptos
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR nombre LIKE '%'+@buscar+'%')
                  AND (@tipo='' OR tipo=@tipo)
                  AND (@estado='' OR CASE WHEN activo=1 THEN 'Activo' ELSE 'Inactivo' END = @estado)
                ORDER BY codigo";
            using var db = Open();
            return db.Query<ConceptoRow>(sql, new { buscar, tipo, estado })
                     .Select(r => new ConceptoNomina
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         Nombre = r.nombre,
                         Tipo = Enum.Parse<TipoConcepto>(r.tipo),
                         AfectaCalculo = r.afecta_calculo,
                         EsRemunerativo = r.es_remunerativo,
                         Activo = r.activo,
                         FechaCreacion = r.fecha_creacion
                     }).ToList();
        }

        public void InsertarConcepto(ConceptoNomina c)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.conceptos (codigo,nombre,tipo,afecta_calculo,es_remunerativo,activo,fecha_creacion)
                         VALUES (@Codigo,@Nombre,@Tipo,@AfectaCalculo,@EsRemunerativo,@Activo,@FechaCreacion)",
                new { c.Codigo, c.Nombre, Tipo = c.Tipo.ToString(), c.AfectaCalculo, c.EsRemunerativo, c.Activo, c.FechaCreacion });
        }

        public void ActualizarConcepto(ConceptoNomina c)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.conceptos SET nombre=@Nombre,tipo=@Tipo,afecta_calculo=@AfectaCalculo,
                         es_remunerativo=@EsRemunerativo,activo=@Activo WHERE id=@Id",
                new { c.Id, c.Nombre, Tipo = c.Tipo.ToString(), c.AfectaCalculo, c.EsRemunerativo, c.Activo });
        }

        public void EliminarConcepto(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.conceptos WHERE id=@id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // DESCUENTOS
        // ══════════════════════════════════════════════════════════════

        public List<Descuento> ObtenerDescuentos(string buscar = "", string tipo = "", string estado = "")
        {
            const string sql = @"
                SELECT id          AS Id,
                       codigo      AS Codigo,
                       nombre      AS Nombre,
                       tipo        AS Tipo,
                       obligatorio AS Obligatorio,
                       afecta_neto AS AfectaNeto,
                       porcentaje  AS Porcentaje,
                       activo      AS Activo
                FROM nomina.descuentos
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR nombre LIKE '%'+@buscar+'%')
                  AND (@tipo='' OR tipo=@tipo)
                  AND (@estado='' OR CASE WHEN activo=1 THEN 'Activo' ELSE 'Inactivo' END = @estado)
                ORDER BY codigo";
            using var db = Open();
            return db.Query<Descuento>(sql, new { buscar, tipo, estado }).ToList();
        }

        public void InsertarDescuento(Descuento d)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.descuentos (codigo,nombre,tipo,obligatorio,afecta_neto,porcentaje,activo)
                         VALUES (@Codigo,@Nombre,@Tipo,@Obligatorio,@AfectaNeto,@Porcentaje,@Activo)", d);
        }

        public void ActualizarDescuento(Descuento d)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.descuentos SET nombre=@Nombre,tipo=@Tipo,obligatorio=@Obligatorio,
                         afecta_neto=@AfectaNeto,porcentaje=@Porcentaje,activo=@Activo WHERE id=@Id", d);
        }

        public void EliminarDescuento(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.descuentos WHERE id=@id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // BENEFICIOS
        // ══════════════════════════════════════════════════════════════

        public List<Beneficio> ObtenerBeneficios(string buscar = "", string categoria = "", string tipo = "")
        {
            const string sql = @"
                SELECT id, codigo, nombre, categoria, tipo, periodicidad,
                       monto_cadena, monto_fijo, activo, fecha_creacion
                FROM nomina.beneficios
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR nombre LIKE '%'+@buscar+'%')
                  AND (@categoria='' OR categoria=@categoria)
                  AND (@tipo='' OR tipo=@tipo)
                ORDER BY codigo";
            using var db = Open();
            return db.Query<BeneficioRow>(sql, new { buscar, categoria, tipo })
                     .Select(r => new Beneficio
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         Nombre = r.nombre,
                         Categoria = Enum.Parse<CategoriaBeneficio>(r.categoria),
                         Tipo = Enum.Parse<TipoBeneficio>(r.tipo),
                         Periodicidad = Enum.Parse<Periodicidad>(r.periodicidad),
                         MontoCadena = r.monto_cadena ?? "",
                         MontoFijo = r.monto_fijo,
                         Activo = r.activo,
                         FechaCreacion = r.fecha_creacion
                     }).ToList();
        }

        public void InsertarBeneficio(Beneficio b)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.beneficios (codigo,nombre,categoria,tipo,periodicidad,monto_cadena,monto_fijo,activo,fecha_creacion)
                         VALUES (@Codigo,@Nombre,@Categoria,@Tipo,@Periodicidad,@MontoCadena,@MontoFijo,@Activo,@FechaCreacion)",
                new
                {
                    b.Codigo,
                    b.Nombre,
                    Categoria = b.Categoria.ToString(),
                    Tipo = b.Tipo.ToString(),
                    Periodicidad = b.Periodicidad.ToString(),
                    b.MontoCadena,
                    b.MontoFijo,
                    b.Activo,
                    b.FechaCreacion
                });
        }

        public void ActualizarBeneficio(Beneficio b)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.beneficios SET nombre=@Nombre,categoria=@Categoria,tipo=@Tipo,
                         periodicidad=@Periodicidad,monto_cadena=@MontoCadena,monto_fijo=@MontoFijo,activo=@Activo WHERE id=@Id",
                new
                {
                    b.Id,
                    b.Nombre,
                    Categoria = b.Categoria.ToString(),
                    Tipo = b.Tipo.ToString(),
                    Periodicidad = b.Periodicidad.ToString(),
                    b.MontoCadena,
                    b.MontoFijo,
                    b.Activo
                });
        }

        public void EliminarBeneficio(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.beneficios WHERE id=@id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // GRATIFICACIONES
        // ══════════════════════════════════════════════════════════════

        public List<Gratificacion> ObtenerGratificaciones(string buscar = "", string tipo = "", string estadoFilt = "")
        {
            const string sql = @"
                SELECT id, codigo, nombre, tipo, periodo, frecuencia,
                       porcentaje_monto, monto_fijo, porcentaje, base_calculo,
                       fecha_estimada, fecha_pago, estado,
                       empleados_aplica, cantidad_empleados, creado_por, fecha_creacion
                FROM nomina.gratificaciones
                WHERE (@buscar='' OR nombre LIKE '%'+@buscar+'%' OR codigo LIKE '%'+@buscar+'%')
                  AND (@tipo='' OR tipo=@tipo)
                  AND (@estadoFilt='' OR estado=@estadoFilt)
                ORDER BY fecha_creacion DESC";
            using var db = Open();
            return db.Query<GratificacionRow>(sql, new { buscar, tipo, estadoFilt })
                     .Select(r => new Gratificacion
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         Nombre = r.nombre,
                         Tipo = Enum.Parse<TipoGratificacion>(r.tipo),
                         Periodo = r.periodo ?? "",
                         Frecuencia = Enum.Parse<FrecuenciaGratificacion>(r.frecuencia),
                         PorcentajeMonto = r.porcentaje_monto ?? "",
                         MontoFijo = r.monto_fijo,
                         Porcentaje = r.porcentaje,
                         BaseDeCalculo = Enum.Parse<BaseCalculo>(r.base_calculo),
                         FechaEstimada = r.fecha_estimada,
                         FechaPago = r.fecha_pago,
                         Estado = Enum.Parse<EstadoGratificacion>(r.estado),
                         EmpleadosAplica = r.empleados_aplica,
                         CantidadEmpleados = r.cantidad_empleados,
                         CreadoPor = r.creado_por,
                         FechaCreacion = r.fecha_creacion
                     }).ToList();
        }

        public void InsertarGratificacion(Gratificacion g)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.gratificaciones
                (codigo,nombre,tipo,periodo,frecuencia,porcentaje_monto,monto_fijo,porcentaje,
                 base_calculo,fecha_estimada,fecha_pago,estado,empleados_aplica,cantidad_empleados,creado_por,fecha_creacion)
                VALUES (@Codigo,@Nombre,@Tipo,@Periodo,@Frecuencia,@PorcentajeMonto,@MontoFijo,@Porcentaje,
                        @BaseDeCalculo,@FechaEstimada,@FechaPago,@Estado,@EmpleadosAplica,@CantidadEmpleados,@CreadoPor,@FechaCreacion)",
                new
                {
                    g.Codigo,
                    g.Nombre,
                    Tipo = g.Tipo.ToString(),
                    g.Periodo,
                    Frecuencia = g.Frecuencia.ToString(),
                    g.PorcentajeMonto,
                    g.MontoFijo,
                    g.Porcentaje,
                    BaseDeCalculo = g.BaseDeCalculo.ToString(),
                    g.FechaEstimada,
                    g.FechaPago,
                    Estado = g.Estado.ToString(),
                    g.EmpleadosAplica,
                    g.CantidadEmpleados,
                    g.CreadoPor,
                    g.FechaCreacion
                });
        }

        public void ActualizarGratificacion(Gratificacion g)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.gratificaciones SET
                nombre=@Nombre,tipo=@Tipo,periodo=@Periodo,frecuencia=@Frecuencia,
                porcentaje_monto=@PorcentajeMonto,monto_fijo=@MontoFijo,porcentaje=@Porcentaje,
                base_calculo=@BaseDeCalculo,fecha_estimada=@FechaEstimada,fecha_pago=@FechaPago,
                estado=@Estado,empleados_aplica=@EmpleadosAplica,cantidad_empleados=@CantidadEmpleados
                WHERE id=@Id",
                new
                {
                    g.Id,
                    g.Nombre,
                    Tipo = g.Tipo.ToString(),
                    g.Periodo,
                    Frecuencia = g.Frecuencia.ToString(),
                    g.PorcentajeMonto,
                    g.MontoFijo,
                    g.Porcentaje,
                    BaseDeCalculo = g.BaseDeCalculo.ToString(),
                    g.FechaEstimada,
                    g.FechaPago,
                    Estado = g.Estado.ToString(),
                    g.EmpleadosAplica,
                    g.CantidadEmpleados
                });
        }

        public void EliminarGratificacion(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.gratificaciones WHERE id=@id", new { id });
        }
        public List<Feriado> ObtenerFeriados()
        {
            using var db = Open();
            return db.Query<dynamic>("SELECT * FROM configuracion.feriados ORDER BY fecha")
                .Select(r => new Feriado
                {
                    Id = r.id,
                    Fecha = r.fecha,
                    Nombre = r.nombre,
                    Tipo = r.tipo,
                    Recuperable = r.recuperable,
                    Activo = r.activo
                }).ToList();
        }

        public List<CentroCosto> ObtenerCentros()
        {
            using var db = Open();
            return db.Query<dynamic>("SELECT * FROM configuracion.centros_costo ORDER BY nombre")
                .Select(r => new CentroCosto
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    Nombre = r.nombre,
                    Descripcion = r.descripcion ?? "",
                    Responsable = r.responsable ?? "",
                    Activo = r.activo
                }).ToList();
        }
        public List<UsuarioNomina> ObtenerUsuariosNom()
        {
            using var db = Open();
            return db.Query<dynamic>("SELECT * FROM seguridad.usuarios_nomina ORDER BY nombre")
                .Select(r => new UsuarioNomina
                {
                    Id = r.id,
                    Usuario = r.usuario,
                    Nombre = r.nombre,
                    Rol = r.rol,
                    Email = r.email,
                    Activo = r.activo,
                    Emoji = r.emoji
                }).ToList();
        }

        public void InsertarUsuarioNom(UsuarioNomina u)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO seguridad.usuarios_nomina (usuario, nombre, rol, email, activo, emoji)
        VALUES (@Usuario, @Nombre, @Rol, @Email, @Activo, @Emoji)",
                new { u.Usuario, u.Nombre, u.Rol, u.Email, Activo = u.Activo ? 1 : 0, u.Emoji });
        }

        public void ActualizarUsuarioNom(UsuarioNomina u)
        {
            using var db = Open();
            db.Execute(@"UPDATE seguridad.usuarios_nomina SET
        usuario=@Usuario, nombre=@Nombre, rol=@Rol, email=@Email, activo=@Activo, emoji=@Emoji
        WHERE id=@Id",
                new { u.Id, u.Usuario, u.Nombre, u.Rol, u.Email, Activo = u.Activo ? 1 : 0, u.Emoji });
        }

        public void EliminarUsuarioNom(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM seguridad.usuarios_nomina WHERE id=@id", new { id });
        }
        public void InsertarCentro(CentroCosto c)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO configuracion.centros_costo (codigo, nombre, descripcion, responsable, activo)
        VALUES (@Codigo, @Nombre, @Descripcion, @Responsable, @Activo)",
                new { c.Codigo, c.Nombre, c.Descripcion, c.Responsable, Activo = c.Activo ? 1 : 0 });
        }

        public void ActualizarCentro(CentroCosto c)
        {
            using var db = Open();
            db.Execute(@"UPDATE configuracion.centros_costo SET
        codigo=@Codigo, nombre=@Nombre, descripcion=@Descripcion, responsable=@Responsable, activo=@Activo
        WHERE id=@Id",
                new { c.Id, c.Codigo, c.Nombre, c.Descripcion, c.Responsable, Activo = c.Activo ? 1 : 0 });
        }

        public void EliminarCentro(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM configuracion.centros_costo WHERE id=@id", new { id });
        }
        public void InsertarFeriado(Feriado f)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO configuracion.feriados (fecha, nombre, tipo, recuperable, activo)
        VALUES (@Fecha, @Nombre, @Tipo, @Recuperable, @Activo)",
                new { f.Fecha, f.Nombre, f.Tipo, Recuperable = f.Recuperable ? 1 : 0, Activo = f.Activo ? 1 : 0 });
        }

        public void ActualizarFeriado(Feriado f)
        {
            using var db = Open();
            db.Execute(@"UPDATE configuracion.feriados SET
        fecha=@Fecha, nombre=@Nombre, tipo=@Tipo, recuperable=@Recuperable, activo=@Activo
        WHERE id=@Id",
                new { f.Id, f.Fecha, f.Nombre, f.Tipo, Recuperable = f.Recuperable ? 1 : 0, Activo = f.Activo ? 1 : 0 });
        }

        public void EliminarFeriado(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM configuracion.feriados WHERE id=@id", new { id });
        }
        // ══════════════════════════════════════════════════════════════
        // PAGOS PLANILLA (Historial de pagos)
        // ══════════════════════════════════════════════════════════════

        public List<PagoPlanilla> ObtenerPagos(string buscar = "", string banco = "", string estadoPago = "")
        {
            const string sql = @"
                SELECT id, codigo, planilla_concepto, periodo, fecha_pago,
                       banco, monto_pagado, estado, observacion, empleados
                FROM nomina.pagos_planilla
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR periodo LIKE '%'+@buscar+'%')
                  AND (@banco='' OR banco=@banco)
                  AND (@estadoPago='' OR estado=@estadoPago)
                ORDER BY fecha_pago DESC";
            using var db = Open();
            return db.Query<PagoRow>(sql, new { buscar, banco, estadoPago })
                     .Select(r => new PagoPlanilla
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         PlanillaConcepto = r.planilla_concepto ?? "",
                         Periodo = r.periodo,
                         FechaPago = r.fecha_pago,
                         Banco = Enum.Parse<MedioPago>(r.banco),
                         MontoPagado = r.monto_pagado,
                         Estado = Enum.Parse<EstadoPago>(r.estado),
                         Observacion = r.observacion ?? "",
                         Empleados = r.empleados
                     }).ToList();
        }

        public void InsertarPago(PagoPlanilla p)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.pagos_planilla (codigo,planilla_concepto,periodo,fecha_pago,banco,monto_pagado,estado,observacion,empleados)
                         VALUES (@Codigo,@PlanillaConcepto,@Periodo,@FechaPago,@Banco,@MontoPagado,@Estado,@Observacion,@Empleados)",
                new
                {
                    p.Codigo,
                    p.PlanillaConcepto,
                    p.Periodo,
                    p.FechaPago,
                    Banco = p.Banco.ToString(),
                    p.MontoPagado,
                    Estado = p.Estado.ToString(),
                    p.Observacion,
                    p.Empleados
                });
        }
        public List<BancoConfig> ObtenerBancos()
        {
            using var db = Open();
            return db.Query<dynamic>("SELECT * FROM configuracion.bancos ORDER BY nombre")
                .Select(r => new BancoConfig
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    Nombre = r.nombre,
                    Moneda = r.moneda,
                    CuentaPrincipal = r.cuenta_principal ?? "",
                    Activo = r.activo,
                    Emoji = r.emoji
                }).ToList();
        }

        public void InsertarBanco(BancoConfig b)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO configuracion.bancos (codigo, nombre, moneda, cuenta_principal, activo, emoji)
        VALUES (@Codigo, @Nombre, @Moneda, @CuentaPrincipal, @Activo, @Emoji)",
                new { b.Codigo, b.Nombre, b.Moneda, b.CuentaPrincipal, Activo = b.Activo ? 1 : 0, b.Emoji });
        }

        public void ActualizarBanco(BancoConfig b)
        {
            using var db = Open();
            db.Execute(@"UPDATE configuracion.bancos SET
        nombre=@Nombre, moneda=@Moneda, cuenta_principal=@CuentaPrincipal, activo=@Activo, emoji=@Emoji
        WHERE id=@Id",
                new { b.Id, b.Nombre, b.Moneda, b.CuentaPrincipal, Activo = b.Activo ? 1 : 0, b.Emoji });
        }

        public void EliminarBanco(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM configuracion.bancos WHERE id=@id", new { id });
        }
        public List<RangoRenta> ObtenerRangos()
        {
            using var db = Open();
            return db.Query<dynamic>("SELECT * FROM configuracion.rangos_renta ORDER BY desde")
                .Select(r => new RangoRenta
                {
                    Id = r.id,
                    Desde = r.desde,
                    Hasta = r.hasta,
                    Tasa = r.tasa,
                    MontoFijo = r.monto_fijo,
                    Activo = r.activo
                }).ToList();
        }

        public void InsertarRango(RangoRenta r)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO configuracion.rangos_renta (desde, hasta, tasa, monto_fijo, activo)
        VALUES (@Desde, @Hasta, @Tasa, @MontoFijo, @Activo)",
                new { r.Desde, r.Hasta, r.Tasa, r.MontoFijo, Activo = r.Activo ? 1 : 0 });
        }

        public void ActualizarRango(RangoRenta r)
        {
            using var db = Open();
            db.Execute(@"UPDATE configuracion.rangos_renta SET
        desde=@Desde, hasta=@Hasta, tasa=@Tasa, monto_fijo=@MontoFijo, activo=@Activo
        WHERE id=@Id",
                new { r.Id, r.Desde, r.Hasta, r.Tasa, r.MontoFijo, Activo = r.Activo ? 1 : 0 });
        }

        public void EliminarRango(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM configuracion.rangos_renta WHERE id=@id", new { id });
        }
        public ParametrosGenerales ObtenerParametros()
        {
            using var db = Open();
            var r = db.QueryFirstOrDefault<dynamic>(
                "SELECT * FROM configuracion.parametros_generales WHERE id=1");
            if (r == null) return new ParametrosGenerales();
            return new ParametrosGenerales
            {
                Empresa = r.empresa,
                Moneda = r.moneda,
                DiaCierrePlanilla = r.dia_cierre_planilla,
                DiaPagoPlanilla = r.dia_pago_planilla,
                CalcHorasExtrasAuto = r.calc_horas_extras_auto,
                InclFeriadosAsist = r.incl_feriados_asist
            };
        }

        public void GuardarParametros(ParametrosGenerales p)
        {
            using var db = Open();
            db.Execute(@"UPDATE configuracion.parametros_generales SET
        empresa=@Empresa, moneda=@Moneda,
        dia_cierre_planilla=@DiaCierrePlanilla,
        dia_pago_planilla=@DiaPagoPlanilla,
        calc_horas_extras_auto=@CalcHorasExtrasAuto,
        incl_feriados_asist=@InclFeriadosAsist
        WHERE id=1",
                new
                {
                    p.Empresa,
                    p.Moneda,
                    p.DiaCierrePlanilla,
                    p.DiaPagoPlanilla,
                    CalcHorasExtrasAuto = p.CalcHorasExtrasAuto ? 1 : 0,
                    InclFeriadosAsist = p.InclFeriadosAsist ? 1 : 0
                });
        }
        public List<Reporte> ObtenerReportes()
        {
            using var db = Open();
            return db.Query<dynamic>(@"
        SELECT id, codigo, nombre, submodulo, periodo, fecha_generacion,
               generado_por, estado, formato, filas_generadas, tamano_kb
        FROM configuracion.reportes
        ORDER BY fecha_generacion DESC")
                .Select(r => new Reporte
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    Nombre = r.nombre,
                    Submodulo = r.submodulo ?? "",
                    Periodo = r.periodo ?? "",
                    FechaGeneracion = r.fecha_generacion,
                    GeneradoPor = r.generado_por,
                    Estado = r.estado,
                    Formato = r.formato,
                    FilasGeneradas = r.filas_generadas,
                    TamañoKb = r.tamano_kb
                }).ToList();
        }

        public void InsertarReporte(Reporte r)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO configuracion.reportes
        (codigo, nombre, submodulo, periodo, fecha_generacion, generado_por,
         estado, formato, filas_generadas, tamano_kb)
        VALUES (@Codigo, @Nombre, @Submodulo, @Periodo, @FechaGeneracion, @GeneradoPor,
                @Estado, @Formato, @FilasGeneradas, @TamañoKb)",
                new
                {
                    r.Codigo,
                    r.Nombre,
                    r.Submodulo,
                    r.Periodo,
                    r.FechaGeneracion,
                    r.GeneradoPor,
                    r.Estado,
                    r.Formato,
                    r.FilasGeneradas,
                    r.TamañoKb
                });
        }
        public void EliminarReporte(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM configuracion.reportes WHERE id = @id", new { id });
        }
        public void ActualizarPago(PagoPlanilla p)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.pagos_planilla SET
        planilla_concepto=@PlanillaConcepto, periodo=@Periodo, fecha_pago=@FechaPago,
        banco=@Banco, monto_pagado=@MontoPagado, estado=@Estado,
        observacion=@Observacion, empleados=@Empleados
        WHERE id=@Id",
                new
                {
                    p.Id,
                    p.PlanillaConcepto,
                    p.Periodo,
                    p.FechaPago,
                    Banco = p.Banco.ToString(),
                    p.MontoPagado,
                    Estado = p.Estado.ToString(),
                    p.Observacion,
                    p.Empleados
                });
        }

        public void EliminarPago(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.pagos_planilla WHERE id = @id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // UTILIDADES
        // ══════════════════════════════════════════════════════════════

        public List<Utilidad> ObtenerUtilidades(string buscar = "", string estadoFilt = "")
        {
            const string sql = @"
                SELECT id, codigo, ejercicio_fiscal, porcentaje_participacion,
                       utilidad_neta_declarada, dias_computables, remuneracion_computable,
                       monto_distribuido, fecha_pago_estimada, fecha_pago_real,
                       estado, empleados_aplica, cantidad_empleados, observacion, fecha_creacion
                FROM nomina.utilidades
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR CAST(ejercicio_fiscal AS VARCHAR)=@buscar)
                  AND (@estadoFilt='' OR estado=@estadoFilt)
                ORDER BY ejercicio_fiscal DESC";
            using var db = Open();
            return db.Query<UtilidadRow>(sql, new { buscar, estadoFilt })
                     .Select(r => new Utilidad
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         EjercicioFiscal = r.ejercicio_fiscal,
                         PorcentajeParticipacion = r.porcentaje_participacion,
                         UtilidadNetaDeclarada = r.utilidad_neta_declarada,
                         DiasComputables = r.dias_computables,
                         RemuneracionComputable = r.remuneracion_computable,
                         MontoDistribuido = r.monto_distribuido,
                         FechaPagoEstimada = r.fecha_pago_estimada,
                         FechaPagoReal = r.fecha_pago_real,
                         Estado = Enum.Parse<EstadoUtilidad>(r.estado),
                         EmpleadosAplica = r.empleados_aplica,
                         CantidadEmpleados = r.cantidad_empleados,
                         Observacion = r.observacion ?? "",
                         FechaCreacion = r.fecha_creacion
                     }).ToList();
        }

        public void EliminarDeclaracionSunat(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.declaraciones_sunat WHERE id = @id", new { id });
        }
        public void InsertarUtilidad(Utilidad u)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.utilidades
                (codigo,ejercicio_fiscal,porcentaje_participacion,utilidad_neta_declarada,
                 dias_computables,remuneracion_computable,fecha_pago_estimada,estado,
                 empleados_aplica,cantidad_empleados,observacion,fecha_creacion)
                VALUES (@Codigo,@EjercicioFiscal,@PorcentajeParticipacion,@UtilidadNetaDeclarada,
                        @DiasComputables,@RemuneracionComputable,@FechaPagoEstimada,@Estado,
                        @EmpleadosAplica,@CantidadEmpleados,@Observacion,@FechaCreacion)",
                new
                {
                    u.Codigo,
                    u.EjercicioFiscal,
                    u.PorcentajeParticipacion,
                    u.UtilidadNetaDeclarada,
                    u.DiasComputables,
                    u.RemuneracionComputable,
                    u.FechaPagoEstimada,
                    Estado = u.Estado.ToString(),
                    u.EmpleadosAplica,
                    u.CantidadEmpleados,
                    u.Observacion,
                    u.FechaCreacion
                });
        }
        public void ActualizarUtilidad(Utilidad u)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.utilidades SET
        ejercicio_fiscal          = @EjercicioFiscal,
        porcentaje_participacion  = @PorcentajeParticipacion,
        utilidad_neta_declarada   = @UtilidadNetaDeclarada,
        dias_computables          = @DiasComputables,
        remuneracion_computable   = @RemuneracionComputable,
        monto_distribuido         = @MontoDistribuido,
        fecha_pago_estimada       = @FechaPagoEstimada,
        estado                    = @Estado,
        empleados_aplica          = @EmpleadosAplica,
        cantidad_empleados        = @CantidadEmpleados,
        observacion               = @Observacion
        WHERE id = @Id",
                new
                {
                    u.Id,
                    u.EjercicioFiscal,
                    u.PorcentajeParticipacion,
                    u.UtilidadNetaDeclarada,
                    u.DiasComputables,
                    u.RemuneracionComputable,
                    u.MontoDistribuido,
                    u.FechaPagoEstimada,
                    Estado = u.Estado.ToString(),
                    u.EmpleadosAplica,
                    u.CantidadEmpleados,
                    u.Observacion
                });
        }

        public void EliminarUtilidad(int id)
        {
            using var db = Open();
            db.Execute("DELETE FROM nomina.utilidades WHERE id=@id", new { id });
        }

        // ══════════════════════════════════════════════════════════════
        // SUNAT – Declaraciones
        // ══════════════════════════════════════════════════════════════
        public List<DeclaracionSunat> ObtenerDeclaracionesSunat()
        {
            using var db = Open();
            return db.Query<dynamic>(@"
        SELECT id, codigo, tipo, periodo, ejercicio, fecha_generacion,
               fecha_envio, estado, nro_orden, tiene_constancia, usuario, observacion
        FROM nomina.declaraciones_sunat
        ORDER BY fecha_generacion DESC")
                .Select(r => new DeclaracionSunat
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    Tipo = Enum.Parse<TipoPdt>((string)r.tipo),
                    Periodo = r.periodo,
                    Ejercicio = r.ejercicio,
                    FechaGeneracion = r.fecha_generacion,
                    FechaEnvio = r.fecha_envio,
                    Estado = Enum.Parse<EstadoPdt>((string)r.estado),
                    NroOrden = r.nro_orden ?? "",
                    TieneConstancia = (bool)r.tiene_constancia,
                    Usuario = r.usuario,
                    Observacion = r.observacion ?? ""
                }).ToList();
        }

        public void InsertarDeclaracionSunat(DeclaracionSunat d)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.declaraciones_sunat
        (codigo, tipo, periodo, ejercicio, fecha_generacion, fecha_envio,
         estado, nro_orden, tiene_constancia, usuario, observacion)
        VALUES (@Codigo, @Tipo, @Periodo, @Ejercicio, @FechaGeneracion, @FechaEnvio,
                @Estado, @NroOrden, @TieneConstancia, @Usuario, @Observacion)",
                new
                {
                    d.Codigo,
                    Tipo = d.Tipo.ToString(),
                    d.Periodo,
                    d.Ejercicio,
                    d.FechaGeneracion,
                    FechaEnvio = d.FechaEnvio.HasValue ? (object)d.FechaEnvio.Value : DBNull.Value,
                    Estado = d.Estado.ToString(),
                    NroOrden = d.NroOrden,
                    TieneConstancia = d.TieneConstancia ? 1 : 0,
                    d.Usuario,
                    d.Observacion
                });
        }

        public void ActualizarDeclaracionSunat(DeclaracionSunat d)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.declaraciones_sunat SET
        tipo=@Tipo, periodo=@Periodo, ejercicio=@Ejercicio,
        fecha_generacion=@FechaGeneracion, fecha_envio=@FechaEnvio,
        estado=@Estado, nro_orden=@NroOrden, tiene_constancia=@TieneConstancia,
        usuario=@Usuario, observacion=@Observacion
        WHERE id=@Id",
                new
                {
                    d.Id,
                    Tipo = d.Tipo.ToString(),
                    d.Periodo,
                    d.Ejercicio,
                    d.FechaGeneracion,
                    FechaEnvio = d.FechaEnvio.HasValue ? (object)d.FechaEnvio.Value : DBNull.Value,
                    Estado = d.Estado.ToString(),
                    NroOrden = d.NroOrden,
                    TieneConstancia = d.TieneConstancia ? 1 : 0,
                    d.Usuario,
                    d.Observacion
                });
        }

        // ══════════════════════════════════════════════════════════════
        // ESSALUD – Declaraciones
        // ══════════════════════════════════════════════════════════════

        public List<DeclaracionEsSalud> ObtenerDeclaraciones(string buscar = "", string estadoFilt = "", string tipoFilt = "", string periodo = "")
        {
            const string sql = @"
                SELECT id, codigo, periodo, trabajadores, remuneracion_asignable,
                       aporte_essalud, subsidios, total_pagar, fecha_envio,
                       estado, tipo_declaracion, nro_orden_sunat, observacion
                FROM nomina.declaraciones_essalud
                WHERE (@buscar='' OR codigo LIKE '%'+@buscar+'%' OR periodo LIKE '%'+@buscar+'%')
                  AND (@estadoFilt='' OR estado=@estadoFilt)
                  AND (@tipoFilt='' OR tipo_declaracion=@tipoFilt)
                  AND (@periodo='' OR periodo LIKE '%'+@periodo+'%')
                ORDER BY fecha_envio DESC";
            using var db = Open();
            return db.Query<DeclaracionRow>(sql, new { buscar, estadoFilt, tipoFilt, periodo })
                     .Select(r => new DeclaracionEsSalud
                     {
                         Id = r.id,
                         Codigo = r.codigo,
                         Periodo = r.periodo,
                         Trabajadores = r.trabajadores,
                         RemuneracionAsignable = r.remuneracion_asignable,
                         AporteEsSalud = r.aporte_essalud,
                         Subsidios = r.subsidios,
                         TotalPagar = r.total_pagar,
                         FechaEnvio = r.fecha_envio ?? DateTime.MinValue,
                         Estado = Enum.Parse<EstadoDeclaracion>(r.estado),
                         TipoDeclaracion = Enum.Parse<TipoDeclaracion>(r.tipo_declaracion),
                         NroOrdenSunat = r.nro_orden_sunat ?? "",
                         Observacion = r.observacion
                     }).ToList();
        }

        public void InsertarDeclaracion(DeclaracionEsSalud d)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.declaraciones_essalud
                (codigo,periodo,trabajadores,remuneracion_asignable,aporte_essalud,
                 subsidios,total_pagar,fecha_envio,estado,tipo_declaracion,nro_orden_sunat,observacion)
                VALUES (@Codigo,@Periodo,@Trabajadores,@RemuneracionAsignable,@AporteEsSalud,
                        @Subsidios,@TotalPagar,@FechaEnvio,@Estado,@TipoDeclaracion,@NroOrdenSunat,@Observacion)",
                new
                {
                    d.Codigo,
                    d.Periodo,
                    d.Trabajadores,
                    d.RemuneracionAsignable,
                    d.AporteEsSalud,
                    d.Subsidios,
                    d.TotalPagar,
                    FechaEnvio = (DateTime?)d.FechaEnvio,
                    Estado = d.Estado.ToString(),
                    TipoDeclaracion = d.TipoDeclaracion.ToString(),
                    d.NroOrdenSunat,
                    d.Observacion
                });
        }

        public void ActualizarDeclaracion(DeclaracionEsSalud d)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.declaraciones_essalud SET
                estado=@Estado, fecha_envio=@FechaEnvio, nro_orden_sunat=@NroOrdenSunat,
                observacion=@Observacion WHERE id=@Id",
                new
                {
                    d.Id,
                    Estado = d.Estado.ToString(),
                    FechaEnvio = (DateTime?)d.FechaEnvio,
                    d.NroOrdenSunat,
                    d.Observacion
                });
        }

        // ══════════════════════════════════════════════════════════════
        // ESSALUD – SCTR
        // ══════════════════════════════════════════════════════════════

        public List<GrupoSctr> ObtenerGruposSctr()
        {
            using var db = Open();
            return db.Query<GrupoSctrRow>(
                "SELECT id, nivel_riesgo, trabajadores, sctr_salud, sctr_pension, aseguradora, activo FROM nomina.grupos_sctr ORDER BY id")
                .Select(r => new GrupoSctr
                {
                    Id = r.id,
                    NivelRiesgo = Enum.Parse<NivelRiesgoSCTR>(r.nivel_riesgo),
                    Trabajadores = r.trabajadores,
                    SctrSalud = r.sctr_salud,
                    SctrPension = r.sctr_pension,
                    Aseguradora = r.aseguradora,
                    Activo = r.activo
                }).ToList();
        }

        public void ActualizarGrupoSctr(GrupoSctr g)
        {
            using var db = Open();
            db.Execute(@"UPDATE nomina.grupos_sctr SET
                trabajadores=@Trabajadores, sctr_salud=@SctrSalud,
                sctr_pension=@SctrPension, aseguradora=@Aseguradora WHERE id=@Id",
                new { g.Id, g.Trabajadores, g.SctrSalud, g.SctrPension, g.Aseguradora });
        }

        // ══════════════════════════════════════════════════════════════
        // ESSALUD – Historial envíos
        // ══════════════════════════════════════════════════════════════

        public List<HistorialEnvioEsSalud> ObtenerHistorialEnvios()
        {
            using var db = Open();
            return db.Query<HistorialRow>(
                "SELECT id, fecha_hora, declaracion, usuario, estado, mensaje FROM nomina.historial_envios_essalud ORDER BY fecha_hora DESC")
                .Select(r => new HistorialEnvioEsSalud
                {
                    Id = r.id,
                    FechaHora = r.fecha_hora,
                    Declaracion = r.declaracion,
                    Usuario = r.usuario,
                    Estado = Enum.Parse<EstadoEnvio>(r.estado),
                    Mensaje = r.mensaje
                }).ToList();
        }

        public void InsertarHistorialEnvio(HistorialEnvioEsSalud h)
        {
            using var db = Open();
            db.Execute(@"INSERT INTO nomina.historial_envios_essalud (fecha_hora,declaracion,usuario,estado,mensaje)
                         VALUES (@FechaHora,@Declaracion,@Usuario,@Estado,@Mensaje)",
                new { h.FechaHora, h.Declaracion, h.Usuario, Estado = h.Estado.ToString(), h.Mensaje });
        }

        // ══════════════════════════════════════════════════════════════
        // VERIFICAR CONEXIÓN
        // ══════════════════════════════════════════════════════════════

        public bool VerificarConexion()
        {
            try { using var db = Open(); db.Open(); return true; }
            catch { return false; }
        }
        // ══════════════════════════════════════════════════════════════
        // DASHBOARD – Resumen agregado (una sola ida a BD)
        // ══════════════════════════════════════════════════════════════

        public DashboardResumen ObtenerResumenDashboard()
        {
            // Determinar mes actual y mes anterior
            var hoy = DateTime.Today;
            var mesActual = hoy.Month;
            var anioActual = hoy.Year;
            var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
            var anioAnterior = mesActual == 1 ? anioActual - 1 : anioActual;

            // Nombre de período en formato "MMMM yyyy" en español (ej: "Mayo 2026")
            // La columna periodo en BD usa ese formato libre, así que filtramos por año
            // y comparamos el campo fecha_registro para mes actual vs anterior.
            const string sql = @"
        -- ① Totales de planillas mes actual (por fecha_registro)
        SELECT
            ISNULL(SUM(total_neto),       0) AS TotalNetoActual,
            ISNULL(SUM(total_bruto),      0) AS TotalBrutoActual,
            ISNULL(SUM(total_descuentos), 0) AS DescuentosActual,
            ISNULL(SUM(empleados),        0) AS EmpleadosActual
        FROM nomina.planillas
        WHERE MONTH(fecha_registro) = @mesActual
          AND YEAR(fecha_registro)  = @anioActual
          AND estado <> 'Anulada';

        -- ② Totales planillas mes anterior
        SELECT
            ISNULL(SUM(total_neto), 0) AS TotalNetoAnterior
        FROM nomina.planillas
        WHERE MONTH(fecha_registro) = @mesAnterior
          AND YEAR(fecha_registro)  = @anioAnterior
          AND estado <> 'Anulada';

        -- ③ Conteo de estados de planillas (todas, sin filtro de mes)
        SELECT
            ISNULL(SUM(CASE WHEN estado IN ('Cerrada','Aprobada','Pagado','Pagada') THEN 1 ELSE 0 END), 0) AS Pagadas,
            ISNULL(SUM(CASE WHEN estado = 'EnCalculo'             THEN 1 ELSE 0 END), 0) AS EnProceso,
            ISNULL(SUM(CASE WHEN estado = 'Pendiente'             THEN 1 ELSE 0 END), 0) AS Pendientes,
            ISNULL(SUM(CASE WHEN estado = 'Anulada'               THEN 1 ELSE 0 END), 0) AS Anuladas
        FROM nomina.planillas;

        -- ④ Empleados activos totales
        SELECT COUNT(1) AS Total
        FROM nomina.empleados
        WHERE estado = 'Activo';

        -- ⑤ Empleados nuevos este mes (ingresaron en el mes actual)
        SELECT COUNT(1) AS Nuevos
        FROM nomina.empleados
        WHERE MONTH(fecha_ingreso) = @mesActual
          AND YEAR(fecha_ingreso)  = @anioActual;

        -- ⑥ Próximo pago: fecha_cierre más cercana en el futuro
        SELECT TOP 1 fecha_cierre AS ProximoPago
        FROM nomina.planillas
        WHERE fecha_cierre >= @hoy
          AND estado IN ('Pendiente','EnCalculo')
        ORDER BY fecha_cierre ASC;

-- ⑦ Últimas 5 planillas para la tabla del dashboard
        SELECT TOP 5
    codigo, periodo, empleados,
    total_bruto      AS TotalBruto,
    total_descuentos AS TotalDescuentos,
    total_neto       AS TotalNeto,
    estado, fecha_cierre
FROM nomina.planillas
ORDER BY fecha_registro DESC;

        -- ⑧ Empleados en vacaciones
        SELECT COUNT(1) AS EnVacaciones
        FROM nomina.empleados
        WHERE estado = 'Vacaciones';

        -- ⑨ Masa salarial (suma sueldos base empleados no inactivos)
        SELECT ISNULL(SUM(sueldo_base), 0) AS MasaSalarial
        FROM nomina.empleados
        WHERE estado <> 'Inactivo';
    ";

            using var db = Open();
            using var multi = db.QueryMultiple(sql, new
            {
                mesActual,
                anioActual,
                mesAnterior,
                anioAnterior,
                hoy
            });

            var actual = multi.ReadFirstOrDefault<dynamic>();
            var anterior = multi.ReadFirstOrDefault<dynamic>();
            var estados = multi.ReadFirstOrDefault<dynamic>();
            var empTotal = multi.ReadFirstOrDefault<dynamic>();
            var empNuevos = multi.ReadFirstOrDefault<dynamic>();
            var proximoPagoRow = multi.ReadFirstOrDefault<dynamic>();
            var ultimas = multi.Read<Planilla>().ToList();
            var empVacaciones = multi.ReadFirstOrDefault<dynamic>();
            var masaSalarialRow = multi.ReadFirstOrDefault<dynamic>();

            return new DashboardResumen
            {
                TotalNetoMesActual = actual != null ? (decimal)actual.TotalNetoActual : 0m,
                TotalBrutoMesActual = actual != null ? (decimal)actual.TotalBrutoActual : 0m,
                DescuentosMesActual = actual != null ? (decimal)actual.DescuentosActual : 0m,
                TotalNetoMesAnterior = anterior != null ? (decimal)anterior.TotalNetoAnterior : 0m,
                PlanillasPagadas = estados != null ? (int)estados.Pagadas : 0,
                PlanillasEnProceso = estados != null ? (int)estados.EnProceso : 0,
                PlanillasPendientes = estados != null ? (int)estados.Pendientes : 0,
                PlanillasAnuladas = estados != null ? (int)estados.Anuladas : 0,
                EmpleadosActivos = empTotal != null ? (int)empTotal.Total : 0,
                EmpleadosNuevosMes = empNuevos != null ? (int)empNuevos.Nuevos : 0,
                ProximoPago = proximoPagoRow != null
                                           ? (DateTime?)proximoPagoRow.ProximoPago
                                           : null,
                UltimasPlanillas = ultimas,
                EmpleadosEnVacaciones = empVacaciones != null ? (int)empVacaciones.EnVacaciones : 0,
                MasaSalarial = masaSalarialRow != null ? (decimal)masaSalarialRow.MasaSalarial : 0m,
            };
        }

        // ══════════════════════════════════════════════════════════════
        // FILAS INTERNAS (mapeo columnas SQL → propiedades)
        // ══════════════════════════════════════════════════════════════

        private static Empleado MapEmpleado(EmpleadoRow r) => new()
        {
            Id = r.id,
            Codigo = r.codigo,
            Nombres = r.nombres,
            ApellidoPaterno = r.apellido_paterno,
            ApellidoMaterno = r.apellido_materno,
            TipoDocumento = Enum.TryParse<TipoDocumento>(r.tipo_documento, out var td) ? td : TipoDocumento.DNI,
            NumeroDocumento = r.numero_documento,
            FechaNacimiento = r.fecha_nacimiento,
            Sexo = r.sexo ?? "M",
            Telefono = r.telefono ?? "",
            Email = r.email ?? "",
            Direccion = r.direccion ?? "",
            FechaIngreso = r.fecha_ingreso,
            FechaCese = r.fecha_cese,
            Cargo = r.cargo,
            Departamento = r.departamento,
            CentroCostoId = r.centro_costo_id,
            TipoContrato = Enum.TryParse<TipoContrato>(r.tipo_contrato, out var tc) ? tc : TipoContrato.Indeterminado,
            RegimeLaboral = Enum.TryParse<RegimeLaboralT>(r.regimen_laboral, out var rl) ? rl : RegimeLaboralT.Regimen728,
            Estado = Enum.TryParse<EstadoEmpleado>(r.estado, out var ee) ? ee : EstadoEmpleado.Activo,
            SueldoBase = r.sueldo_base,
            AsignacionFamiliar = r.asignacion_familiar,
            TieneHijos = r.tiene_hijos,
            SistemaPrevisional = Enum.TryParse<TipoAFP>(r.sistema_previsional, out var afp) ? afp : TipoAFP.ONP,
            CodigoAFP = r.codigo_afp,
            CUSPP = r.cuspp,
            BancoPago = Enum.TryParse<MedioPago>(r.banco_pago, out var bp) ? bp : MedioPago.BCP,
            NumeroCuenta = r.numero_cuenta ?? "",
            TipoCuenta = r.tipo_cuenta ?? "Ahorros",
            CCI = r.cci ?? "",
            AfectoRenta5ta = r.afecto_renta_5ta,
            AfectoEssalud = r.afecto_essalud
        };

        // ── Clases de fila (snake_case → camelCase Dapper) ─────────
        private class EmpleadoRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string nombres { get; set; } = "";
            public string apellido_paterno { get; set; } = ""; public string apellido_materno { get; set; } = "";
            public string tipo_documento { get; set; } = "DNI"; public string numero_documento { get; set; } = "";
            public DateTime fecha_nacimiento { get; set; }
            public string? sexo { get; set; }
            public string? telefono { get; set; }
            public string? email { get; set; }
            public string? direccion { get; set; }
            public DateTime fecha_ingreso { get; set; }
            public DateTime? fecha_cese { get; set; }
            public string cargo { get; set; } = ""; public string departamento { get; set; } = ""; public int centro_costo_id { get; set; }
            public string tipo_contrato { get; set; } = ""; public string regimen_laboral { get; set; } = ""; public string estado { get; set; } = "";
            public decimal sueldo_base { get; set; }
            public decimal asignacion_familiar { get; set; }
            public bool tiene_hijos { get; set; }
            public string sistema_previsional { get; set; } = "ONP"; public string? codigo_afp { get; set; }
            public string? cuspp { get; set; }
            public string banco_pago { get; set; } = "BCP"; public string? numero_cuenta { get; set; }
            public string? tipo_cuenta { get; set; }
            public string? cci { get; set; }
            public bool afecto_renta_5ta { get; set; }
            public bool afecto_essalud { get; set; }
        }
        private class ConceptoRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string nombre { get; set; } = "";
            public string tipo { get; set; } = ""; public bool afecta_calculo { get; set; }
            public bool es_remunerativo { get; set; }
            public bool activo { get; set; }
            public DateTime fecha_creacion { get; set; }
        }
        private class BeneficioRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string nombre { get; set; } = "";
            public string categoria { get; set; } = ""; public string tipo { get; set; } = ""; public string periodicidad { get; set; } = "";
            public string? monto_cadena { get; set; }
            public decimal? monto_fijo { get; set; }
            public bool activo { get; set; }
            public DateTime fecha_creacion { get; set; }
        }
        private class GratificacionRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string nombre { get; set; } = "";
            public string tipo { get; set; } = ""; public string? periodo { get; set; }
            public string frecuencia { get; set; } = "";
            public string? porcentaje_monto { get; set; }
            public decimal? monto_fijo { get; set; }
            public decimal? porcentaje { get; set; }
            public string base_calculo { get; set; } = ""; public DateTime? fecha_estimada { get; set; }
            public DateTime? fecha_pago { get; set; }
            public string estado { get; set; } = ""; public string empleados_aplica { get; set; } = ""; public int cantidad_empleados { get; set; }
            public string creado_por { get; set; } = ""; public DateTime fecha_creacion { get; set; }
        }
        private class PagoRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string? planilla_concepto { get; set; }
            public string periodo { get; set; } = ""; public DateTime fecha_pago { get; set; }
            public string banco { get; set; } = "";
            public decimal monto_pagado { get; set; }
            public string estado { get; set; } = ""; public string? observacion { get; set; }
            public int empleados { get; set; }
        }
        private class UtilidadRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public int ejercicio_fiscal { get; set; }
            public decimal porcentaje_participacion { get; set; }
            public decimal utilidad_neta_declarada { get; set; }
            public int dias_computables { get; set; }
            public decimal remuneracion_computable { get; set; }
            public decimal? monto_distribuido { get; set; }
            public DateTime fecha_pago_estimada { get; set; }
            public DateTime? fecha_pago_real { get; set; }
            public string estado { get; set; } = "";
            public string empleados_aplica { get; set; } = ""; public int cantidad_empleados { get; set; }
            public string? observacion { get; set; }
            public DateTime fecha_creacion { get; set; }
        }
        private class DeclaracionRow
        {
            public int id { get; set; }
            public string codigo { get; set; } = ""; public string periodo { get; set; } = "";
            public int trabajadores { get; set; }
            public decimal remuneracion_asignable { get; set; }
            public decimal aporte_essalud { get; set; }
            public decimal subsidios { get; set; }
            public decimal total_pagar { get; set; }
            public DateTime? fecha_envio { get; set; }
            public string estado { get; set; } = ""; public string tipo_declaracion { get; set; } = "";
            public string? nro_orden_sunat { get; set; }
            public string? observacion { get; set; }
        }
        private class GrupoSctrRow
        {
            public int id { get; set; }
            public string nivel_riesgo { get; set; } = ""; public int trabajadores { get; set; }
            public decimal sctr_salud { get; set; }
            public decimal sctr_pension { get; set; }
            public string aseguradora { get; set; } = ""; public bool activo { get; set; }
        }
        private class HistorialRow
        {
            public int id { get; set; }
            public DateTime fecha_hora { get; set; }
            public string declaracion { get; set; } = "";
            public string usuario { get; set; } = ""; public string estado { get; set; } = ""; public string? mensaje { get; set; }
        }
    }
}