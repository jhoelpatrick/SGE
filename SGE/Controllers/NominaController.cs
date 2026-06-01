using Microsoft.AspNetCore.Mvc;
using SGE.Data;
using SGE.Models;
using System.Text;

namespace SGE.Controllers
{
    public class NominaController : Controller
    {
        // ── Inyección de dependencia: repositorio de base de datos ──
        private readonly SgeDb _db;
        public NominaController(SgeDb db) => _db = db;

        // ── Planillas en memoria ──────────────────────────────────
        private static List<Planilla> _planillas = new List<Planilla>
        {
            new Planilla { Codigo="PLAN-2024-05", Periodo="Mayo 2024",       FechaCierre=new DateTime(2024,5,20),  Empleados=128, TotalBruto=315450, TotalDescuentos=58670, TotalNeto=256780, Estado="En Proceso" },
            new Planilla { Codigo="PLAN-2024-04", Periodo="Abril 2024",      FechaCierre=new DateTime(2024,4,30),  Empleados=125, TotalBruto=302150, TotalDescuentos=57500, TotalNeto=244650, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2024-03", Periodo="Marzo 2024",      FechaCierre=new DateTime(2024,3,31),  Empleados=122, TotalBruto=298750, TotalDescuentos=57430, TotalNeto=241320, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2024-02", Periodo="Febrero 2024",    FechaCierre=new DateTime(2024,2,29),  Empleados=120, TotalBruto=294500, TotalDescuentos=56610, TotalNeto=237890, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2024-01", Periodo="Enero 2024",      FechaCierre=new DateTime(2024,1,31),  Empleados=118, TotalBruto=292300, TotalDescuentos=56520, TotalNeto=235780, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-12", Periodo="Diciembre 2023",  FechaCierre=new DateTime(2023,12,29), Empleados=118, TotalBruto=289600, TotalDescuentos=55930, TotalNeto=233670, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-11", Periodo="Noviembre 2023",  FechaCierre=new DateTime(2023,11,30), Empleados=115, TotalBruto=285400, TotalDescuentos=54780, TotalNeto=230620, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-10", Periodo="Octubre 2023",    FechaCierre=new DateTime(2023,10,31), Empleados=115, TotalBruto=283900, TotalDescuentos=54210, TotalNeto=229690, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-09", Periodo="Septiembre 2023", FechaCierre=new DateTime(2023,9,30),  Empleados=112, TotalBruto=279500, TotalDescuentos=53800, TotalNeto=225700, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-08", Periodo="Agosto 2023",     FechaCierre=new DateTime(2023,8,31),  Empleados=110, TotalBruto=275200, TotalDescuentos=53100, TotalNeto=222100, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-07", Periodo="Julio 2023",      FechaCierre=new DateTime(2023,7,31),  Empleados=110, TotalBruto=272800, TotalDescuentos=52600, TotalNeto=220200, Estado="Pagado" },
            new Planilla { Codigo="PLAN-2023-06", Periodo="Junio 2023",      FechaCierre=new DateTime(2023,6,30),  Empleados=108, TotalBruto=268500, TotalDescuentos=51900, TotalNeto=216600, Estado="Pagado" },
        };
        // ── Empleados en memoria ───────────────────────────────────
        private static List<Empleado> _empleados = new List<Empleado>
{
    new() { Id=1,  Codigo="EMP-001", Nombres="Juan Carlos",   ApellidoPaterno="Pérez",    ApellidoMaterno="García",   NumeroDocumento="45123456", FechaNacimiento=new DateTime(1990,3,15), FechaIngreso=new DateTime(2020,1,6),  Cargo="Analista de Sistemas",    Departamento="TI",            CentroCostoId=1, SueldoBase=3500m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.AFP_Integra, BancoPago=MedioPago.BCP,       NumeroCuenta="19100234561", CCI="00219100234561234567", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=2,  Codigo="EMP-002", Nombres="María Elena",   ApellidoPaterno="Torres",   ApellidoMaterno="Quispe",   NumeroDocumento="52987654", FechaNacimiento=new DateTime(1988,7,22), FechaIngreso=new DateTime(2019,3,1),  Cargo="Contadora Senior",        Departamento="Contabilidad",  CentroCostoId=2, SueldoBase=4200m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.ONP,        BancoPago=MedioPago.BBVA,     NumeroCuenta="00110012345", CCI="01100110012345678901", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=3,  Codigo="EMP-003", Nombres="Carlos Alberto",ApellidoPaterno="Mendoza",  ApellidoMaterno="Ríos",     NumeroDocumento="38456789", FechaNacimiento=new DateTime(1985,11,8), FechaIngreso=new DateTime(2018,6,15), Cargo="Jefe de Recursos Humanos",Departamento="RRHH",          CentroCostoId=3, SueldoBase=5800m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.AFP_Prima,   BancoPago=MedioPago.Interbank,NumeroCuenta="200-3012345",CCI="00320020012345678901", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=4,  Codigo="EMP-004", Nombres="Ana Lucía",     ApellidoPaterno="Vargas",   ApellidoMaterno="Flores",   NumeroDocumento="70234567", FechaNacimiento=new DateTime(1995,2,14), FechaIngreso=new DateTime(2022,8,1),  Cargo="Asistente Administrativo",Departamento="Administración", CentroCostoId=4, SueldoBase=1800m,  TieneHijos=false, AsignacionFamiliar=0m,      SistemaPrevisional=TipoAFP.AFP_Habitat, BancoPago=MedioPago.BCP,       NumeroCuenta="19200345678", CCI="00219200345678234567", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.PlazoFijo,     RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=5,  Codigo="EMP-005", Nombres="Roberto",       ApellidoPaterno="Castillo", ApellidoMaterno="Huamán",   NumeroDocumento="29876543", FechaNacimiento=new DateTime(1980,9,30), FechaIngreso=new DateTime(2015,1,2),  Cargo="Gerente de Finanzas",     Departamento="Finanzas",      CentroCostoId=2, SueldoBase=8500m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.AFP_Profuturo,BancoPago=MedioPago.Scotiabank,NumeroCuenta="04100456789",CCI="00901040045678901234", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=6,  Codigo="EMP-006", Nombres="Luciana",       ApellidoPaterno="Morales",  ApellidoMaterno="Salas",    NumeroDocumento="61345678", FechaNacimiento=new DateTime(1993,6,5),  FechaIngreso=new DateTime(2021,4,12), Cargo="Diseñadora Gráfica",      Departamento="Marketing",     CentroCostoId=5, SueldoBase=2600m,  TieneHijos=false, AsignacionFamiliar=0m,      SistemaPrevisional=TipoAFP.AFP_Integra, BancoPago=MedioPago.BBVA,     NumeroCuenta="00120567890", CCI="01100120056789012345", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=7,  Codigo="EMP-007", Nombres="Miguel Ángel",  ApellidoPaterno="Paredes",  ApellidoMaterno="Chávez",   NumeroDocumento="47890123", FechaNacimiento=new DateTime(1991,12,18),FechaIngreso=new DateTime(2020,9,1),  Cargo="Desarrollador Backend",   Departamento="TI",            CentroCostoId=1, SueldoBase=4500m,  TieneHijos=false, AsignacionFamiliar=0m,      SistemaPrevisional=TipoAFP.AFP_Prima,   BancoPago=MedioPago.BCP,       NumeroCuenta="19300678901", CCI="00219300678901345678", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=8,  Codigo="EMP-008", Nombres="Sofía",         ApellidoPaterno="Reyes",    ApellidoMaterno="Mamani",   NumeroDocumento="73456789", FechaNacimiento=new DateTime(1997,4,25), FechaIngreso=new DateTime(2023,2,6),  Cargo="Practicante de Marketing",Departamento="Marketing",     CentroCostoId=5, SueldoBase=1025m,  TieneHijos=false, AsignacionFamiliar=0m,      SistemaPrevisional=TipoAFP.ONP,        BancoPago=MedioPago.Interbank,NumeroCuenta="200-0789012",CCI="00320020078901234567", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Practicante,   RegimeLaboral=RegimeLaboralT.Mype      },
    new() { Id=9,  Codigo="EMP-009", Nombres="Fernando",      ApellidoPaterno="Gutiérrez",ApellidoMaterno="León",     NumeroDocumento="32109876", FechaNacimiento=new DateTime(1978,1,11), FechaIngreso=new DateTime(2012,7,1),  Cargo="Supervisor de Logística", Departamento="Operaciones",   CentroCostoId=6, SueldoBase=3200m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.AFP_Habitat, BancoPago=MedioPago.BCP,       NumeroCuenta="19400890123", CCI="00219400890123456789", Estado=EstadoEmpleado.Vacaciones, TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
    new() { Id=10, Codigo="EMP-010", Nombres="Patricia",      ApellidoPaterno="Salinas",  ApellidoMaterno="Condori",  NumeroDocumento="58901234", FechaNacimiento=new DateTime(1986,8,3),  FechaIngreso=new DateTime(2017,11,15),Cargo="Jefa de Contabilidad",    Departamento="Contabilidad",  CentroCostoId=2, SueldoBase=5200m,  TieneHijos=true,  AsignacionFamiliar=102.50m, SistemaPrevisional=TipoAFP.AFP_Integra, BancoPago=MedioPago.BBVA,     NumeroCuenta="00130901234", CCI="01100130090123456789", Estado=EstadoEmpleado.Activo,    TipoContrato=TipoContrato.Indeterminado, RegimeLaboral=RegimeLaboralT.Regimen728 },
};

        // ── Método privado: calcula DetallePlanilla para un empleado ──
        private static DetallePlanilla CalcularDetalle(Empleado emp, string codigoPlanilla, string periodo)
        {
            decimal bruto = emp.RemuneracionComputable;

            // Descuentos según sistema previsional
            bool esAFP = emp.SistemaPrevisional != TipoAFP.ONP;
            decimal aportPrevisional = esAFP ? Math.Round(bruto * 0.10m, 2) : Math.Round(bruto * 0.13m, 2);
            decimal comisionAFP = esAFP ? Math.Round(bruto * 0.0147m, 2) : 0m;
            decimal seguroAFP = esAFP ? Math.Round(bruto * 0.0174m, 2) : 0m;
            decimal essaludTrab = emp.AfectoEssalud ? Math.Round(bruto * 0.04m, 2) : 0m;  // retención

            // Renta 5ta categoría simplificada (UIT 2024 = S/ 5,150)
            decimal renta5ta = 0m;
            if (emp.AfectoRenta5ta)
            {
                decimal anual = bruto * 14m; // 12 meses + gratificaciones
                decimal uit = 5150m;
                decimal base7UIT = anual - (7 * uit);
                if (base7UIT > 0) renta5ta = Math.Round((base7UIT * 0.08m) / 12m, 2);
            }

            decimal totalDesc = aportPrevisional + comisionAFP + seguroAFP + essaludTrab + renta5ta;
            decimal neto = bruto - totalDesc;

            return new DetallePlanilla
            {
                CodigoPlanilla = codigoPlanilla,
                EmpleadoId = emp.Id,
                Periodo = periodo,
                SueldoBase = emp.SueldoBase,
                AsignacionFamiliar = emp.TieneHijos ? emp.AsignacionFamiliar : 0m,
                TotalBruto = bruto,
                DescuentoAFP_ONP = aportPrevisional,
                ComisionAFP = comisionAFP,
                SeguroAFP = seguroAFP,
                EssaludTrabajador = essaludTrab,
                Renta5taCategoria = renta5ta,
                TotalDescuentos = totalDesc,
                EssaludEmpleador = Math.Round(bruto * 0.09m, 2),
                TotalNeto = neto,
                Estado = "Aprobado",
                NombreEmpleado = emp.NombreCompleto,
                DNIEmpleado = emp.NumeroDocumento,
                CargoEmpleado = emp.Cargo,
                SistemaPrevisional = emp.SistemaPrevisional.ToString().Replace("_", " "),
                BancoPago = emp.BancoPago.ToString(),
                NumeroCuenta = emp.NumeroCuenta,
            };
        }

        // ── Conceptos en memoria ──────────────────────────────────
        private static List<ConceptoNomina> _conceptos = new()
        {
            new() { Id=1,  Codigo="CON-001", Nombre="Sueldo Básico",              Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=2,  Codigo="CON-002", Nombre="Bonificación por Desempeño", Tipo=TipoConcepto.Variable, AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=3,  Codigo="CON-003", Nombre="Asignación Familiar",        Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=4,  Codigo="CON-004", Nombre="Horas Extras",               Tipo=TipoConcepto.Variable, AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,2,1) },
            new() { Id=5,  Codigo="CON-005", Nombre="Movilidad",                  Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,2,1) },
            new() { Id=6,  Codigo="CON-006", Nombre="Refrigerio",                 Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,2,1) },
            new() { Id=7,  Codigo="CON-007", Nombre="Gratificación",              Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=8,  Codigo="CON-008", Nombre="Otros Ingresos",             Tipo=TipoConcepto.Variable, AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=9,  Codigo="CON-009", Nombre="Descuento AFP",              Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=false, Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=10, Codigo="CON-010", Nombre="Descuento SNP",              Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=false, Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=11, Codigo="CON-011", Nombre="Essalud",                    Tipo=TipoConcepto.Fijo,     AfectaCalculo=true,  EsRemunerativo=false, Activo=true,  FechaCreacion=new DateTime(2024,4,1) },
            new() { Id=12, Codigo="CON-012", Nombre="Impuesto a la Renta",        Tipo=TipoConcepto.Variable, AfectaCalculo=true,  EsRemunerativo=false, Activo=true,  FechaCreacion=new DateTime(2024,4,1) },
            new() { Id=13, Codigo="CON-013", Nombre="Bono Escolaridad",           Tipo=TipoConcepto.Fijo,     AfectaCalculo=false, EsRemunerativo=true,  Activo=false, FechaCreacion=new DateTime(2024,4,1) },
            new() { Id=14, Codigo="CON-014", Nombre="Subsidio por Maternidad",    Tipo=TipoConcepto.Variable, AfectaCalculo=false, EsRemunerativo=false, Activo=false, FechaCreacion=new DateTime(2024,5,1) },
            new() { Id=15, Codigo="CON-015", Nombre="Compensación Vacaciones",    Tipo=TipoConcepto.Variable, AfectaCalculo=true,  EsRemunerativo=true,  Activo=true,  FechaCreacion=new DateTime(2024,5,1) },
        };

        // ── Descuentos en memoria ─────────────────────────────────
        private static List<Descuento> _descuentos = new();





        // ── Beneficios en memoria ──────────────────────────────────
        private static List<Beneficio> _beneficios = new()
        {
            new() { Id=1,  Codigo="BEN-001", Nombre="Alimentación",             Categoria=CategoriaBeneficio.Alimentacion, Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Mensual,   MontoCadena="S/ 250.00",         MontoFijo=250m,   Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=2,  Codigo="BEN-002", Nombre="Movilidad",                Categoria=CategoriaBeneficio.Transporte,   Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Mensual,   MontoCadena="S/ 200.00",         MontoFijo=200m,   Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=3,  Codigo="BEN-003", Nombre="Seguro Médico",            Categoria=CategoriaBeneficio.Salud,        Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Mensual,   MontoCadena="Según Plan",        MontoFijo=null,   Activo=true,  FechaCreacion=new DateTime(2024,1,1) },
            new() { Id=4,  Codigo="BEN-004", Nombre="Asignación Familiar",      Categoria=CategoriaBeneficio.Otros,        Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Mensual,   MontoCadena="S/ 100.00",         MontoFijo=100m,   Activo=true,  FechaCreacion=new DateTime(2024,2,1) },
            new() { Id=5,  Codigo="BEN-005", Nombre="Bonificación por Desempeño",Categoria=CategoriaBeneficio.Otros,       Tipo=TipoBeneficio.Bonificacion, Periodicidad=Periodicidad.Variable,  MontoCadena="Según Desempeño",   MontoFijo=null,   Activo=true,  FechaCreacion=new DateTime(2024,2,1) },
            new() { Id=6,  Codigo="BEN-006", Nombre="Capacitación",             Categoria=CategoriaBeneficio.Educacion,    Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Anual,     MontoCadena="S/ 500.00",         MontoFijo=500m,   Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=7,  Codigo="BEN-007", Nombre="Uniformes",                Categoria=CategoriaBeneficio.Otros,        Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Anual,     MontoCadena="S/ 300.00",         MontoFijo=300m,   Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=8,  Codigo="BEN-008", Nombre="Refrigerio",               Categoria=CategoriaBeneficio.Alimentacion, Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Diario,    MontoCadena="S/ 10.00 por día",  MontoFijo=10m,    Activo=true,  FechaCreacion=new DateTime(2024,3,1) },
            new() { Id=9,  Codigo="BEN-009", Nombre="Internet",                 Categoria=CategoriaBeneficio.Otros,        Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Mensual,   MontoCadena="S/ 80.00",          MontoFijo=80m,    Activo=false, FechaCreacion=new DateTime(2024,4,1) },
            new() { Id=10, Codigo="BEN-010", Nombre="Ingreso por Nacimiento",   Categoria=CategoriaBeneficio.Otros,        Tipo=TipoBeneficio.Beneficio,    Periodicidad=Periodicidad.Unico,     MontoCadena="S/ 500.00",         MontoFijo=500m,   Activo=true,  FechaCreacion=new DateTime(2024,4,1) },
        };

        // ── Gratificaciones en memoria ──────────────────────────────────
        private static List<Gratificacion> _gratificaciones = new()
        {
            new() { Id=1,  Codigo="GRA-001", Nombre="Cálculo gratificación julio y diciembre", Tipo=TipoGratificacion.Obligatoria, Periodo="Julio 2025",     Frecuencia=FrecuenciaGratificacion.Semestral, PorcentajeMonto="50% salario",   Porcentaje=50m,  MontoFijo=null, BaseDeCalculo=BaseCalculo.RemuneracionBasica,     FechaEstimada=new DateTime(2025,7,15),  FechaPago=null,                Estado=EstadoGratificacion.Pendiente,  EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=2,  Codigo="GRA-002", Nombre="CTS semestral (mayo y noviembre)",        Tipo=TipoGratificacion.Obligatoria, Periodo="Mayo 2025",      Frecuencia=FrecuenciaGratificacion.Semestral, PorcentajeMonto="100% salario",  Porcentaje=100m, MontoFijo=null, BaseDeCalculo=BaseCalculo.RemuneracionComputable, FechaEstimada=new DateTime(2025,5,15),  FechaPago=null,                Estado=EstadoGratificacion.Pendiente,  EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=3,  Codigo="GRA-003", Nombre="Bono extraordinario por gratificación",  Tipo=TipoGratificacion.Obligatoria, Periodo="Julio 2025",     Frecuencia=FrecuenciaGratificacion.Semestral, PorcentajeMonto="9% salario",    Porcentaje=9m,   MontoFijo=null, BaseDeCalculo=BaseCalculo.RemuneracionComputable, FechaEstimada=new DateTime(2025,7,15),  FechaPago=null,                Estado=EstadoGratificacion.Activa,     EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=4,  Codigo="GRA-004", Nombre="Gratificación julio",                    Tipo=TipoGratificacion.Obligatoria, Periodo="Julio 2025",     Frecuencia=FrecuenciaGratificacion.Semestral, PorcentajeMonto="50% salario",   Porcentaje=50m,  MontoFijo=null, BaseDeCalculo=BaseCalculo.RemuneracionBasica,     FechaEstimada=new DateTime(2025,7,15),  FechaPago=null,                Estado=EstadoGratificacion.Activa,     EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=5,  Codigo="GRA-005", Nombre="Gratificación diciembre",                Tipo=TipoGratificacion.Obligatoria, Periodo="Diciembre 2025", Frecuencia=FrecuenciaGratificacion.Semestral, PorcentajeMonto="50% salario",   Porcentaje=50m,  MontoFijo=null, BaseDeCalculo=BaseCalculo.RemuneracionBasica,     FechaEstimada=new DateTime(2025,12,15), FechaPago=null,                Estado=EstadoGratificacion.Programada, EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=6,  Codigo="GRA-006", Nombre="Bono por productividad",                 Tipo=TipoGratificacion.Voluntaria,  Periodo="Junio 2025",     Frecuencia=FrecuenciaGratificacion.Mensual,   PorcentajeMonto="S/ 1,000.00",   Porcentaje=null, MontoFijo=1000m,BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,6,30),  FechaPago=null,                Estado=EstadoGratificacion.Activa,     EmpleadosAplica="Ventas",CantidadEmpleados=25,  CreadoPor="Jefe de RRHH",FechaCreacion=new DateTime(2025,5,28) },
            new() { Id=7,  Codigo="GRA-007", Nombre="Bono por desempeño",                     Tipo=TipoGratificacion.Voluntaria,  Periodo="Mayo 2025",      Frecuencia=FrecuenciaGratificacion.Mensual,   PorcentajeMonto="S/ 800.00",     Porcentaje=null, MontoFijo=800m, BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,5,31),  FechaPago=new DateTime(2025,5,28), Estado=EstadoGratificacion.Pagada,     EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Jefe de RRHH",FechaCreacion=new DateTime(2025,5,28) },
            new() { Id=8,  Codigo="GRA-008", Nombre="Gratificación especial",                 Tipo=TipoGratificacion.Voluntaria,  Periodo="Junio 2025",     Frecuencia=FrecuenciaGratificacion.Unica,     PorcentajeMonto="S/ 500.00",     Porcentaje=null, MontoFijo=500m, BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,6,30),  FechaPago=null,                Estado=EstadoGratificacion.Borrador,   EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,6,1) },
            new() { Id=9,  Codigo="GRA-009", Nombre="Bono calidad de servicio",               Tipo=TipoGratificacion.Voluntaria,  Periodo="Mayo 2025",      Frecuencia=FrecuenciaGratificacion.Mensual,   PorcentajeMonto="S/ 600.00",     Porcentaje=null, MontoFijo=600m, BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,5,31),  FechaPago=null,                Estado=EstadoGratificacion.Programada, EmpleadosAplica="Atención al Cliente", CantidadEmpleados=15, CreadoPor="Jefe de RRHH",FechaCreacion=new DateTime(2025,5,29) },
            new() { Id=10, Codigo="GRA-010", Nombre="Bono metas",                             Tipo=TipoGratificacion.Voluntaria,  Periodo="Mayo 2025",      Frecuencia=FrecuenciaGratificacion.Mensual,   PorcentajeMonto="Porcentaje variable", Porcentaje=null, MontoFijo=null,BaseDeCalculo=BaseCalculo.PorcentajeVariable, FechaEstimada=new DateTime(2025,5,31), FechaPago=new DateTime(2025,5,26), Estado=EstadoGratificacion.Pagada,  EmpleadosAplica="Ventas",CantidadEmpleados=25,  CreadoPor="Jefe de RRHH",FechaCreacion=new DateTime(2025,5,29) },
            new() { Id=11, Codigo="GRA-011", Nombre="Bonificación por antigüedad",            Tipo=TipoGratificacion.Voluntaria,  Periodo="Junio 2025",     Frecuencia=FrecuenciaGratificacion.Mensual,   PorcentajeMonto="S/ 800.00",     Porcentaje=null, MontoFijo=800m, BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,6,30),  FechaPago=null,                Estado=EstadoGratificacion.Activa,     EmpleadosAplica="Todos", CantidadEmpleados=120, CreadoPor="Admin",       FechaCreacion=new DateTime(2025,5,30) },
            new() { Id=12, Codigo="GRA-012", Nombre="Bono innovación",                        Tipo=TipoGratificacion.Voluntaria,  Periodo="Abril 2025",     Frecuencia=FrecuenciaGratificacion.Unica,     PorcentajeMonto="S/ 1,500.00",   Porcentaje=null, MontoFijo=1500m,BaseDeCalculo=BaseCalculo.Fijo,              FechaEstimada=new DateTime(2025,4,30),  FechaPago=new DateTime(2025,4,25), Estado=EstadoGratificacion.Pagada,  EmpleadosAplica="IT",    CantidadEmpleados=10,  CreadoPor="Gerente General",FechaCreacion=new DateTime(2025,4,24) },
        };

        // ─────────────────────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            // Recargar listas en memoria (patrón existente del proyecto)
            _planillas = _db.ObtenerPlanillas("", "");
            _empleados = _db.ObtenerEmpleados("", "", "");

            // Obtener resumen agregado desde BD (una sola consulta multi-result)
            var resumen = _db.ObtenerResumenDashboard();

            // Calcular próximo día de pago desde configuración si no hay planilla futura
            var parametros = _db.ObtenerParametros();
            var hoy = DateTime.Today;
            var diaPago = parametros.DiaPagoPlanilla; // ej: 31
            var proximoPago = resumen.ProximoPago
                              ?? new DateTime(hoy.Year, hoy.Month, Math.Min(diaPago, DateTime.DaysInMonth(hoy.Year, hoy.Month)));
            if (proximoPago < hoy)
            {
                // Ya pasó el día de pago este mes → proyectar al mes siguiente
                var mesProx = hoy.Month == 12 ? 1 : hoy.Month + 1;
                var anioProx = hoy.Month == 12 ? hoy.Year + 1 : hoy.Year;
                proximoPago = new DateTime(anioProx, mesProx,
                                  Math.Min(diaPago, DateTime.DaysInMonth(anioProx, mesProx)));
            }

            var vm = new NominaViewModel
            {
                // Totales monetarios reales desde BD
                TotalPlanillaMesActual = resumen.TotalNetoMesActual,
                TotalPlanillaMesAnterior = resumen.TotalNetoMesAnterior,
                DescuentosTotales = resumen.DescuentosMesActual,

                // Empleados reales desde BD
                EmpleadosEnPlanilla = resumen.EmpleadosActivos,
                EmpleadosNuevosMes = resumen.EmpleadosNuevosMes,

                // Fecha próximo pago
                ProximoPago = proximoPago,

                // Estado de planillas real desde BD
                PlanillasPagadas = resumen.PlanillasPagadas,
                PlanillasEnProceso = resumen.PlanillasEnProceso,
                PlanillasPendientes = resumen.PlanillasPendientes,
                PlanillasAnuladas = resumen.PlanillasAnuladas,

                // Últimas planillas para la tabla del dashboard
                UltimasPlanillas = resumen.UltimasPlanillas,

                // Fuerza Laboral desde BD
                TotalEmpleados = resumen.EmpleadosActivos + resumen.EmpleadosEnVacaciones,
                EmpleadosActivos = resumen.EmpleadosActivos,
                EmpleadosEnVacaciones = resumen.EmpleadosEnVacaciones,
                MasaSalarial = resumen.MasaSalarial,
                EmpleadosPreview = _empleados
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────
        // PLANILLAS
        // ─────────────────────────────────────────────────────────
        public IActionResult Planillas(string? buscar, string? estado, int pagina = 1)
        {
            _planillas = _db.ObtenerPlanillas(buscar ?? "", estado ?? "");
            int porPagina = 8;
            var lista = _planillas.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(p =>
                    p.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    p.Periodo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(estado))
                lista = lista.Where(p => p.Estado == estado);

            int total = lista.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            var vm = new PlanillasViewModel
            {
                Planillas = lista.Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                EstadoFiltro = estado ?? "",
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult CrearPlanilla() => Json(new { ok = true });

        [HttpPost]
        public IActionResult CrearPlanilla(string periodo, DateTime fechaCierre,
                                            int empleados, decimal totalBruto,
                                            decimal totalDescuentos, string estado)
        {
            int num = _db.ObtenerPlanillas("", "").Count + 1;
            string cod = $"PLAN-{fechaCierre.Year}-{num:D2}";
            var nuevaPlanilla = new Planilla
            {
                Codigo = cod,
                Periodo = periodo,
                FechaCierre = fechaCierre,
                Empleados = empleados,
                TotalBruto = totalBruto,
                Descuentos = totalDescuentos,
                TotalDescuentos = totalDescuentos,
                TotalNeto = totalBruto - totalDescuentos,
                Estado = estado
            };
            _db.InsertarPlanilla(nuevaPlanilla);
            _planillas = _db.ObtenerPlanillas("", "");
            TempData["Mensaje"] = $"Planilla {cod} creada correctamente.";
            return RedirectToAction("Planillas");
        }

        public IActionResult EditarPlanilla(string codigo)
        {
            var p = _db.ObtenerPlanillaPorCodigo(codigo);
            if (p == null) return NotFound();
            return Json(new
            {
                p.Codigo,
                p.Periodo,
                fechaCierre = p.FechaCierre.ToString("yyyy-MM-dd"),
                p.Empleados,
                p.TotalBruto,
                totalDescuentos = p.TotalDescuentos > 0 ? p.TotalDescuentos : p.Descuentos,
                totalNeto = p.TotalNeto,
                p.Estado
            });
        }

        [HttpPost]
        public IActionResult EditarPlanilla(string codigo, string periodo,
                                             string? fechaCierre, int empleados,
                                             decimal totalBruto, decimal totalDescuentos, string estado)
        {
            DateTime fecha = DateTime.TryParse(fechaCierre, out var fd) ? fd : DateTime.Today;
            var p = new Planilla
            {
                Codigo = codigo,
                Periodo = periodo,
                FechaCierre = fecha,
                Empleados = empleados,
                TotalBruto = totalBruto,
                Descuentos = totalDescuentos,
                TotalDescuentos = totalDescuentos,
                TotalNeto = totalBruto - totalDescuentos,
                Estado = estado
            };
            _db.ActualizarPlanilla(p);
            _planillas = _db.ObtenerPlanillas("", "");
            TempData["Mensaje"] = $"Planilla {codigo} actualizada.";
            return RedirectToAction("Planillas");
        }

        [HttpPost]
        public IActionResult EliminarPlanilla(string codigo)
        {
            _db.EliminarPlanilla(codigo);
            _planillas = _db.ObtenerPlanillas("", "");
            TempData["Mensaje"] = $"Planilla {codigo} eliminada.";
            return RedirectToAction("Planillas");
        }

        public IActionResult ExportarCSV(string? buscar, string? estado)
        {
            var lista = _planillas.AsQueryable();
            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(p =>
                    p.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    p.Periodo.Contains(buscar, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(estado))
                lista = lista.Where(p => p.Estado == estado);

            var sb = new StringBuilder();
            sb.AppendLine("Código,Período,Fecha Cierre,Empleados,Total Bruto,Total Descuentos,Total Neto,Estado");
            foreach (var p in lista)
                sb.AppendLine($"{p.Codigo},{p.Periodo},{p.FechaCierre:dd/MM/yyyy},{p.Empleados},{p.TotalBruto},{p.TotalDescuentos},{p.TotalNeto},{p.Estado}");

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "planillas.csv");
        }

        // ─────────────────────────────────────────────────────────
        // CONCEPTOS
        // ─────────────────────────────────────────────────────────
        public IActionResult Conceptos(string? buscar, string? tipo, string? estado, int pagina = 1)
        {
            _conceptos = _db.ObtenerConceptos(buscar ?? "", tipo ?? "", estado ?? "");
            int porPagina = 8;
            var lista = _conceptos.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(c =>
                    c.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    c.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoConcepto>(tipo, out var t))
                lista = lista.Where(c => c.Tipo == t);

            if (!string.IsNullOrEmpty(estado))
            {
                bool activo = estado == "Activo";
                lista = lista.Where(c => c.Activo == activo);
            }

            int total = lista.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            ViewBag.StatsTotal = _conceptos.Count;
            ViewBag.StatsActivos = _conceptos.Count(c => c.Activo);
            ViewBag.StatsFijos = _conceptos.Count(c => c.Tipo == TipoConcepto.Fijo);
            ViewBag.StatsVariables = _conceptos.Count(c => c.Tipo == TipoConcepto.Variable);

            var vm = new ConceptosViewModel
            {
                Conceptos = lista.Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                TipoFiltro = tipo ?? "",
                EstadoFiltro = estado ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearConcepto(string nombre, string tipoConcepto,
            bool afectaCalculo, bool esRemunerativo, bool activo)
        {
            int nextId = _conceptos.Count > 0 ? _conceptos.Max(c => c.Id) + 1 : 1;
            Enum.TryParse<TipoConcepto>(tipoConcepto, out var tipo);
            var _obj_conc = new ConceptoNomina
            {
                Id = nextId,
                Codigo = $"CON-{nextId:D3}",
                Nombre = nombre,
                Tipo = tipo,
                AfectaCalculo = afectaCalculo,
                EsRemunerativo = esRemunerativo,
                Activo = activo,
                FechaCreacion = DateTime.Today
            };
            _conceptos.Add(_obj_conc);
            _db.InsertarConcepto(_obj_conc);
            TempData["MensajeConcepto"] = $"Concepto CON-{nextId:D3} creado correctamente.";
            return RedirectToAction(nameof(Conceptos));
        }

        [HttpGet]
        public IActionResult ObtenerConcepto(int id)
        {
            var c = _conceptos.FirstOrDefault(x => x.Id == id);
            if (c == null) return NotFound();
            return Json(new
            {
                c.Id,
                c.Codigo,
                c.Nombre,
                tipo = c.Tipo.ToString(),
                afectaCalculo = c.AfectaCalculo,
                esRemunerativo = c.EsRemunerativo,
                activo = c.Activo
            });
        }

        [HttpPost]
        public IActionResult EditarConcepto(int id, string nombre, string tipoConcepto,
            bool afectaCalculo, bool esRemunerativo, bool activo)
        {
            var c = _conceptos.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                Enum.TryParse<TipoConcepto>(tipoConcepto, out var tipo);
                c.Nombre = nombre; c.Tipo = tipo;
                c.AfectaCalculo = afectaCalculo;
                c.EsRemunerativo = esRemunerativo;
                c.Activo = activo;
                _db.ActualizarConcepto(c);
                _conceptos = _db.ObtenerConceptos();
            }
            TempData["MensajeConcepto"] = $"Concepto {c?.Codigo} actualizado.";
            return RedirectToAction(nameof(Conceptos));
        }

        [HttpPost]
        public IActionResult EliminarConcepto(int id)
        {
            var c = _conceptos.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                _db.EliminarConcepto(id);
                _conceptos.Remove(c); TempData["MensajeConcepto"] = $"Concepto {c.Codigo} eliminado.";
            }
            return RedirectToAction(nameof(Conceptos));
        }

        [HttpPost]
        public IActionResult ToggleConcepto(int id)
        {
            var c = _conceptos.FirstOrDefault(x => x.Id == id);
            if (c != null) c.Activo = !c.Activo;
            return RedirectToAction(nameof(Conceptos));
        }

        // ─────────────────────────────────────────────────────────
        // DESCUENTOS
        // ─────────────────────────────────────────────────────────
        public IActionResult Descuentos(string? buscar, string? tipo, string? estado, int pagina = 1)
        {
            _descuentos = _db.ObtenerDescuentos(buscar ?? "", tipo ?? "", estado ?? "");
            int porPagina = 8;
            var lista = _descuentos.AsQueryable();

            int total = lista.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            // Stats desde BD sin filtros para mostrar totales reales
            var todos = _db.ObtenerDescuentos();
            ViewBag.StatsTotal = todos.Count;
            ViewBag.StatsActivos = todos.Count(d => d.Activo);
            ViewBag.StatsObligatorio = todos.Count(d => d.Tipo == "Obligatorio");
            ViewBag.StatsVoluntario = todos.Count(d => d.Tipo == "Voluntario");

            var vm = new DescuentosViewModel
            {
                Descuentos = lista.Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                TipoFiltro = tipo ?? "",
                EstadoFiltro = estado ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearDescuento(string nombre, string tipo,
            bool obligatorio, bool afectaNeto, decimal porcentaje, bool activo)
        {
            _descuentos = _db.ObtenerDescuentos();
            int nextId = _descuentos.Count > 0 ? _descuentos.Max(d => d.Id) + 1 : 1;
            var _obj_desc = new Descuento
            {
                Id = nextId,
                Codigo = $"DES-{nextId:D3}",
                Nombre = nombre,
                Tipo = tipo,
                Obligatorio = obligatorio,
                AfectaNeto = afectaNeto,
                Porcentaje = porcentaje,
                Activo = activo
            };
            _db.InsertarDescuento(_obj_desc);
            _descuentos = _db.ObtenerDescuentos();
            TempData["MensajeDescuento"] = $"Descuento DES-{nextId:D3} creado correctamente.";
            return RedirectToAction(nameof(Descuentos));
        }

        [HttpGet]
        public IActionResult ObtenerDescuento(int id)
        {
            var d = _descuentos.FirstOrDefault(x => x.Id == id)
                    ?? _db.ObtenerDescuentos().FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();
            return Json(new
            {
                d.Id,
                d.Codigo,
                d.Nombre,
                d.Tipo,
                obligatorio = d.Obligatorio,
                afectaNeto = d.AfectaNeto,
                d.Porcentaje,
                activo = d.Activo
            });
        }

        [HttpPost]
        public IActionResult EditarDescuento(int id, string nombre, string tipo,
            bool obligatorio, bool afectaNeto, decimal porcentaje, bool activo)
        {
            var d = _descuentos.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                d.Nombre = nombre; d.Tipo = tipo;
                d.Obligatorio = obligatorio; d.AfectaNeto = afectaNeto;
                d.Porcentaje = porcentaje; d.Activo = activo;
                _db.ActualizarDescuento(d);
                _descuentos = _db.ObtenerDescuentos();
            }
            TempData["MensajeDescuento"] = $"Descuento {d?.Codigo} actualizado.";
            return RedirectToAction(nameof(Descuentos));
        }

        [HttpPost]
        public IActionResult EliminarDescuento(int id)
        {
            var d = _descuentos.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                _db.EliminarDescuento(id);
                _descuentos.Remove(d); TempData["MensajeDescuento"] = $"Descuento {d.Codigo} eliminado.";
            }
            return RedirectToAction(nameof(Descuentos));
        }

        [HttpPost]
        public IActionResult ToggleDescuento(int id)
        {
            var d = _descuentos.FirstOrDefault(x => x.Id == id);
            if (d != null) d.Activo = !d.Activo;
            return RedirectToAction(nameof(Descuentos));
        }

        // ─────────────────────────────────────────────────────────
        // BENEFICIOS
        // ─────────────────────────────────────────────────────────
        public IActionResult Beneficios(string? buscar, string? categoria, string? estado, int pagina = 1)
        {
            _beneficios = _db.ObtenerBeneficios(buscar ?? "", categoria ?? "", "");
            int porPagina = 10;
            var lista = _beneficios.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(b =>
                    b.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    b.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(categoria) && Enum.TryParse<CategoriaBeneficio>(categoria, out var cat))
                lista = lista.Where(b => b.Categoria == cat);

            if (!string.IsNullOrEmpty(estado))
            {
                bool activo = estado == "Activo";
                lista = lista.Where(b => b.Activo == activo);
            }

            int total = lista.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            // Stats para KPIs
            ViewBag.StatsTotal = _beneficios.Count;
            ViewBag.StatsActivos = _beneficios.Count(b => b.Activo);
            ViewBag.StatsInactivos = _beneficios.Count(b => !b.Activo);
            ViewBag.MontoMensual = _beneficios
                .Where(b => b.Activo && b.MontoFijo.HasValue)
                .Sum(b => b.Periodicidad switch {
                    Periodicidad.Diario => b.MontoFijo!.Value * 26,
                    Periodicidad.Mensual => b.MontoFijo!.Value,
                    Periodicidad.Trimestral => b.MontoFijo!.Value / 3,
                    Periodicidad.Anual => b.MontoFijo!.Value / 12,
                    Periodicidad.Unico => 0,
                    _ => 0
                });

            var vm = new BeneficiosViewModel
            {
                Beneficios = lista.Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                CategoriaFiltro = categoria ?? "",
                EstadoFiltro = estado ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearBeneficio(string nombre, string categoria, string tipo,
            string periodicidad, string montoCadena, decimal? montoFijo, bool activo)
        {
            int nextId = _beneficios.Count > 0 ? _beneficios.Max(b => b.Id) + 1 : 1;
            Enum.TryParse<CategoriaBeneficio>(categoria, out var cat);
            Enum.TryParse<TipoBeneficio>(tipo, out var tip);
            Enum.TryParse<Periodicidad>(periodicidad, out var per);
            var _obj_bene = new Beneficio
            {
                Id = nextId,
                Codigo = $"BEN-{nextId:D3}",
                Nombre = nombre,
                Categoria = cat,
                Tipo = tip,
                Periodicidad = per,
                MontoCadena = string.IsNullOrWhiteSpace(montoCadena) ? (montoFijo.HasValue ? $"S/ {montoFijo:N2}" : "Variable") : montoCadena,
                MontoFijo = montoFijo,
                Activo = activo,
                FechaCreacion = DateTime.Today
            };
            _beneficios.Add(_obj_bene);
            _db.InsertarBeneficio(_obj_bene);
            TempData["MensajeBeneficio"] = $"Beneficio BEN-{nextId:D3} creado correctamente.";
            return RedirectToAction(nameof(Beneficios));
        }

        [HttpGet]
        public IActionResult ObtenerBeneficio(int id)
        {
            var b = _beneficios.FirstOrDefault(x => x.Id == id);
            if (b == null) return NotFound();
            return Json(new
            {
                b.Id,
                b.Codigo,
                b.Nombre,
                categoria = b.Categoria.ToString(),
                tipo = b.Tipo.ToString(),
                periodicidad = b.Periodicidad.ToString(),
                b.MontoCadena,
                montoFijo = b.MontoFijo,
                activo = b.Activo
            });
        }

        [HttpPost]
        public IActionResult EditarBeneficio(int id, string nombre, string categoria, string tipo,
            string periodicidad, string montoCadena, decimal? montoFijo, bool activo)
        {
            var b = _beneficios.FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                Enum.TryParse<CategoriaBeneficio>(categoria, out var cat);
                Enum.TryParse<TipoBeneficio>(tipo, out var tip);
                Enum.TryParse<Periodicidad>(periodicidad, out var per);
                b.Nombre = nombre; b.Categoria = cat; b.Tipo = tip;
                b.Periodicidad = per;
                b.MontoCadena = string.IsNullOrWhiteSpace(montoCadena) ? (montoFijo.HasValue ? $"S/ {montoFijo:N2}" : "Variable") : montoCadena;
                b.MontoFijo = montoFijo; b.Activo = activo;
                _db.ActualizarBeneficio(b);
                _beneficios = _db.ObtenerBeneficios("", "", "");
            }
            TempData["MensajeBeneficio"] = $"Beneficio {b?.Codigo} actualizado.";
            return RedirectToAction(nameof(Beneficios));
        }

        [HttpPost]
        public IActionResult EliminarBeneficio(int id)
        {
            var b = _beneficios.FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                _db.EliminarBeneficio(id);
                _beneficios.Remove(b); TempData["MensajeBeneficio"] = $"Beneficio {b.Codigo} eliminado.";
            }
            return RedirectToAction(nameof(Beneficios));
        }

        [HttpPost]
        public IActionResult ToggleBeneficio(int id)
        {
            var b = _beneficios.FirstOrDefault(x => x.Id == id);
            if (b != null) b.Activo = !b.Activo;
            return RedirectToAction(nameof(Beneficios));
        }

        // ─────────────────────────────────────────────────────────
        // GRATIFICACIONES
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult EditarGratificacion(int id, string nombre, string tipoGrat, string periodo,
    string frecuencia, string porcentajeMonto, string baseCalculo,
    decimal? montoFijo, decimal? porcentaje, DateTime? fechaEstimada,
    string empleadosAplica, int cantidadEmpleados, string estadoGrat)
        {
            _gratificaciones = _db.ObtenerGratificaciones();
            var g = _gratificaciones.FirstOrDefault(x => x.Id == id);
            if (g != null)
            {
                Enum.TryParse<TipoGratificacion>(tipoGrat, out var tip);
                Enum.TryParse<FrecuenciaGratificacion>(frecuencia, out var frec);
                Enum.TryParse<BaseCalculo>(baseCalculo, out var bc);
                Enum.TryParse<EstadoGratificacion>(estadoGrat, out var est);
                g.Nombre = nombre; g.Tipo = tip; g.Periodo = periodo; g.Frecuencia = frec;
                g.PorcentajeMonto = porcentajeMonto; g.BaseDeCalculo = bc; g.MontoFijo = montoFijo;
                g.Porcentaje = porcentaje; g.FechaEstimada = fechaEstimada; g.Estado = est;
                g.EmpleadosAplica = empleadosAplica; g.CantidadEmpleados = cantidadEmpleados;
                _db.ActualizarGratificacion(g); // ← guardar en BD
                _gratificaciones = _db.ObtenerGratificaciones();
            }
            TempData["MensajeGratificacion"] = $"Gratificación {g?.Codigo} actualizada.";
            return RedirectToAction(nameof(Gratificaciones));
        }

        public IActionResult Gratificaciones(string? buscar, string? tipo, string? estado, int pagina = 1)
        {
            _gratificaciones = _db.ObtenerGratificaciones(buscar ?? "", tipo ?? "", estado ?? "");
            const int porPagina = 10;
            var lista = _gratificaciones.AsEnumerable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(g => g.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                                      || g.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoGratificacion>(tipo, out var t))
                lista = lista.Where(g => g.Tipo == t);

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoGratificacion>(estado, out var e))
                lista = lista.Where(g => g.Estado == e);

            var total = lista.Count();
            var paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Clamp(pagina, 1, Math.Max(1, paginas));

            ViewBag.StatsTotal = _gratificaciones.Count;
            ViewBag.StatsActivas = _gratificaciones.Count(g => g.Estado == EstadoGratificacion.Activa);
            ViewBag.StatsPendientes = _gratificaciones.Count(g => g.Estado == EstadoGratificacion.Pendiente);
            ViewBag.StatsPagadas = _gratificaciones.Count(g => g.Estado == EstadoGratificacion.Pagada);
            ViewBag.MontoTotalAnio = _gratificaciones
                .Where(g => g.Estado == EstadoGratificacion.Pagada && g.MontoFijo.HasValue)
                .Sum(g => g.MontoFijo ?? 0);

            var vm = new GratificacionesViewModel
            {
                Gratificaciones = lista.Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                TipoFiltro = tipo ?? "",
                EstadoFiltro = estado ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearGratificacion(string nombre, string tipoGrat, string periodo,
            string frecuencia, string porcentajeMonto, string baseCalculo,
            decimal? montoFijo, decimal? porcentaje, DateTime? fechaEstimada,
            string empleadosAplica, int cantidadEmpleados, string estadoGrat)
        {
            var nextId = _gratificaciones.Any() ? _gratificaciones.Max(g => g.Id) + 1 : 1;
            Enum.TryParse<TipoGratificacion>(tipoGrat, out var tip);
            Enum.TryParse<FrecuenciaGratificacion>(frecuencia, out var frec);
            Enum.TryParse<BaseCalculo>(baseCalculo, out var bc);
            Enum.TryParse<EstadoGratificacion>(estadoGrat, out var est);

            var _obj_grat = new Gratificacion
            {
                Id = nextId,
                Codigo = $"GRA-{nextId:D3}",
                Nombre = nombre,
                Tipo = tip,
                Periodo = periodo,
                Frecuencia = frec,
                PorcentajeMonto = porcentajeMonto,
                MontoFijo = montoFijo,
                Porcentaje = porcentaje,
                BaseDeCalculo = bc,
                FechaEstimada = fechaEstimada,
                Estado = est,
                EmpleadosAplica = empleadosAplica,
                CantidadEmpleados = cantidadEmpleados,
                CreadoPor = "Admin",
                FechaCreacion = DateTime.Today
            };
            _gratificaciones.Add(_obj_grat);
            _db.InsertarGratificacion(_obj_grat);
            TempData["MensajeGratificacion"] = $"Gratificación GRA-{nextId:D3} creada correctamente.";
            return RedirectToAction(nameof(Gratificacion));
        }

        public IActionResult ObtenerGratificacion(int id)
        {
            var g = _gratificaciones.FirstOrDefault(x => x.Id == id);
            if (g == null) return NotFound();
            return Json(new
            {
                g.Id,
                g.Nombre,
                g.Periodo,
                g.PorcentajeMonto,
                tipo = g.Tipo.ToString(),
                frecuencia = g.Frecuencia.ToString(),
                baseCalculo = g.BaseDeCalculo.ToString(),
                estado = g.Estado.ToString(),
                g.MontoFijo,
                g.Porcentaje,
                fechaEstimada = g.FechaEstimada?.ToString("yyyy-MM-dd"),
                g.EmpleadosAplica,
                g.CantidadEmpleados
            });
        }

        [HttpPost]
        public IActionResult EliminarGratificacion(int id)
        {
            var g = _gratificaciones.FirstOrDefault(x => x.Id == id);
            if (g != null)
            {
                _db.EliminarGratificacion(id);
                _gratificaciones.Remove(g); TempData["MensajeGratificacion"] = $"Gratificación {g.Codigo} eliminada.";
            }
            return RedirectToAction("Gratificaciones");
        }

        // ─────────────────────────────────────────────────────────
        // UTILIDADES / PARTICIPACIÓN
        // ─────────────────────────────────────────────────────────
        private static List<Utilidad> _utilidades = new()
        {
            new() { Id=1, Codigo="UTI-001", EjercicioFiscal=2025, PorcentajeParticipacion=10m, UtilidadNetaDeclarada=1_200_000m, DiasComputables=360, RemuneracionComputable=8_500_000m, MontoDistribuido=null,         FechaPagoEstimada=new DateTime(2025,12,31), FechaPagoReal=null,                     Estado=EstadoUtilidad.Pendiente, EmpleadosAplica="Todos", CantidadEmpleados=120, Observacion="" },
            new() { Id=2, Codigo="UTI-002", EjercicioFiscal=2025, PorcentajeParticipacion=8m,  UtilidadNetaDeclarada=  950_000m, DiasComputables=360, RemuneracionComputable=7_250_000m, MontoDistribuido=null,         FechaPagoEstimada=new DateTime(2025,12,31), FechaPagoReal=null,                     Estado=EstadoUtilidad.Pendiente, EmpleadosAplica="Todos", CantidadEmpleados=120, Observacion="" },
            new() { Id=3, Codigo="UTI-003", EjercicioFiscal=2025, PorcentajeParticipacion=10m, UtilidadNetaDeclarada=1_050_000m, DiasComputables=360, RemuneracionComputable=8_000_000m, MontoDistribuido=null,         FechaPagoEstimada=new DateTime(2025,12,31), FechaPagoReal=null,                     Estado=EstadoUtilidad.Pendiente, EmpleadosAplica="Todos", CantidadEmpleados=120, Observacion="" },
            new() { Id=4, Codigo="UTI-004", EjercicioFiscal=2024, PorcentajeParticipacion=10m, UtilidadNetaDeclarada=  980_000m, DiasComputables=360, RemuneracionComputable=7_800_000m, MontoDistribuido=  980_000m,   FechaPagoEstimada=new DateTime(2025,3,15),  FechaPagoReal=new DateTime(2025,3,15),  Estado=EstadoUtilidad.Pagada,    EmpleadosAplica="Todos", CantidadEmpleados=118, Observacion="Pagado en tiempo" },
            new() { Id=5, Codigo="UTI-005", EjercicioFiscal=2024, PorcentajeParticipacion=8m,  UtilidadNetaDeclarada=  820_000m, DiasComputables=360, RemuneracionComputable=6_400_000m, MontoDistribuido=  820_000m,   FechaPagoEstimada=new DateTime(2025,3,15),  FechaPagoReal=new DateTime(2025,3,15),  Estado=EstadoUtilidad.Pagada,    EmpleadosAplica="Todos", CantidadEmpleados=118, Observacion="" },
            new() { Id=6, Codigo="UTI-006", EjercicioFiscal=2024, PorcentajeParticipacion=10m, UtilidadNetaDeclarada=1_100_000m, DiasComputables=360, RemuneracionComputable=8_900_000m, MontoDistribuido=1_100_000m,   FechaPagoEstimada=new DateTime(2025,3,15),  FechaPagoReal=new DateTime(2025,3,15),  Estado=EstadoUtilidad.Pagada,    EmpleadosAplica="Todos", CantidadEmpleados=118, Observacion="" },
            new() { Id=7, Codigo="UTI-007", EjercicioFiscal=2023, PorcentajeParticipacion=10m, UtilidadNetaDeclarada=  900_000m, DiasComputables=360, RemuneracionComputable=7_200_000m, MontoDistribuido=  900_000m,   FechaPagoEstimada=new DateTime(2024,3,15),  FechaPagoReal=new DateTime(2024,3,15),  Estado=EstadoUtilidad.Pagada,    EmpleadosAplica="Todos", CantidadEmpleados=115, Observacion="" },
            new() { Id=8, Codigo="UTI-008", EjercicioFiscal=2023, PorcentajeParticipacion=8m,  UtilidadNetaDeclarada=  720_000m, DiasComputables=360, RemuneracionComputable=5_700_000m, MontoDistribuido=  720_000m,   FechaPagoEstimada=new DateTime(2024,3,15),  FechaPagoReal=new DateTime(2024,3,15),  Estado=EstadoUtilidad.Pagada,    EmpleadosAplica="Todos", CantidadEmpleados=115, Observacion="" },
        };

        public IActionResult Utilidades(string? buscar, string? estado, string? anio, int pagina = 1)
        {
            _utilidades = _db.ObtenerUtilidades(buscar ?? "", estado ?? "");
            const int porPagina = 10;
            var lista = _utilidades.AsEnumerable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(u => u.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                                      || u.EjercicioFiscal.ToString().Contains(buscar)
                                      || u.Observacion.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoUtilidad>(estado, out var est))
                lista = lista.Where(u => u.Estado == est);

            if (!string.IsNullOrEmpty(anio) && int.TryParse(anio, out var anioNum))
                lista = lista.Where(u => u.EjercicioFiscal == anioNum);

            var total = lista.Count();
            var paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Clamp(pagina, 1, Math.Max(1, paginas));

            // Stats
            ViewBag.StatsTotal = _utilidades.Count;
            ViewBag.StatsPendientes = _utilidades.Count(u => u.Estado == EstadoUtilidad.Pendiente || u.Estado == EstadoUtilidad.EnCalculo);
            ViewBag.StatsPagadas = _utilidades.Count(u => u.Estado == EstadoUtilidad.Pagada);
            ViewBag.ProximoPago = _utilidades
                .Where(u => u.Estado == EstadoUtilidad.Pendiente && u.FechaPagoEstimada >= DateTime.Today)
                .OrderBy(u => u.FechaPagoEstimada)
                .Select(u => (DateTime?)u.FechaPagoEstimada)
                .FirstOrDefault();
            ViewBag.TotalProyectado = _utilidades
                .Where(u => u.Estado != EstadoUtilidad.Anulada)
                .Sum(u => u.MontoDistribuido ?? u.UtilidadNetaDeclarada * u.PorcentajeParticipacion / 100m);
            ViewBag.AniosDisponibles = _utilidades.Select(u => u.EjercicioFiscal).Distinct().OrderByDescending(x => x).ToList();

            var vm = new UtilidadesViewModel
            {
                Utilidades = lista.OrderByDescending(u => u.EjercicioFiscal).ThenBy(u => u.Id)
                                    .Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                EstadoFiltro = estado ?? "",
                AnioFiltro = anio ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearUtilidad(int ejercicioFiscal, decimal porcentajeParticipacion,
            decimal utilidadNetaDeclarada, int diasComputables, decimal remuneracionComputable,
            decimal? montoDistribuido, DateTime fechaPagoEstimada,
            string empleadosAplica, int cantidadEmpleados, string estadoUtil, string? observacion)
        {
            var nextId = _utilidades.Any() ? _utilidades.Max(u => u.Id) + 1 : 1;
            Enum.TryParse<EstadoUtilidad>(estadoUtil, out var est);
            var _obj_util = new Utilidad
            {
                Id = nextId,
                Codigo = $"UTI-{nextId:D3}",
                EjercicioFiscal = ejercicioFiscal,
                PorcentajeParticipacion = porcentajeParticipacion,
                UtilidadNetaDeclarada = utilidadNetaDeclarada,
                DiasComputables = diasComputables,
                RemuneracionComputable = remuneracionComputable,
                MontoDistribuido = montoDistribuido,
                FechaPagoEstimada = fechaPagoEstimada,
                Estado = est,
                EmpleadosAplica = empleadosAplica,
                CantidadEmpleados = cantidadEmpleados,
                Observacion = observacion ?? "",
                FechaCreacion = DateTime.Today
            };
            _utilidades.Add(_obj_util);
            _db.InsertarUtilidad(_obj_util);
            TempData["MensajeUtilidad"] = $"Utilidad UTI-{nextId:D3} creada correctamente.";
            return RedirectToAction(nameof(Utilidades));
        }

        public IActionResult ObtenerUtilidad(int id)
        {
            var u = _utilidades.FirstOrDefault(x => x.Id == id);
            if (u == null) return NotFound();
            return Json(new
            {
                u.Id,
                u.EjercicioFiscal,
                u.PorcentajeParticipacion,
                u.UtilidadNetaDeclarada,
                u.DiasComputables,
                u.RemuneracionComputable,
                u.MontoDistribuido,
                fechaPagoEstimada = u.FechaPagoEstimada.ToString("yyyy-MM-dd"),
                estado = u.Estado.ToString(),
                u.EmpleadosAplica,
                u.CantidadEmpleados,
                u.Observacion
            });
        }

        [HttpPost]
        public IActionResult EditarUtilidad(int id, int ejercicioFiscal, decimal porcentajeParticipacion,
    decimal utilidadNetaDeclarada, int diasComputables, decimal remuneracionComputable,
    decimal? montoDistribuido, DateTime fechaPagoEstimada,
    string empleadosAplica, int cantidadEmpleados, string estadoUtil, string? observacion)
        {
            _utilidades = _db.ObtenerUtilidades();
            var u = _utilidades.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                Enum.TryParse<EstadoUtilidad>(estadoUtil, out var est);
                u.EjercicioFiscal = ejercicioFiscal; u.PorcentajeParticipacion = porcentajeParticipacion;
                u.UtilidadNetaDeclarada = utilidadNetaDeclarada; u.DiasComputables = diasComputables;
                u.RemuneracionComputable = remuneracionComputable; u.MontoDistribuido = montoDistribuido;
                u.FechaPagoEstimada = fechaPagoEstimada; u.Estado = est;
                u.EmpleadosAplica = empleadosAplica; u.CantidadEmpleados = cantidadEmpleados;
                u.Observacion = observacion ?? "";
                _db.ActualizarUtilidad(u);
                _utilidades = _db.ObtenerUtilidades();
            }
            TempData["MensajeUtilidad"] = $"Utilidad {u?.Codigo} actualizada.";
            return RedirectToAction(nameof(Utilidades));
        }

        [HttpPost]
        public IActionResult EliminarUtilidad(int id)
        {
            var u = _utilidades.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                _db.EliminarUtilidad(id);
                _utilidades.Remove(u); TempData["MensajeUtilidad"] = $"Utilidad {u.Codigo} eliminada.";
            }
            return RedirectToAction(nameof(Utilidades));
        }

        // ─────────────────────────────────────────────────────────
        // SUNAT / PDT
        // ─────────────────────────────────────────────────────────
        private static List<DeclaracionSunat> _declaracionesSunat = new();

        public IActionResult SunatPdt(string? buscar, string? tipo, string? periodo,
            string? estado, string? ejercicio, int pagina = 1)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            const int porPagina = 8;
            var lista = _declaracionesSunat.AsEnumerable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(d => d.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || d.NroOrden.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || d.Periodo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoPdt>(tipo, out var t))
                lista = lista.Where(d => d.Tipo == t);

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPdt>(estado, out var e))
                lista = lista.Where(d => d.Estado == e);

            if (!string.IsNullOrEmpty(periodo))
                lista = lista.Where(d => d.Periodo.Equals(periodo, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(ejercicio) && int.TryParse(ejercicio, out var ej))
                lista = lista.Where(d => d.Ejercicio == ej);

            var total = lista.Count();
            var paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Clamp(pagina, 1, Math.Max(1, paginas));

            ViewBag.StatsTotal = _declaracionesSunat.Count;
            ViewBag.StatsPendientes = _declaracionesSunat.Count(d => d.Estado == EstadoPdt.Pendiente);
            ViewBag.StatsEnviadas = _declaracionesSunat.Count(d => d.FechaEnvio.HasValue);
            ViewBag.StatsAceptadas = _declaracionesSunat.Count(d => d.Estado == EstadoPdt.Aceptada);
            ViewBag.StatsObsRechaz = _declaracionesSunat.Count(d => d.Estado == EstadoPdt.Observada || d.Estado == EstadoPdt.Rechazada);
            ViewBag.Periodos = _declaracionesSunat.Select(d => d.Periodo).Distinct().OrderByDescending(x => x).ToList();
            ViewBag.Ejercicios = _declaracionesSunat.Select(d => d.Ejercicio).Distinct().OrderByDescending(x => x).ToList();

            var vm = new SunatPdtViewModel
            {
                Declaraciones = lista.OrderByDescending(d => d.FechaGeneracion).Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                TipoFiltro = tipo ?? "",
                PeriodoFiltro = periodo ?? "",
                EstadoFiltro = estado ?? "",
                EjercicioFiltro = ejercicio ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CrearDeclaracionPdt(string tipoDecl, string periodo, int ejercicio,
    DateTime fechaGeneracion, string estadoDecl, string? nroOrden, string usuario, string? observacion)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            var nextId = _declaracionesSunat.Any() ? _declaracionesSunat.Max(d => d.Id) + 1 : 1;
            Enum.TryParse<TipoPdt>(tipoDecl, out var tip);
            Enum.TryParse<EstadoPdt>(estadoDecl, out var est);
            var nueva = new DeclaracionSunat
            {
                Codigo = $"SUN-{nextId:D3}",
                Tipo = tip,
                Periodo = periodo,
                Ejercicio = ejercicio,
                FechaGeneracion = fechaGeneracion,
                Estado = est,
                NroOrden = nroOrden ?? "",
                TieneConstancia = est == EstadoPdt.Aceptada,
                Usuario = usuario,
                FechaEnvio = est == EstadoPdt.Pendiente ? null : (DateTime?)DateTime.Now,
                Observacion = observacion ?? ""

            };
            _db.InsertarDeclaracionSunat(nueva);
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            TempData["MensajeSunat"] = $"Declaración {nueva.Codigo} creada correctamente.";
            return RedirectToAction(nameof(SunatPdt));
        }

        public IActionResult ObtenerDeclaracionPdt(int id)
        {
            var d = _declaracionesSunat.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();
            return Json(new
            {
                d.Id,
                d.Periodo,
                d.Ejercicio,
                d.NroOrden,
                d.Usuario,
                d.Observacion,
                d.TieneConstancia,
                tipo = d.Tipo.ToString(),
                estado = d.Estado.ToString(),
                fechaGeneracion = d.FechaGeneracion.ToString("yyyy-MM-ddTHH:mm"),
                fechaEnvio = d.FechaEnvio?.ToString("yyyy-MM-ddTHH:mm") ?? ""
            });
        }

        [HttpPost]
        public IActionResult EditarDeclaracionPdt(int id, string tipoDecl, string periodo, int ejercicio,
    DateTime fechaGeneracion, string estadoDecl, string? nroOrden, string usuario, string? observacion)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            var d = _declaracionesSunat.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                Enum.TryParse<TipoPdt>(tipoDecl, out var tip);
                Enum.TryParse<EstadoPdt>(estadoDecl, out var est);
                d.Tipo = tip; d.Periodo = periodo; d.Ejercicio = ejercicio;
                d.FechaGeneracion = fechaGeneracion; d.Estado = est;
                d.NroOrden = nroOrden ?? "";
                d.TieneConstancia = est == EstadoPdt.Aceptada;
                d.Usuario = usuario; d.Observacion = observacion ?? "";
                // Asignar FechaEnvio si no tenía y ahora es Enviada o Aceptada
                if (!d.FechaEnvio.HasValue && est != EstadoPdt.Pendiente && est != EstadoPdt.Rechazada)
                    d.FechaEnvio = DateTime.Now;
                _db.ActualizarDeclaracionSunat(d);
                _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            }
            TempData["MensajeSunat"] = $"Declaración {d?.Codigo} actualizada.";
            return RedirectToAction(nameof(SunatPdt));
        }

        [HttpPost]
        public IActionResult EliminarDeclaracionPdt(int id)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            var d = _declaracionesSunat.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                _db.EliminarDeclaracionSunat(id);
                _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
                TempData["MensajeSunat"] = $"Declaración {d.Codigo} eliminada.";
            }
            return RedirectToAction(nameof(SunatPdt));
        }

        [HttpPost]
        public IActionResult EnviarDeclaracionPdt(int id)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            var d = _declaracionesSunat.FirstOrDefault(x => x.Id == id);
            if (d != null && d.Estado == EstadoPdt.Pendiente)
            {
                d.FechaEnvio = DateTime.Now;
                d.Estado = EstadoPdt.Enviada;
                d.NroOrden = $"2025{new Random().Next(10000000, 99999999)}";
                _db.ActualizarDeclaracionSunat(d);
                _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
                TempData["MensajeSunat"] = $"Declaración {d.Codigo} enviada. N° orden: {d.NroOrden}";
            }
            return RedirectToAction(nameof(SunatPdt));
        }

        public IActionResult DescargarConstanciaPdt(int id)
        {
            _declaracionesSunat = _db.ObtenerDeclaracionesSunat();
            var d = _declaracionesSunat.FirstOrDefault(x => x.Id == id);
            if (d == null || !d.TieneConstancia) return NotFound();

            string tipoNombre = d.Tipo switch
            {
                TipoPdt.PLAME => "Generación de PLAME (.txt para T-Registro)",
                TipoPdt.PDT601 => "PDT 601 — Planilla Electrónica",
                _ => "AFP Net / Declaración AFP"
            };
            string estadoNombre = d.Estado switch
            {
                EstadoPdt.Aceptada => "ACEPTADA",
                EstadoPdt.Observada => "OBSERVADA",
                EstadoPdt.Rechazada => "RECHAZADA",
                EstadoPdt.Enviada => "ENVIADA",
                _ => "PENDIENTE"
            };
            string estadoColor = d.Estado switch
            {
                EstadoPdt.Aceptada => "#166534",
                EstadoPdt.Observada => "#854d0e",
                EstadoPdt.Rechazada => "#b91c1c",
                _ => "#1d4ed8"
            };
            string estadoBg = d.Estado switch
            {
                EstadoPdt.Aceptada => "#dcfce7",
                EstadoPdt.Observada => "#fef9c3",
                EstadoPdt.Rechazada => "#fdecea",
                _ => "#eff6ff"
            };

            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8""/>
<title>Constancia de Recepción — {d.Codigo}</title>
<style>
  *{{box-sizing:border-box;margin:0;padding:0;}}
  body{{font-family:'Segoe UI',Arial,sans-serif;background:#f0f2f8;display:flex;justify-content:center;padding:40px 20px;}}
  .card{{background:#fff;border-radius:16px;width:680px;box-shadow:0 4px 32px rgba(0,0,0,.12);overflow:hidden;}}
  .header{{background:linear-gradient(135deg,#4361ee,#7b8cde);padding:32px 36px;color:#fff;}}
  .header h1{{font-size:22px;font-weight:800;margin-bottom:4px;}}
  .header p{{font-size:13px;opacity:.85;}}
  .logo-row{{display:flex;align-items:center;gap:14px;margin-bottom:20px;}}
  .logo-ico{{width:48px;height:48px;background:rgba(255,255,255,.2);border-radius:12px;display:flex;align-items:center;justify-content:center;font-size:24px;}}
  .brand h2{{font-size:16px;font-weight:800;}}
  .brand p{{font-size:12px;opacity:.8;}}
  .estado-pill{{display:inline-block;background:{estadoBg};color:{estadoColor};
    border-radius:20px;padding:6px 18px;font-size:13px;font-weight:700;margin-top:12px;}}
  .body{{padding:32px 36px;}}
  .section-title{{font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.6px;
    color:#6b7280;margin-bottom:14px;padding-bottom:6px;border-bottom:1px solid #e5e7eb;}}
  .grid{{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:28px;}}
  .field{{background:#fafbff;border-radius:10px;padding:14px 16px;}}
  .field.full{{grid-column:1/-1;}}
  .f-label{{font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.5px;color:#9ca3af;margin-bottom:4px;}}
  .f-value{{font-size:14px;font-weight:700;color:#1a1a2e;}}
  .f-value.mono{{font-family:monospace;font-size:13px;}}
  .f-value.green{{color:#059669;}}
  .watermark{{text-align:center;padding:20px;color:#d1d5db;font-size:12px;border-top:1px solid #f3f4f6;}}
  .footer{{background:#fafbff;padding:20px 36px;border-top:1px solid #e5e7eb;display:flex;justify-content:space-between;align-items:center;}}
  .footer-note{{font-size:11px;color:#9ca3af;}}
  .print-btn{{background:#4361ee;color:#fff;border:none;padding:10px 22px;border-radius:8px;font-size:13px;font-weight:600;cursor:pointer;}}
  .print-btn:hover{{background:#3451d1;}}
  @media print{{body{{background:#fff;padding:0;}}.card{{box-shadow:none;border-radius:0;width:100%;}}.footer{{display:none;}}}}
</style>
</head>
<body>
<div class=""card"">
  <div class=""header"">
    <div class=""logo-row"">
      <div class=""logo-ico"">🏛️</div>
      <div class=""brand""><h2>SUNAT / PDT</h2><p>Sistema de Gestión Empresarial</p></div>
    </div>
    <h1>Constancia de Recepción</h1>
    <p>Declaración Telemática — {tipoNombre}</p>
    <div class=""estado-pill"">{estadoNombre}</div>
  </div>

  <div class=""body"">
    <div class=""section-title"">Datos de la Declaración</div>
    <div class=""grid"">
      <div class=""field""><div class=""f-label"">Código</div><div class=""f-value"">{d.Codigo}</div></div>
      <div class=""field""><div class=""f-label"">Tipo de Declaración</div><div class=""f-value"">{d.Tipo}</div></div>
      <div class=""field""><div class=""f-label"">Período</div><div class=""f-value"">{d.Periodo}</div></div>
      <div class=""field""><div class=""f-label"">Ejercicio Fiscal</div><div class=""f-value"">{d.Ejercicio}</div></div>
      <div class=""field""><div class=""f-label"">Fecha de Generación</div><div class=""f-value"">{d.FechaGeneracion:dd/MM/yyyy HH:mm}</div></div>
      <div class=""field""><div class=""f-label"">Fecha de Envío</div><div class=""f-value"">{(d.FechaEnvio.HasValue ? d.FechaEnvio.Value.ToString("dd/MM/yyyy HH:mm") : "—")}</div></div>
    </div>

    <div class=""section-title"">Constancia SUNAT</div>
    <div class=""grid"">
      <div class=""field full""><div class=""f-label"">Nº de Orden</div><div class=""f-value mono green"">{d.NroOrden}</div></div>
      <div class=""field""><div class=""f-label"">Estado de Recepción</div><div class=""f-value"">{estadoNombre}</div></div>
      <div class=""field""><div class=""f-label"">Usuario Declarante</div><div class=""f-value"">{d.Usuario}</div></div>
      {(string.IsNullOrEmpty(d.Observacion) ? "" : $@"<div class=""field full""><div class=""f-label"">Observaciones</div><div class=""f-value"" style=""font-weight:400;"">{d.Observacion}</div></div>")}
    </div>

    <div class=""watermark"">
      Documento generado el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm:ss} — SGE Nómina / Planilla
    </div>
  </div>

  <div class=""footer"">
    <span class=""footer-note"">Este documento es una constancia generada por el sistema. Consérvela para sus registros.</span>
    <button class=""print-btn"" onclick=""window.print()"">🖨️ Imprimir</button>
  </div>
</div>
</body>
</html>";

            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            return Content(html, "text/html", System.Text.Encoding.UTF8);
        }

        // ── Historial de Pagos en memoria ─────────────────────────
        private static List<PagoPlanilla> _pagos = new()
        {
            new() { Id=1,  Codigo="PAG-2024-05", PlanillaConcepto="Planilla Mensual - Mayo 2024",       Periodo="Mayo 2024",       FechaPago=new DateTime(2024,5,31),  Banco=MedioPago.BCP,          MontoPagado=256780m,  Estado=EstadoPago.Pagado,    Empleados=128, Observacion="" },
            new() { Id=2,  Codigo="PAG-2024-04", PlanillaConcepto="Planilla Mensual - Abril 2024",      Periodo="Abril 2024",      FechaPago=new DateTime(2024,4,30),  Banco=MedioPago.BCP,          MontoPagado=244650m,  Estado=EstadoPago.Pagado,    Empleados=125, Observacion="" },
            new() { Id=3,  Codigo="PAG-2024-03", PlanillaConcepto="Planilla Mensual - Marzo 2024",      Periodo="Marzo 2024",      FechaPago=new DateTime(2024,3,31),  Banco=MedioPago.BCP,          MontoPagado=241320m,  Estado=EstadoPago.Pagado,    Empleados=122, Observacion="" },
            new() { Id=4,  Codigo="PAG-2024-02", PlanillaConcepto="Planilla Mensual - Febrero 2024",    Periodo="Febrero 2024",    FechaPago=new DateTime(2024,2,29),  Banco=MedioPago.BCP,          MontoPagado=237890m,  Estado=EstadoPago.Pagado,    Empleados=120, Observacion="" },
            new() { Id=5,  Codigo="PAG-2024-01", PlanillaConcepto="Planilla Mensual - Enero 2024",      Periodo="Enero 2024",      FechaPago=new DateTime(2024,1,31),  Banco=MedioPago.BCP,          MontoPagado=235780m,  Estado=EstadoPago.Pagado,    Empleados=118, Observacion="" },
            new() { Id=6,  Codigo="PAG-2023-12", PlanillaConcepto="Planilla Mensual - Diciembre 2023",  Periodo="Diciembre 2023",  FechaPago=new DateTime(2023,12,29), Banco=MedioPago.BCP,          MontoPagado=233670m,  Estado=EstadoPago.Pagado,    Empleados=118, Observacion="" },
            new() { Id=7,  Codigo="PAG-2023-11", PlanillaConcepto="Planilla Mensual - Noviembre 2023",  Periodo="Noviembre 2023",  FechaPago=new DateTime(2023,11,30), Banco=MedioPago.BBVA,         MontoPagado=230620m,  Estado=EstadoPago.Pagado,    Empleados=115, Observacion="" },
            new() { Id=8,  Codigo="PAG-2023-10", PlanillaConcepto="Planilla Mensual - Octubre 2023",    Periodo="Octubre 2023",    FechaPago=new DateTime(2023,10,31), Banco=MedioPago.BBVA,         MontoPagado=229690m,  Estado=EstadoPago.Pagado,    Empleados=115, Observacion="" },
            new() { Id=9,  Codigo="PAG-2023-09", PlanillaConcepto="Planilla Mensual - Septiembre 2023", Periodo="Septiembre 2023", FechaPago=new DateTime(2023,9,30),  Banco=MedioPago.Interbank,    MontoPagado=225700m,  Estado=EstadoPago.Pagado,    Empleados=112, Observacion="" },
            new() { Id=10, Codigo="PAG-2023-08", PlanillaConcepto="Planilla Mensual - Agosto 2023",     Periodo="Agosto 2023",     FechaPago=new DateTime(2023,8,31),  Banco=MedioPago.Interbank,    MontoPagado=222100m,  Estado=EstadoPago.Pagado,    Empleados=110, Observacion="" },
            new() { Id=11, Codigo="PAG-2023-07", PlanillaConcepto="Planilla Mensual - Julio 2023",      Periodo="Julio 2023",      FechaPago=new DateTime(2023,7,31),  Banco=MedioPago.Scotiabank,   MontoPagado=220200m,  Estado=EstadoPago.Pagado,    Empleados=110, Observacion="" },
            new() { Id=12, Codigo="PAG-2023-06", PlanillaConcepto="Planilla Mensual - Junio 2023",      Periodo="Junio 2023",      FechaPago=new DateTime(2023,6,30),  Banco=MedioPago.Scotiabank,   MontoPagado=216600m,  Estado=EstadoPago.Pagado,    Empleados=108, Observacion="" },
            new() { Id=13, Codigo="PAG-GRAT-2023",PlanillaConcepto="Gratificación Julio 2023",          Periodo="Julio 2023",      FechaPago=new DateTime(2023,7,15),  Banco=MedioPago.BCP,          MontoPagado=148500m,  Estado=EstadoPago.Pagado,    Empleados=110, Observacion="Gratificación Fiestas Patrias" },
            new() { Id=14, Codigo="PAG-GRAT-DIC", PlanillaConcepto="Gratificación Diciembre 2023",      Periodo="Diciembre 2023",  FechaPago=new DateTime(2023,12,15), Banco=MedioPago.BCP,          MontoPagado=152000m,  Estado=EstadoPago.Pagado,    Empleados=118, Observacion="Gratificación Navidad" },
            new() { Id=15, Codigo="PAG-CTS-2023", PlanillaConcepto="CTS Mayo 2023",                     Periodo="Mayo 2023",       FechaPago=new DateTime(2023,5,15),  Banco=MedioPago.BBVA,         MontoPagado=98400m,   Estado=EstadoPago.Pagado,    Empleados=108, Observacion="Depósito CTS semestral" },
            new() { Id=16, Codigo="PAG-CTS-NOV",  PlanillaConcepto="CTS Noviembre 2023",                Periodo="Noviembre 2023",  FechaPago=new DateTime(2023,11,15), Banco=MedioPago.BBVA,         MontoPagado=101200m,  Estado=EstadoPago.Pagado,    Empleados=115, Observacion="Depósito CTS semestral" },
            new() { Id=17, Codigo="PAG-UTI-2022", PlanillaConcepto="Utilidades 2022",                   Periodo="Abril 2023",      FechaPago=new DateTime(2023,4,20),  Banco=MedioPago.Transferencia,MontoPagado=320000m,  Estado=EstadoPago.Pagado,    Empleados=108, Observacion="Reparto utilidades ejercicio 2022" },
            new() { Id=18, Codigo="PAG-ADL-001",  PlanillaConcepto="Adelanto Quincena Mayo 2024",       Periodo="Mayo 2024",       FechaPago=new DateTime(2024,5,15),  Banco=MedioPago.BCP,          MontoPagado=85000m,   Estado=EstadoPago.Pagado,    Empleados=128, Observacion="Adelanto de quincena" },
            new() { Id=19, Codigo="PAG-PEND-001", PlanillaConcepto="Planilla Mensual - Junio 2024",     Periodo="Junio 2024",      FechaPago=new DateTime(2024,6,30),  Banco=MedioPago.BCP,          MontoPagado=260000m,  Estado=EstadoPago.Pendiente, Empleados=130, Observacion="Pendiente de autorización" },
            new() { Id=20, Codigo="PAG-PEND-002", PlanillaConcepto="Gratificación Julio 2024",          Periodo="Julio 2024",      FechaPago=new DateTime(2024,7,15),  Banco=MedioPago.BCP,          MontoPagado=155000m,  Estado=EstadoPago.EnProceso, Empleados=130, Observacion="En validación de montos" },
        };

        // ─────────────────────────────────────────────────────────
        // HISTORIAL DE PAGOS
        // ─────────────────────────────────────────────────────────
        public IActionResult HistorialPagos(string? buscar, string? estado, string? medio, string? periodo, int pagina = 1)
        {
            _pagos = _db.ObtenerPagos();
            int porPagina = 6;
            var lista = _pagos.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(p =>
                    p.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    p.PlanillaConcepto.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    p.Periodo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoPago>(estado, out var est))
                lista = lista.Where(p => p.Estado == est);

            if (!string.IsNullOrEmpty(medio) && Enum.TryParse<MedioPago>(medio, out var med))
                lista = lista.Where(p => p.Banco == med);

            if (!string.IsNullOrEmpty(periodo))
                lista = lista.Where(p => p.Periodo.Contains(periodo, StringComparison.OrdinalIgnoreCase));

            int total = lista.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            ViewBag.TotalPagos = _pagos.Count;
            ViewBag.TotalPagado = _pagos.Where(p => p.Estado == EstadoPago.Pagado).Sum(p => p.MontoPagado);
            ViewBag.CountPagado = _pagos.Count(p => p.Estado == EstadoPago.Pagado);
            ViewBag.CountPendiente = _pagos.Count(p => p.Estado == EstadoPago.Pendiente || p.Estado == EstadoPago.EnProceso);
            ViewBag.MontoMesActual = _pagos.Where(p => p.FechaPago.Month == DateTime.Today.Month && p.FechaPago.Year == DateTime.Today.Year).Sum(p => p.MontoPagado);

            var vm = new HistorialPagosViewModel
            {
                Pagos = lista.OrderByDescending(p => p.FechaPago).Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                Buscar = buscar ?? "",
                EstadoFiltro = estado ?? "",
                MedioFiltro = medio ?? "",
                PeriodoFiltro = periodo ?? "",
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult CrearPago(string planillaConcepto, string periodo, DateTime fechaPago,
    string banco, decimal montoPagado, string estado, int empleados, string? observacion)
        {
            _pagos = _db.ObtenerPagos();
            var nextId = _pagos.Any() ? _pagos.Max(p => p.Id) + 1 : 1;
            Enum.TryParse<EstadoPago>(estado, out var est);
            var nuevo = new PagoPlanilla
            {
                Codigo = $"PAG-{nextId:D3}",
                PlanillaConcepto = planillaConcepto,
                Periodo = periodo,
                FechaPago = fechaPago,
                Banco = Enum.TryParse<MedioPago>(banco, out var b) ? b : MedioPago.BCP,
                MontoPagado = montoPagado,
                Estado = est,
                Empleados = empleados,
                Observacion = observacion ?? "",

            };
            _db.InsertarPago(nuevo);
            _pagos = _db.ObtenerPagos();
            TempData["MensajePago"] = $"Pago {nuevo.Codigo} registrado correctamente.";
            return RedirectToAction(nameof(HistorialPagos));
        }

        [HttpPost]
        public IActionResult EditarPago(int id, string planillaConcepto, string periodo, DateTime fechaPago,
    string banco, decimal montoPagado, string estado, int empleados, string? observacion)
        {
            _pagos = _db.ObtenerPagos();
            var p = _pagos.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                Enum.TryParse<MedioPago>(banco, out var med);
                Enum.TryParse<EstadoPago>(estado, out var est);
                p.PlanillaConcepto = planillaConcepto;
                p.Periodo = periodo;
                p.FechaPago = fechaPago;
                p.Banco = med;
                p.MontoPagado = montoPagado;
                p.Estado = est;
                p.Empleados = empleados;
                p.Observacion = observacion ?? "";
                _db.ActualizarPago(p);
                _pagos = _db.ObtenerPagos();
            }
            TempData["MensajePago"] = $"Pago {p?.Codigo} actualizado.";
            return RedirectToAction(nameof(HistorialPagos));
        }

        [HttpPost]
        public IActionResult EliminarPago(int id)
        {
            _pagos = _db.ObtenerPagos();
            var p = _pagos.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                _db.EliminarPago(id);
                _pagos = _db.ObtenerPagos();
                TempData["MensajePago"] = $"Pago {p.Codigo} eliminado.";
            }
            return RedirectToAction(nameof(HistorialPagos));
        }

        [HttpGet]
        public IActionResult ObtenerPago(int id)
        {
            var p = _pagos.FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Json(new
            {
                p.Id,
                p.Codigo,
                p.PlanillaConcepto,
                p.Periodo,
                fechaPago = p.FechaPago.ToString("yyyy-MM-dd"),
                banco = p.Banco.ToString(),
                p.MontoPagado,
                estado = p.Estado.ToString(),
                p.Empleados,
                p.Observacion
            });
        }


        // ═══════════════════════════════════════════════════════════
        // CONFIGURACIÓN — stores en memoria
        // ═══════════════════════════════════════════════════════════
        private static ParametrosGenerales _params = new();

        private static List<RangoRenta> _rangos = new()
        {
            new() { Id=1, Desde=0m,        Hasta=5000m,    Tasa=8m,  MontoFijo=0m,      Activo=true },
            new() { Id=2, Desde=5000.01m,  Hasta=20000m,   Tasa=14m, MontoFijo=400m,    Activo=true },
            new() { Id=3, Desde=20000.01m, Hasta=35000m,   Tasa=17m, MontoFijo=1500m,   Activo=true },
            new() { Id=4, Desde=35000.01m, Hasta=45000m,   Tasa=20m, MontoFijo=3600m,   Activo=true },
            new() { Id=5, Desde=45000.01m, Hasta=null,     Tasa=30m, MontoFijo=5600m,   Activo=true },
        };

        private static List<BancoConfig> _bancos = new()
        {
            new() { Id=1, Nombre="Banco de Crédito del Perú", Codigo="002", Moneda="Soles (S/)",    CuentaPrincipal="793-2215-647-0-35",    Activo=true, Emoji="🔵" },
            new() { Id=2, Nombre="BBVA Perú",                 Codigo="001", Moneda="Soles (S/)",    CuentaPrincipal="0011-0332-0201234567", Activo=true, Emoji="🔷" },
            new() { Id=3, Nombre="Interbank",                  Codigo="003", Moneda="Soles (S/)",    CuentaPrincipal="011-3031234567",       Activo=true, Emoji="🟢" },
            new() { Id=4, Nombre="Scotiabank Perú",            Codigo="009", Moneda="Soles (S/)",    CuentaPrincipal="000-1701-2345678",     Activo=true, Emoji="🔴" },
            new() { Id=5, Nombre="Banco Continental",          Codigo="007", Moneda="Dólares (US$)", CuentaPrincipal="0011-0210-0100001234", Activo=true, Emoji="🟠" },
        };

        private static List<Feriado> _feriados = new()
        {
            new() { Id=1,  Fecha=new DateTime(2026,1,1),  Nombre="Año Nuevo",          Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=2,  Fecha=new DateTime(2026,4,2),  Nombre="Jueves Santo",       Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=3,  Fecha=new DateTime(2026,4,3),  Nombre="Viernes Santo",      Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=4,  Fecha=new DateTime(2026,5,1),  Nombre="Día del Trabajo",    Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=5,  Fecha=new DateTime(2026,5,29), Nombre="San Pedro y San Pablo",Tipo="Nacional",   Recuperable=true,  Activo=true },
            new() { Id=6,  Fecha=new DateTime(2026,7,28), Nombre="Fiestas Patrias",    Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=7,  Fecha=new DateTime(2026,8,6),  Nombre="Batalla de Junín",   Tipo="Nacional",     Recuperable=true,  Activo=true },
            new() { Id=8,  Fecha=new DateTime(2026,8,30), Nombre="Santa Rosa de Lima", Tipo="Nacional",     Recuperable=false, Activo=true },
            new() { Id=9,  Fecha=new DateTime(2026,10,8), Nombre="Combate de Angamos", Tipo="Nacional",     Recuperable=true,  Activo=true },
            new() { Id=10, Fecha=new DateTime(2026,11,1), Nombre="Día de Todos los Santos",Tipo="Nacional", Recuperable=false, Activo=true },
            new() { Id=11, Fecha=new DateTime(2026,12,8), Nombre="Inmaculada Concepción",Tipo="Nacional",   Recuperable=false, Activo=true },
            new() { Id=12, Fecha=new DateTime(2026,12,25),Nombre="Navidad",            Tipo="Nacional",     Recuperable=false, Activo=true },
        };

        private static List<CentroCosto> _centros = new()
        {
            new() { Id=1, Codigo="ADM",  Nombre="Administración",   Descripcion="Área administrativa y gerencial",  Responsable="María López",   Activo=true },
            new() { Id=2, Codigo="FIN",  Nombre="Finanzas",         Descripcion="Área de finanzas y contabilidad",  Responsable="Carlos Ramírez", Activo=true },
            new() { Id=3, Codigo="RRHH", Nombre="Recursos Humanos", Descripcion="Gestión de talento humano",        Responsable="Laura Sánchez",  Activo=true },
            new() { Id=4, Codigo="VENT", Nombre="Ventas",           Descripcion="Área comercial y ventas",          Responsable="Pedro Torres",   Activo=true },
            new() { Id=5, Codigo="TI",   Nombre="Tecnología",       Descripcion="Sistemas y desarrollo",            Responsable="John Pérez",     Activo=true },
            new() { Id=6, Codigo="LOG",  Nombre="Logística",        Descripcion="Compras y logística",              Responsable="Ana Gómez",      Activo=true },
        };

        private static List<UsuarioNomina> _usuariosNom = new();

        // ─── Configuración — GET principal ───────────────────────────
        public IActionResult Configuracion(string seccion = "parametros")
        {
            _params = _db.ObtenerParametros();
            _centros = _db.ObtenerCentros();
            _usuariosNom = _db.ObtenerUsuariosNom();
            _rangos = _db.ObtenerRangos();
            _bancos = _db.ObtenerBancos();
            _feriados = _db.ObtenerFeriados();
            PrepararViewBagConfig(seccion);
            return View();
        }

        private void PrepararViewBagConfig(string seccion)
        {
            ViewBag.Seccion = seccion;
            ViewBag.Params = _params;
            ViewBag.Rangos = _rangos;
            ViewBag.Bancos = _bancos;
            ViewBag.Feriados = _feriados;
            ViewBag.Centros = _centros;
            ViewBag.UsuariosNom = _usuariosNom;
        }

        // ─── Parámetros Generales ────────────────────────────────────
        [HttpPost]
        public IActionResult GuardarParametros(string empresa, string moneda,
    int diaCierre, int diaPago, bool calcHoras = false, bool inclFeriados = false)
        {
            _params.Empresa = empresa;
            _params.Moneda = moneda;
            _params.DiaCierrePlanilla = diaCierre;
            _params.DiaPagoPlanilla = diaPago;
            _params.CalcHorasExtrasAuto = calcHoras;
            _params.InclFeriadosAsist = inclFeriados;
            _db.GuardarParametros(_params);
            TempData["MsgConfig"] = "Parámetros guardados correctamente.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "parametros" });
        }

        // ─── Rangos de Renta ─────────────────────────────────────────
        [HttpPost]
        public IActionResult CrearRango(decimal desde, decimal? hasta, decimal tasa, decimal montoFijo)
        {
            var nuevo = new RangoRenta { Desde = desde, Hasta = hasta, Tasa = tasa, MontoFijo = montoFijo, Activo = true };
            _db.InsertarRango(nuevo);
            _rangos = _db.ObtenerRangos();
            TempData["MsgConfig"] = "Rango de renta creado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "rangos" });
        }

        [HttpPost]
        public IActionResult EditarRango(int id, decimal desde, decimal? hasta, decimal tasa, decimal montoFijo)
        {
            _rangos = _db.ObtenerRangos();
            var r = _rangos.FirstOrDefault(x => x.Id == id);
            if (r != null)
            {
                r.Desde = desde; r.Hasta = hasta; r.Tasa = tasa; r.MontoFijo = montoFijo;
                _db.ActualizarRango(r);
                _rangos = _db.ObtenerRangos();
            }
            TempData["MsgConfig"] = "Rango actualizado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "rangos" });
        }

        [HttpPost]
        public IActionResult EliminarRango(int id)
        {
            _db.EliminarRango(id);
            _rangos = _db.ObtenerRangos();
            TempData["MsgConfig"] = "Rango eliminado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "rangos" });
        }

        [HttpGet]
        public IActionResult ObtenerRango(int id)
        {
            var r = _rangos.FirstOrDefault(x => x.Id == id);
            if (r == null) return NotFound();
            return Json(new { r.Id, r.Desde, hasta = r.Hasta, r.Tasa, r.MontoFijo, r.Activo });
        }

        // ─── Bancos ──────────────────────────────────────────────────
        [HttpPost]
        public IActionResult CrearBanco(string nombre, string codigo, string moneda, string cuentaPrincipal)
        {
            var nuevo = new BancoConfig { Nombre = nombre, Codigo = codigo, Moneda = moneda, CuentaPrincipal = cuentaPrincipal, Activo = true, Emoji = "🏦" };
            _db.InsertarBanco(nuevo);
            _bancos = _db.ObtenerBancos();
            TempData["MsgConfig"] = $"Banco \"{nombre}\" agregado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "bancos" });
        }

        [HttpPost]
        public IActionResult EditarBanco(int id, string nombre, string codigo, string moneda, string cuentaPrincipal)
        {
            _bancos = _db.ObtenerBancos();
            var b = _bancos.FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                b.Nombre = nombre; b.Codigo = codigo; b.Moneda = moneda; b.CuentaPrincipal = cuentaPrincipal;
                _db.ActualizarBanco(b);
                _bancos = _db.ObtenerBancos();
            }
            TempData["MsgConfig"] = $"Banco \"{nombre}\" actualizado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "bancos" });
        }

        [HttpPost]
        public IActionResult EliminarBanco(int id)
        {
            _db.EliminarBanco(id);
            _bancos = _db.ObtenerBancos();
            TempData["MsgConfig"] = "Banco eliminado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "bancos" });
        }

        [HttpGet]
        public IActionResult ObtenerBanco(int id)
        {
            var b = _bancos.FirstOrDefault(x => x.Id == id);
            if (b == null) return NotFound();
            return Json(new { b.Id, b.Nombre, b.Codigo, b.Moneda, b.CuentaPrincipal, b.Activo });
        }

        // ─── Feriados ────────────────────────────────────────────────
        [HttpPost]
        public IActionResult CrearFeriado(DateTime fecha, string feriado_nombre, string tipo, bool recuperable = false)
        {
            var nuevo = new Feriado { Fecha = fecha, Nombre = feriado_nombre, Tipo = tipo, Recuperable = recuperable, Activo = true };
            _db.InsertarFeriado(nuevo);
            _feriados = _db.ObtenerFeriados();
            TempData["MsgConfig"] = $"Feriado \"{feriado_nombre}\" agregado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "feriados" });
        }

        [HttpPost]
        public IActionResult EditarFeriado(int id, DateTime fecha, string feriado_nombre, string tipo, bool recuperable = false)
        {
            _feriados = _db.ObtenerFeriados();
            var f = _feriados.FirstOrDefault(x => x.Id == id);
            if (f != null)
            {
                f.Fecha = fecha; f.Nombre = feriado_nombre; f.Tipo = tipo; f.Recuperable = recuperable;
                _db.ActualizarFeriado(f);
                _feriados = _db.ObtenerFeriados();
            }
            TempData["MsgConfig"] = $"Feriado \"{feriado_nombre}\" actualizado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "feriados" });
        }

        [HttpPost]
        public IActionResult EliminarFeriado(int id)
        {
            _db.EliminarFeriado(id);
            _feriados = _db.ObtenerFeriados();
            TempData["MsgConfig"] = "Feriado eliminado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "feriados" });
        }
        [HttpGet]
        public IActionResult ObtenerFeriado(int id)
        {
            var f = _feriados.FirstOrDefault(x => x.Id == id);
            if (f == null) return NotFound();
            return Json(new { f.Id, fecha = f.Fecha.ToString("yyyy-MM-dd"), f.Nombre, f.Tipo, f.Recuperable, f.Activo });
        }

        // ─── Centros de Costo ────────────────────────────────────────
        [HttpPost]
        public IActionResult CrearCentro(string centro_codigo, string centro_nombre, string descripcion, string responsable)
        {
            var nuevo = new CentroCosto { Codigo = centro_codigo, Nombre = centro_nombre, Descripcion = descripcion, Responsable = responsable, Activo = true };
            _db.InsertarCentro(nuevo);
            _centros = _db.ObtenerCentros();
            TempData["MsgConfig"] = $"Centro de costo \"{centro_nombre}\" creado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "centros" });
        }

        [HttpPost]
        public IActionResult EditarCentro(int id, string centro_codigo, string centro_nombre, string descripcion, string responsable)
        {
            _centros = _db.ObtenerCentros();
            var c = _centros.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                c.Codigo = centro_codigo; c.Nombre = centro_nombre; c.Descripcion = descripcion; c.Responsable = responsable;
                _db.ActualizarCentro(c);
                _centros = _db.ObtenerCentros();
            }
            TempData["MsgConfig"] = $"Centro \"{centro_nombre}\" actualizado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "centros" });
        }

        [HttpPost]
        public IActionResult EliminarCentro(int id)
        {
            _db.EliminarCentro(id);
            _centros = _db.ObtenerCentros();
            TempData["MsgConfig"] = "Centro de costo eliminado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "centros" });
        }

        [HttpGet]
        public IActionResult ObtenerCentro(int id)
        {
            var c = _centros.FirstOrDefault(x => x.Id == id);
            if (c == null) return NotFound();
            return Json(new { c.Id, c.Codigo, c.Nombre, c.Descripcion, c.Responsable, c.Activo });
        }

        // ─── Usuarios de Nómina ──────────────────────────────────────
        [HttpPost]
        public IActionResult CrearUsuarioNom(string usuario, string usuario_nombre, string rol, string email)
        {
            var nuevo = new UsuarioNomina { Usuario = usuario, Nombre = usuario_nombre, Rol = rol, Email = email, Activo = true, Emoji = "👤" };
            _db.InsertarUsuarioNom(nuevo);
            _usuariosNom = _db.ObtenerUsuariosNom();
            TempData["MsgConfig"] = $"Usuario \"{usuario}\" creado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "usuarios" });
        }

        [HttpPost]
        public IActionResult EditarUsuarioNom(int id, string usuario, string usuario_nombre, string rol, string email)
        {
            _usuariosNom = _db.ObtenerUsuariosNom();
            var u = _usuariosNom.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                u.Usuario = usuario; u.Nombre = usuario_nombre; u.Rol = rol; u.Email = email;
                _db.ActualizarUsuarioNom(u);
                _usuariosNom = _db.ObtenerUsuariosNom();
            }
            TempData["MsgConfig"] = $"Usuario \"{usuario}\" actualizado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "usuarios" });
        }

        [HttpPost]
        public IActionResult EliminarUsuarioNom(int id)
        {
            _db.EliminarUsuarioNom(id);
            _usuariosNom = _db.ObtenerUsuariosNom();
            TempData["MsgConfig"] = "Usuario eliminado.";
            return RedirectToAction(nameof(Configuracion), new { seccion = "usuarios" });
        }
        [HttpGet]
        public IActionResult ObtenerUsuarioNom(int id)
        {
            var u = _usuariosNom.FirstOrDefault(x => x.Id == id);
            if (u == null) return NotFound();
            return Json(new { u.Id, u.Usuario, u.Nombre, u.Rol, u.Email, u.Activo });
        }

        // ─────────────────────────────────────────────────────────
        // REPORTES
        // ─────────────────────────────────────────────────────────
        private static List<Reporte> _reportes = new()
        {
            new() { Id=56, Codigo="REP-2024-056", Nombre="Resumen de Nómina",             Submodulo="Planillas",        Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,20,10,30,0), GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=128, TamañoKb=245 },
            new() { Id=55, Codigo="REP-2024-055", Nombre="Detalle de Pagos por Empleado", Submodulo="Planillas",        Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,20,10,25,0), GeneradoPor="Administrador", Estado="Completado",  Formato="Excel", FilasGeneradas=512, TamañoKb=890 },
            new() { Id=54, Codigo="REP-2024-054", Nombre="Descuentos Aplicados",          Submodulo="Descuentos",       Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,20,10,20,0), GeneradoPor="Administrador", Estado="Completado",  Formato="Excel", FilasGeneradas=384, TamañoKb=620 },
            new() { Id=53, Codigo="REP-2024-053", Nombre="Aportes y Contribuciones",      Submodulo="Beneficios",       Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,20,10,15,0), GeneradoPor="Laura Sánchez", Estado="Completado",  Formato="PDF",   FilasGeneradas=128, TamañoKb=310 },
            new() { Id=52, Codigo="REP-2024-052", Nombre="Historial de Planillas",        Submodulo="Historial",        Periodo="Abril 2024",   FechaGeneracion=new DateTime(2024,5,1,9,10,0),   GeneradoPor="Administrador", Estado="Completado",  Formato="CSV",   FilasGeneradas=1240, TamañoKb=128 },
            new() { Id=51, Codigo="REP-2024-051", Nombre="Proyección de Nómina",          Submodulo="Planillas",        Periodo="Junio 2024",   FechaGeneracion=new DateTime(2024,5,20,11,5,0),  GeneradoPor="Administrador", Estado="En Proceso",  Formato="Excel", FilasGeneradas=0,   TamañoKb=0   },
            new() { Id=50, Codigo="REP-2024-050", Nombre="Conceptos por Empleado",        Submodulo="Conceptos",        Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,19,14,0,0),  GeneradoPor="Pedro Torres",  Estado="Completado",  Formato="PDF",   FilasGeneradas=256, TamañoKb=420 },
            new() { Id=49, Codigo="REP-2024-049", Nombre="Beneficios Otorgados",          Submodulo="Beneficios",       Periodo="Mayo 2024",    FechaGeneracion=new DateTime(2024,5,18,9,30,0),  GeneradoPor="Laura Sánchez", Estado="Completado",  Formato="Excel", FilasGeneradas=320, TamañoKb=540 },
            new() { Id=48, Codigo="REP-2024-048", Nombre="Resumen de Descuentos",         Submodulo="Descuentos",       Periodo="Abril 2024",   FechaGeneracion=new DateTime(2024,4,30,16,0,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=128, TamañoKb=280 },
            new() { Id=47, Codigo="REP-2024-047", Nombre="Detalle AFP y ONP",             Submodulo="Descuentos",       Periodo="Abril 2024",   FechaGeneracion=new DateTime(2024,4,30,15,30,0), GeneradoPor="Administrador", Estado="Completado",  Formato="Excel", FilasGeneradas=128, TamañoKb=390 },
            new() { Id=46, Codigo="REP-2024-046", Nombre="Resumen de Nómina",             Submodulo="Planillas",        Periodo="Abril 2024",   FechaGeneracion=new DateTime(2024,4,30,10,0,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=125, TamañoKb=238 },
            new() { Id=45, Codigo="REP-2024-045", Nombre="Liquidación de Beneficios",     Submodulo="Beneficios",       Periodo="Abril 2024",   FechaGeneracion=new DateTime(2024,4,29,11,0,0),  GeneradoPor="María López",   Estado="Error",       Formato="PDF",   FilasGeneradas=0,   TamañoKb=0   },
            new() { Id=44, Codigo="REP-2024-044", Nombre="Historial de Pagos Detallado",  Submodulo="Historial",        Periodo="Marzo 2024",   FechaGeneracion=new DateTime(2024,4,1,9,0,0),    GeneradoPor="Administrador", Estado="Completado",  Formato="CSV",   FilasGeneradas=980, TamañoKb=105 },
            new() { Id=43, Codigo="REP-2024-043", Nombre="Resumen de Nómina",             Submodulo="Planillas",        Periodo="Marzo 2024",   FechaGeneracion=new DateTime(2024,3,31,10,0,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=122, TamañoKb=231 },
            new() { Id=42, Codigo="REP-2024-042", Nombre="Conceptos Variables",           Submodulo="Conceptos",        Periodo="Marzo 2024",   FechaGeneracion=new DateTime(2024,3,30,14,0,0),  GeneradoPor="Pedro Torres",  Estado="Completado",  Formato="Excel", FilasGeneradas=198, TamañoKb=310 },
            new() { Id=41, Codigo="REP-2024-041", Nombre="Detalle de Pagos por Empleado", Submodulo="Planillas",        Periodo="Marzo 2024",   FechaGeneracion=new DateTime(2024,3,31,11,0,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="Excel", FilasGeneradas=488, TamañoKb=860 },
            new() { Id=40, Codigo="REP-2024-040", Nombre="Descuentos Voluntarios",        Submodulo="Descuentos",       Periodo="Febrero 2024", FechaGeneracion=new DateTime(2024,2,29,10,0,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=120, TamañoKb=195 },
            new() { Id=39, Codigo="REP-2024-039", Nombre="Resumen de Nómina",             Submodulo="Planillas",        Periodo="Febrero 2024", FechaGeneracion=new DateTime(2024,2,29,9,30,0),  GeneradoPor="Administrador", Estado="Completado",  Formato="PDF",   FilasGeneradas=120, TamañoKb=226 },
            new() { Id=38, Codigo="REP-2024-038", Nombre="Aportes y Contribuciones",      Submodulo="Beneficios",       Periodo="Febrero 2024", FechaGeneracion=new DateTime(2024,2,28,14,0,0),  GeneradoPor="Laura Sánchez", Estado="En Proceso",  Formato="Excel", FilasGeneradas=0,   TamañoKb=0   },
            new() { Id=37, Codigo="REP-2024-037", Nombre="Historial de Planillas",        Submodulo="Historial",        Periodo="Enero 2024",   FechaGeneracion=new DateTime(2024,2,1,8,0,0),    GeneradoPor="Administrador", Estado="Completado",  Formato="CSV",   FilasGeneradas=720, TamañoKb=78  },
        };

        private static int _reporteIdSeq = 57;

        public IActionResult Reportes(
            string? buscar = null,
            string? submodulo = null,
            string? estado = null,
            string? periodo = null,
            string? formato = null,
            int pagina = 1)
        {
            _reportes = _db.ObtenerReportes();
            const int porPagina = 10;

            var q = _reportes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(buscar))
                q = q.Where(r => r.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                              || r.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(submodulo))
                q = q.Where(r => r.Submodulo == submodulo);
            if (!string.IsNullOrWhiteSpace(estado))
                q = q.Where(r => r.Estado == estado);
            if (!string.IsNullOrWhiteSpace(periodo))
                q = q.Where(r => r.Periodo.Contains(periodo, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(formato))
                q = q.Where(r => r.Formato == formato);

            var total = q.Count();
            var paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));
            var items = q.OrderByDescending(r => r.FechaGeneracion)
                            .Skip((pagina - 1) * porPagina).Take(porPagina).ToList();

            // KPI stats
            var all = _reportes;
            ViewBag.TotalReportes = all.Count;
            ViewBag.Completados = all.Count(r => r.Estado == "Completado");
            ViewBag.EnProceso = all.Count(r => r.Estado == "En Proceso");
            ViewBag.Errores = all.Count(r => r.Estado == "Error");
            ViewBag.TotalDescargas = all.Where(r => r.Estado == "Completado").Sum(r => r.FilasGeneradas) / 1000; // "miles de filas"

            var vm = new ReportesViewModel
            {
                Reportes = items,
                PaginaActual = pagina,
                TotalPaginas = paginas,
                TotalItems = total,
                BuscarFiltro = buscar ?? "",
                SubmoduloFiltro = submodulo ?? "",
                EstadoFiltro = estado ?? "",
                PeriodoFiltro = periodo ?? "",
                FormatoFiltro = formato ?? "",
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult GenerarReporte(string nombre, string submodulo, string periodo, string formato)
        {
            _reportes = _db.ObtenerReportes();
            var nextId = _reportes.Any() ? _reportes.Max(r => r.Id) + 1 : 1;
            var nuevo = new Reporte
            {
                Codigo = $"REP-{DateTime.Now.Year}-{nextId:D3}",
                Nombre = nombre,
                Submodulo = submodulo,
                Periodo = periodo,
                FechaGeneracion = DateTime.Now,
                GeneradoPor = "Administrador",
                Estado = "Completado",
                Formato = formato,
                FilasGeneradas = new Random().Next(100, 600),
                TamañoKb = new Random().Next(200, 1200)
            };
            _db.InsertarReporte(nuevo);
            _reportes = _db.ObtenerReportes();
            TempData["MsgReporte"] = $"Reporte \"{nombre}\" generado correctamente.";
            return RedirectToAction(nameof(Reportes));
        }

        [HttpPost]
        public IActionResult EliminarReporte(int id)
        {
            _reportes = _db.ObtenerReportes();
            var r = _reportes.FirstOrDefault(x => x.Id == id);
            if (r != null)
            {
                _db.EliminarReporte(id);
                _reportes = _db.ObtenerReportes(); // recargar
                TempData["MsgReporte"] = $"Reporte {r.Codigo} eliminado.";
            }
            return RedirectToAction(nameof(Reportes));
        }

        [HttpGet]
        public IActionResult DescargarReporte(int id)
        {
            _reportes = _db.ObtenerReportes();
            var r = _reportes.FirstOrDefault(x => x.Id == id);
            if (r == null) return NotFound();

            if (r.Formato == "PDF")
            {
                var html = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='UTF-8'/><title>{r.Nombre}</title>
<style>
body{{font-family:Segoe UI,sans-serif;padding:40px;max-width:800px;margin:auto;}}
h1{{color:#4361ee;}}
table{{width:100%;border-collapse:collapse;margin-top:20px;}}
th{{background:#4361ee;color:#fff;padding:10px;text-align:left;}}
td{{padding:10px;border-bottom:1px solid #e5e7eb;}}
</style></head>
<body>
<h1>📊 {r.Nombre}</h1>
<p>Período: {r.Periodo} | Generado por: {r.GeneradoPor} | Fecha: {r.FechaGeneracion:dd/MM/yyyy HH:mm}</p>
<table>
<tr><th>Campo</th><th>Valor</th></tr>
<tr><td>Código</td><td>{r.Codigo}</td></tr>
<tr><td>Submódulo</td><td>{r.Submodulo}</td></tr>
<tr><td>Estado</td><td>{r.Estado}</td></tr>
<tr><td>Filas Generadas</td><td>{r.FilasGeneradas}</td></tr>
<tr><td>Tamaño</td><td>{r.TamañoKb} KB</td></tr>
</table>
<br/>
<button onclick='window.print()' style='background:#4361ee;color:#fff;border:none;padding:10px 20px;border-radius:8px;cursor:pointer;'>
    🖨️ Imprimir / Guardar PDF
</button>
</body></html>";
                return Content(html, "text/html", Encoding.UTF8);
            }

            var sb = new StringBuilder();
            sb.AppendLine("Código,Nombre,Submódulo,Período,Fecha Generación,Generado Por,Estado,Formato,Filas");
            sb.AppendLine($"{r.Codigo},{r.Nombre},{r.Submodulo},{r.Periodo},{r.FechaGeneracion:dd/MM/yyyy HH:mm},{r.GeneradoPor},{r.Estado},{r.Formato},{r.FilasGeneradas}");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            return r.Formato switch
            {
                "Excel" => File(bytes, "application/vnd.ms-excel", $"{r.Codigo}.xls"),
                _ => File(bytes, "text/csv", $"{r.Codigo}.csv")
            };
        }



        // ─────────────────────────────────────────────────────────────
        // EMPLEADOS
        // ─────────────────────────────────────────────────────────────
        public IActionResult Empleados(int pagina = 1, string buscar = "",
            string estado = "", string depto = "")
        {
            _empleados = _db.ObtenerEmpleados(buscar ?? "", estado ?? "", depto ?? "");
            const int pageSize = 8;
            var lista = _empleados.AsEnumerable();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(e =>
                    e.NombreCompleto.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    e.NumeroDocumento.Contains(buscar) ||
                    e.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoEmpleado>(estado, out var est))
                lista = lista.Where(e => e.Estado == est);

            if (!string.IsNullOrEmpty(depto))
                lista = lista.Where(e => e.Departamento.Equals(depto, StringComparison.OrdinalIgnoreCase));

            var total = lista.Count();
            var items = lista.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();

            var vm = new EmpleadoViewModel
            {
                Empleados = items,
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                BuscarFiltro = buscar,
                EstadoFiltro = estado,
                DeptFiltro = depto,
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearEmpleado(string ApellidoPaterno, string ApellidoMaterno, string Nombres,
            string NumeroDocumento, DateTime FechaNacimiento, string Sexo, string? Telefono, string? Email,
            string? Direccion, string Cargo, string Departamento, DateTime FechaIngreso,
            TipoContrato TipoContrato, RegimeLaboralT RegimeLaboral, EstadoEmpleado Estado,
            decimal SueldoBase, bool TieneHijos, TipoAFP SistemaPrevisional, string? CUSPP,
            MedioPago BancoPago, string? NumeroCuenta, string? TipoCuenta, string? CCI)
        {
            if (_db.ExisteNumeroDocumento(NumeroDocumento))
            {
                TempData["Error"] = $"Ya existe un empleado con el documento {NumeroDocumento}.";
                return RedirectToAction("Empleados");
            }
            decimal asigFamiliar = TieneHijos ? 102.50m : 0m;
            string tempCodigo = $"EMP-TEMP-{Guid.NewGuid():N}".Substring(0, 20);

            var emp = new Empleado
            {
                Id = 0,
                Codigo = tempCodigo,
                ApellidoPaterno = ApellidoPaterno,
                ApellidoMaterno = ApellidoMaterno,
                Nombres = Nombres,
                NumeroDocumento = NumeroDocumento,
                FechaNacimiento = FechaNacimiento,
                FechaIngreso = FechaIngreso,
                Cargo = Cargo,
                Departamento = Departamento,
                SueldoBase = SueldoBase,
                TieneHijos = TieneHijos,
                AsignacionFamiliar = asigFamiliar,
                SistemaPrevisional = SistemaPrevisional,
                CUSPP = CUSPP ?? "",
                BancoPago = BancoPago,
                NumeroCuenta = NumeroCuenta ?? "",
                CCI = CCI ?? "",
                TipoContrato = TipoContrato,
                RegimeLaboral = RegimeLaboral,
                Estado = Estado,
                CentroCostoId = 1,
            };
            if (_db.ExisteNumeroDocumento(NumeroDocumento))
            {
                TempData["Error"] = $"Ya existe un empleado con el documento {NumeroDocumento}.";
                return RedirectToAction("Empleados");
            }
            emp.Id = _db.InsertarEmpleado(emp);
            emp.Codigo = $"EMP-{emp.Id:D3}";
            // Actualizar código en BD ahora que tenemos el ID real
            _db.ActualizarEmpleado(emp);
            _empleados = _db.ObtenerEmpleados();
            TempData["Mensaje"] = $"Empleado {emp.NombreCompleto} registrado correctamente.";
            return RedirectToAction("Empleados");
        }

        [HttpGet]
        public IActionResult ObtenerEmpleado(int id)
        {
            var emp = _db.ObtenerEmpleadoPorId(id);
            if (emp == null) return NotFound();
            return Json(new
            {
                id = emp.Id,
                apellidoPaterno = emp.ApellidoPaterno,
                apellidoMaterno = emp.ApellidoMaterno,
                nombres = emp.Nombres,
                numeroDocumento = emp.NumeroDocumento,
                fechaNacimiento = emp.FechaNacimiento.ToString("yyyy-MM-dd"),
                telefono = emp.Telefono,
                email = emp.Email,
                direccion = emp.Direccion,
                cargo = emp.Cargo,
                departamento = emp.Departamento,
                fechaIngreso = emp.FechaIngreso.ToString("yyyy-MM-dd"),
                tipoContrato = (int)emp.TipoContrato,
                regimeLaboral = (int)emp.RegimeLaboral,
                estado = (int)emp.Estado,
                sueldoBase = emp.SueldoBase,
                tieneHijos = emp.TieneHijos,
                sistemaPrevisional = (int)emp.SistemaPrevisional,
                cuspp = emp.CUSPP,
                bancoPago = (int)emp.BancoPago,
                numeroCuenta = emp.NumeroCuenta,
                cci = emp.CCI,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarEmpleado(int id, string ApellidoPaterno, string ApellidoMaterno, string Nombres,
            string NumeroDocumento, DateTime FechaNacimiento, string? Telefono, string? Email,
            string? Direccion, string Cargo, string Departamento, DateTime FechaIngreso,
            TipoContrato TipoContrato, RegimeLaboralT RegimeLaboral, EstadoEmpleado Estado,
            decimal SueldoBase, bool TieneHijos, TipoAFP SistemaPrevisional, string? CUSPP,
            MedioPago BancoPago, string? NumeroCuenta, string? TipoCuenta, string? CCI)
        {
            var emp = _empleados.FirstOrDefault(e => e.Id == id) ?? _db.ObtenerEmpleadoPorId(id);
            if (emp == null) return NotFound();
            emp.ApellidoMaterno = ApellidoMaterno;
            emp.Nombres = Nombres;
            emp.NumeroDocumento = NumeroDocumento;
            emp.FechaNacimiento = FechaNacimiento;
            emp.Telefono = Telefono ?? "";
            emp.Email = Email ?? "";
            emp.Direccion = Direccion ?? "";
            emp.Cargo = Cargo;
            emp.Departamento = Departamento;
            emp.FechaIngreso = FechaIngreso;
            emp.TipoContrato = TipoContrato;
            emp.RegimeLaboral = RegimeLaboral;
            emp.Estado = Estado;
            emp.SueldoBase = SueldoBase;
            emp.TieneHijos = TieneHijos;
            emp.AsignacionFamiliar = TieneHijos ? 102.50m : 0m;
            emp.SistemaPrevisional = SistemaPrevisional;
            emp.CUSPP = CUSPP ?? "";
            emp.BancoPago = BancoPago;
            emp.NumeroCuenta = NumeroCuenta ?? "";
            emp.CCI = CCI ?? "";

            _db.ActualizarEmpleado(emp);
            _empleados = _db.ObtenerEmpleados();
            TempData["Mensaje"] = $"Empleado {emp.NombreCompleto} actualizado correctamente.";
            return RedirectToAction("Empleados");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarEmpleado(int id)
        {
            var emp = _db.ObtenerEmpleadoPorId(id);
            if (emp == null) return NotFound();
            _db.EliminarEmpleado(id);
            _empleados = _db.ObtenerEmpleados();
            TempData["Mensaje"] = $"Empleado {emp.NombreCompleto} eliminado.";
            return RedirectToAction("Empleados");
        }

        // ─────────────────────────────────────────────────────────────
        // DETALLE PLANILLA (por empleado)
        // ─────────────────────────────────────────────────────────────
        public IActionResult DetallePlanilla(string codigo, int pagina = 1, string buscar = "")
        {
            const int pageSize = 10;
            var planilla = _planillas.FirstOrDefault(p => p.Codigo == codigo);
            if (planilla == null) return NotFound();

            // Generar detalles de todos los empleados activos
            var detalles = _empleados
                .Where(e => e.Estado != EstadoEmpleado.Inactivo)
                .Select(e => CalcularDetalle(e, codigo, planilla.Periodo))
                .AsEnumerable();

            if (!string.IsNullOrEmpty(buscar))
                detalles = detalles.Where(d =>
                    d.NombreEmpleado.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    d.DNIEmpleado.Contains(buscar));

            var total = detalles.Count();
            var vm = new DetallePlanillaViewModel
            {
                Planilla = planilla,
                Detalles = detalles.Skip((pagina - 1) * pageSize).Take(pageSize).ToList(),
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                BuscarFiltro = buscar,
            };
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────
        // ESSALUD / SCTR
        // ─────────────────────────────────────────────────────────────

        // ── Datos en memoria ──────────────────────────────────────
        private static List<DeclaracionEsSalud> _declaraciones = new();

        private static List<GrupoSctr> _gruposSctr = new()
        {
            new() { Id=1, NivelRiesgo=NivelRiesgoSCTR.Riesgo1, Trabajadores=120, SctrSalud=14_200.00m, SctrPension=17_336.00m, Aseguradora="RIMAC Seguros", Activo=true },
            new() { Id=2, NivelRiesgo=NivelRiesgoSCTR.Riesgo2, Trabajadores= 80, SctrSalud=11_040.00m, SctrPension=13_248.00m, Aseguradora="RIMAC Seguros", Activo=true },
            new() { Id=3, NivelRiesgo=NivelRiesgoSCTR.Riesgo3, Trabajadores= 90, SctrSalud=16_300.00m, SctrPension=19_560.00m, Aseguradora="RIMAC Seguros", Activo=true },
            new() { Id=4, NivelRiesgo=NivelRiesgoSCTR.Riesgo4, Trabajadores= 62, SctrSalud=12_070.00m, SctrPension=14_188.00m, Aseguradora="RIMAC Seguros", Activo=true },
        };

        private static List<ValidacionEsSalud> _validaciones = new()
        {
            new() { Nombre="Declaración manual EsSalud",      Periodo="Abril 2025", Valido=true,  Severidad="Ok",         Detalle="Declaración generada y lista para enviar." },
            new() { Nombre="Remuneración asignable válida",   Periodo="Abril 2025", Valido=true,  Severidad="Ok",         Detalle="Todos los montos de remuneración son correctos." },
            new() { Nombre="Aporte EsSalud (9%) calculado",   Periodo="Abril 2025", Valido=true,  Severidad="Ok",         Detalle="El cálculo del 9% es correcto para todos los empleados." },
            new() { Nombre="Trabajadores con DNI válido",     Periodo="Abril 2025", Valido=true,  Severidad="Ok",         Detalle="Todos los documentos de identidad están validados." },
            new() { Nombre="Topes máximos EsSalud",           Periodo="Abril 2025", Valido=true,  Severidad="Advertencia",Detalle="3 trabajadores superan el tope máximo de aporte." },
            new() { Nombre="2 errores encontrados",           Periodo="Abril 2025", Valido=false, Severidad="Error",      Detalle="Empleados con inconsistencias en su contrato o datos personales." },
        };

        private static List<HistorialEnvioEsSalud> _historialEnvios = new()
        {
            new() { Id=1, FechaHora=new DateTime(2025,5,2,8,15,0),  Declaracion="ESS-2025-00004", Usuario="Admin", Estado=EstadoEnvio.Aceptado,         Mensaje="Envío exitoso" },
            new() { Id=2, FechaHora=new DateTime(2025,4,6,9,5,0),   Declaracion="ESS-2025-00003", Usuario="Admin", Estado=EstadoEnvio.Aceptado,         Mensaje="Envío exitoso" },
            new() { Id=3, FechaHora=new DateTime(2025,3,5,8,45,0),  Declaracion="ESS-2025-00022", Usuario="Admin", Estado=EstadoEnvio.Aceptado,         Mensaje="Envío exitoso" },
            new() { Id=4, FechaHora=new DateTime(2025,2,8,9,1,0),   Declaracion="ESS-2025-00013", Usuario="Admin", Estado=EstadoEnvio.ConObservaciones, Mensaje="Con observaciones" },
            new() { Id=5, FechaHora=new DateTime(2025,1,7,8,50,0),  Declaracion="ESS-3054-00012", Usuario="Admin", Estado=EstadoEnvio.Aceptado,         Mensaje="Envío exitoso" },
            new() { Id=6, FechaHora=new DateTime(2024,12,5,10,20,0),Declaracion="ESS-3054-00011", Usuario="Admin", Estado=EstadoEnvio.Aceptado,         Mensaje="Envío exitoso" },
        };

        // ── Acción principal EsSalud ───────────────────────────────
        public IActionResult EsSalud(string vista = "Resumen", string? periodo = null,
            string? estado = null, string? tipo = null, string? buscar = null, int pagina = 1)
        {
            _declaraciones = _db.ObtenerDeclaraciones(buscar ?? "", estado ?? "", tipo ?? "", periodo ?? "");
            _gruposSctr = _db.ObtenerGruposSctr();
            _historialEnvios = _db.ObtenerHistorialEnvios();
            int porPagina = 5;
            var query = _declaraciones.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(d => d.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase)
                                      || d.Periodo.Contains(buscar, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(periodo))
                query = query.Where(d => d.Periodo.Contains(periodo, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoDeclaracion>(estado, out var est))
                query = query.Where(d => d.Estado == est);
            if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoDeclaracion>(tipo, out var tip))
                query = query.Where(d => d.TipoDeclaracion == tip);

            int total = query.Count();
            int paginas = (int)Math.Ceiling(total / (double)porPagina);
            pagina = Math.Max(1, Math.Min(pagina, Math.Max(1, paginas)));

            var vm = new EsSaludViewModel
            {
                Vista = vista,
                Declaraciones = query.OrderByDescending(d => d.FechaEnvio).Skip((pagina - 1) * porPagina).Take(porPagina).ToList(),
                GruposSctr = _gruposSctr,
                Validaciones = _validaciones,
                Historial = _historialEnvios,
                TotalDeclaraciones = _declaraciones.Count,
                Pendientes = _declaraciones.Count(d => d.Estado == EstadoDeclaracion.Pendiente),
                Enviadas = _declaraciones.Count(d => d.Estado == EstadoDeclaracion.Enviada),
                Aceptadas = _declaraciones.Count(d => d.Estado == EstadoDeclaracion.Aceptada),
                Observadas = _declaraciones.Count(d => d.Estado == EstadoDeclaracion.Observada),
                AporteTotalPeriodo = _declaraciones.Where(d => d.Periodo == "Abril 2025").Sum(d => d.AporteEsSalud),
                SctrSaludTotal = _gruposSctr.Sum(g => g.SctrSalud),
                SctrPensionTotal = _gruposSctr.Sum(g => g.SctrPension),
                Empleados = _empleados,
                PeriodoFiltro = periodo ?? "",
                EstadoFiltro = estado ?? "",
                TipoFiltro = tipo ?? "",
                Buscar = buscar ?? "",
                PaginaActual = pagina,
                TotalPaginas = paginas == 0 ? 1 : paginas,
            };
            return View(vm);
        }

        [HttpPost]

        public IActionResult EnviarDeclaracion(int id)
        {
            var d = _declaraciones.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                d.Estado = EstadoDeclaracion.Enviada;
                d.FechaEnvio = DateTime.Now;
                d.NroOrdenSunat = $"2025{new Random().Next(1000000, 9999999)}";
                _db.ActualizarDeclaracion(d);
                var historial = new HistorialEnvioEsSalud
                {
                    FechaHora = DateTime.Now,
                    Declaracion = d.Codigo,
                    Usuario = "Admin",
                    Estado = EstadoEnvio.Enviado,
                    Mensaje = "Enviado a SUNAT"
                };
                _db.InsertarHistorialEnvio(historial);
                _declaraciones = _db.ObtenerDeclaraciones();
                _historialEnvios = _db.ObtenerHistorialEnvios();
                TempData["MensajeEsSalud"] = $"Declaración {d.Codigo} enviada a SUNAT.";
            }
            return RedirectToAction(nameof(EsSalud), new { vista = "Declaraciones" });
        }

        [HttpPost]
        public IActionResult AceptarDeclaracion(string codigo)
        {
            _declaraciones = _db.ObtenerDeclaraciones();
            var d = _declaraciones.FirstOrDefault(x => x.Codigo == codigo);
            if (d != null)
            {
                d.Estado = EstadoDeclaracion.Aceptada;
                if (string.IsNullOrEmpty(d.NroOrdenSunat) || d.NroOrdenSunat == "-")
                    d.NroOrdenSunat = $"2025{new Random().Next(1000000, 9999999)}";
                _db.ActualizarDeclaracion(d);
                _declaraciones = _db.ObtenerDeclaraciones();
                TempData["MensajeEsSalud"] = $"Declaración {d.Codigo} marcada como Aceptada.";
            }
            return RedirectToAction(nameof(EsSalud), new { vista = "Aportes" });
        }

        // ── Nueva Declaración EsSalud ──────────────────────────────────
        [HttpPost]
        public IActionResult NuevaDeclaracion(string periodo, int trabajadores, decimal remuneracion)
        {
            decimal aporteEsSalud = Math.Round(remuneracion * 0.09m, 2);
            int nextId = _declaraciones.Count > 0 ? _declaraciones.Max(d => d.Id) + 1 : 1;
            string codigo = $"ESS-{DateTime.Now.Year}-{nextId:D5}";

            var nueva = new DeclaracionEsSalud
            {
                Id = nextId,
                Codigo = codigo,
                Periodo = periodo,
                Trabajadores = trabajadores,
                RemuneracionAsignable = remuneracion,
                AporteEsSalud = aporteEsSalud,
                Subsidios = 0m,
                TotalPagar = aporteEsSalud,
                FechaEnvio = DateTime.Now,
                Estado = EstadoDeclaracion.Pendiente,
                TipoDeclaracion = TipoDeclaracion.Mensual,
                NroOrdenSunat = ""
            };

            _db.InsertarDeclaracion(nueva);
            _declaraciones = _db.ObtenerDeclaraciones();

            TempData["MensajeEsSalud"] = $"Declaración {codigo} creada para el período {periodo}.";
            return RedirectToAction(nameof(EsSalud), new { vista = "Declaraciones" });
        }
        // ── Validar Ahora ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult ValidarAhora()
        {
            string periodoActual = "Mayo 2025";
            decimal topeBase = 13_500m;
            var nuevas = new List<ValidacionEsSalud>();

            // 1. ¿Existe declaración del período actual?
            var declActual = _declaraciones.FirstOrDefault(d => d.Periodo == periodoActual);
            bool tieneDecl = declActual != null;
            nuevas.Add(new ValidacionEsSalud
            {
                Nombre = "Declaración del período actual",
                Periodo = periodoActual,
                Valido = tieneDecl,
                Severidad = tieneDecl ? "Ok" : "Error",
                Detalle = tieneDecl
                    ? $"Declaración {declActual!.Codigo} encontrada para {periodoActual}."
                    : $"No existe declaración registrada para {periodoActual}.",
                DetalleLargo = tieneDecl
                    ? $"Se verificó que existe una declaración registrada para el período {periodoActual}. " +
                      $"Código: {declActual!.Codigo}, Estado: {declActual.Estado}, " +
                      $"Trabajadores: {declActual.Trabajadores}, Aporte: S/ {declActual.AporteEsSalud:N2}."
                    : $"No se encontró ninguna declaración EsSalud para el período {periodoActual}. " +
                      "Debe crear una declaración antes de la fecha de vencimiento.",
                AfectadosJson = "[]"
            });

            // 2. Aporte = exactamente 9% de la remuneración asignable
            var erresAporte = new List<object>();
            foreach (var d in _declaraciones)
            {
                decimal esperado = Math.Round(d.RemuneracionAsignable * 0.09m, 2);
                if (Math.Abs(d.AporteEsSalud - esperado) > 0.01m)
                    erresAporte.Add(new { Nombre = d.Codigo, Dato = $"Aporte: S/ {d.AporteEsSalud:N2} | Esperado: S/ {esperado:N2}" });
            }
            bool aporteOk = erresAporte.Count == 0;
            nuevas.Add(new ValidacionEsSalud
            {
                Nombre = "Aporte EsSalud (9%) calculado",
                Periodo = periodoActual,
                Valido = aporteOk,
                Severidad = aporteOk ? "Ok" : "Error",
                Detalle = aporteOk
                    ? "El cálculo del 9% es correcto en todas las declaraciones."
                    : $"{erresAporte.Count} declaración(es) con aporte incorrecto.",
                DetalleLargo = aporteOk
                    ? "Se verificó que el aporte EsSalud de cada declaración equivale exactamente al 9% de la remuneración asignable registrada. No se encontraron diferencias."
                    : "Se encontraron declaraciones cuyo aporte EsSalud no coincide con el 9% de la remuneración asignable. Revise y corrija los montos indicados.",
                AfectadosJson = System.Text.Json.JsonSerializer.Serialize(erresAporte)
            });

            // 3. DNIs de empleados deben tener exactamente 8 dígitos
            var dnisInvalidos = _empleados
                .Where(e => e.NumeroDocumento?.Length != 8 || !e.NumeroDocumento.All(char.IsDigit))
                .Select(e => new { Nombre = $"{e.Nombres} {e.ApellidoPaterno}", Dato = $"DNI: {e.NumeroDocumento}" })
                .ToList<object>();
            bool dnisOk = dnisInvalidos.Count == 0;
            nuevas.Add(new ValidacionEsSalud
            {
                Nombre = "Trabajadores con DNI válido",
                Periodo = periodoActual,
                Valido = dnisOk,
                Severidad = dnisOk ? "Ok" : "Error",
                Detalle = dnisOk
                    ? "Todos los DNIs tienen 8 dígitos y son numéricos."
                    : $"{dnisInvalidos.Count} empleado(s) con DNI inválido.",
                DetalleLargo = dnisOk
                    ? "Se verificaron los números de documento de todos los empleados registrados. Todos cumplen con el formato requerido: 8 dígitos numéricos."
                    : "Los siguientes empleados tienen un número de documento que no cumple el formato DNI (8 dígitos numéricos). Actualice sus datos antes de enviar la declaración.",
                AfectadosJson = System.Text.Json.JsonSerializer.Serialize(dnisInvalidos)
            });

            // 4. Ningún trabajador supera el tope de S/ 13,500 en sueldo base
            var sobretope = _empleados
                .Where(e => e.SueldoBase > topeBase)
                .Select(e => new { Nombre = $"{e.Nombres} {e.ApellidoPaterno}", Dato = $"Base: S/ {e.SueldoBase:N2}" })
                .ToList<object>();
            bool topeOk = sobretope.Count == 0;
            nuevas.Add(new ValidacionEsSalud
            {
                Nombre = "Topes máximos EsSalud",
                Periodo = periodoActual,
                Valido = topeOk,
                Severidad = topeOk ? "Ok" : "Advertencia",
                Detalle = topeOk
                    ? "Ningún trabajador supera el tope máximo de S/ 13,500."
                    : $"{sobretope.Count} trabajador(es) superan el tope de S/ 13,500.",
                DetalleLargo = topeOk
                    ? $"Se verificó que ningún trabajador supera el tope máximo de remuneración asignable de S/ {topeBase:N2} establecido por EsSalud. El aporte se aplica correctamente sobre la totalidad del sueldo."
                    : $"Los siguientes trabajadores tienen un sueldo base superior a S/ {topeBase:N2}. EsSalud limita el aporte al tope; verifique que el cálculo refleje correctamente ese límite.",
                AfectadosJson = System.Text.Json.JsonSerializer.Serialize(sobretope)
            });

            _validaciones = nuevas;
            int errores = nuevas.Count(v => v.Severidad == "Error");
            int advertencias = nuevas.Count(v => v.Severidad == "Advertencia");
            TempData["MensajeEsSalud"] = errores == 0 && advertencias == 0
                ? "✅ Validación completada: todas las verificaciones pasaron correctamente."
                : $"⚠️ Validación completada: {errores} error(es), {advertencias} advertencia(s) encontradas.";
            return RedirectToAction(nameof(EsSalud), new { vista = "Validaciones" });
        }

        public IActionResult ExportarAportesExcel()
        {
            _declaraciones = _db.ObtenerDeclaraciones();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Concepto,Periodo,Trabajadores,Remuneracion Asignable,Aporte EsSalud (9%),Fecha Envio,Estado");
            foreach (var d in _declaraciones.OrderByDescending(x => x.FechaEnvio))
                csv.AppendLine($"Aporte EsSalud,{d.Periodo},{d.Trabajadores},S/ {d.RemuneracionAsignable:N2},S/ {d.AporteEsSalud:N2},{d.FechaEnvio:dd/MM/yyyy},{d.Estado}");
            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                        .Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString()))
                        .ToArray();
            return File(bytes, "text/csv", "AportesEsSalud.csv");
        }

        // ── Configurar grupos SCTR ────────────────────────────────────
        [HttpPost]
        public IActionResult ConfigurarSctr(string aseguradora, string nroPoliza,
    int trab_1, decimal salud_1, decimal pension_1,
    int trab_2, decimal salud_2, decimal pension_2,
    int trab_3, decimal salud_3, decimal pension_3,
    int trab_4, decimal salud_4, decimal pension_4)
        {
            var vals = new Dictionary<int, (int Trab, decimal Salud, decimal Pension)>
            {
                [1] = (trab_1, salud_1, pension_1),
                [2] = (trab_2, salud_2, pension_2),
                [3] = (trab_3, salud_3, pension_3),
                [4] = (trab_4, salud_4, pension_4),
            };

            _gruposSctr = _db.ObtenerGruposSctr(); // recargar desde BD primero

            foreach (var g in _gruposSctr)
            {
                if (vals.TryGetValue(g.Id, out var v))
                {
                    g.Trabajadores = v.Trab;
                    g.SctrSalud = v.Salud;
                    g.SctrPension = v.Pension;
                    g.Aseguradora = aseguradora;
                    _db.ActualizarGrupoSctr(g); // ← guardar en BD
                }
            }

            _gruposSctr = _db.ObtenerGruposSctr(); // recargar tras actualizar
            TempData["MensajeEsSalud"] = $"Configuración SCTR actualizada · Aseguradora: {aseguradora} · Póliza: {nroPoliza}";
            return RedirectToAction(nameof(EsSalud), new { vista = "Sctr" });
        }

        // ── Descargar declaración EsSalud como CSV ────────────────────
        public IActionResult DescargarDeclaracion(int id)
        {
            var d = _declaraciones.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Campo,Valor");
            csv.AppendLine($"Código,{d.Codigo}");
            csv.AppendLine($"Período,{d.Periodo}");
            csv.AppendLine($"Trabajadores,{d.Trabajadores}");
            csv.AppendLine($"Remuneración Asignable,{d.RemuneracionAsignable:N2}");
            csv.AppendLine($"Aporte EsSalud (9%),{d.AporteEsSalud:N2}");
            csv.AppendLine($"Subsidios,{d.Subsidios:N2}");
            csv.AppendLine($"Total a Pagar,{d.TotalPagar:N2}");
            csv.AppendLine($"Estado,{d.Estado}");
            csv.AppendLine($"Tipo Declaración,{d.TipoDeclaracion}");
            csv.AppendLine($"Fecha Envío,{d.FechaEnvio:dd/MM/yyyy HH:mm}");
            csv.AppendLine($"N° Orden SUNAT,{(string.IsNullOrEmpty(d.NroOrdenSunat) ? "-" : d.NroOrdenSunat)}");
            csv.AppendLine($"Observación,{d.Observacion ?? ""}");

            var bytes = System.Text.Encoding.UTF8.GetPreamble()   // BOM para Excel
                        .Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString()))
                        .ToArray();

            return File(bytes, "text/csv", $"DeclaracionEsSalud_{d.Codigo}.csv");
        }

        // ─────────────────────────────────────────────────────────────
        // BOLETA DE PAGO individual
        // ─────────────────────────────────────────────────────────────
        public IActionResult Boleta(string codigo, int empleadoId)
        {
            var planilla = _planillas.FirstOrDefault(p => p.Codigo == codigo);
            var empleado = _empleados.FirstOrDefault(e => e.Id == empleadoId);
            if (planilla == null || empleado == null) return NotFound();

            var vm = new BoletaPagoViewModel
            {
                Empleado = empleado,
                Detalle = CalcularDetalle(empleado, codigo, planilla.Periodo),
                Periodo = planilla.Periodo,
            };
            return View(vm);
        }
    }
}