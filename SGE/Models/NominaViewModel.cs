using System;
using System.Collections.Generic;

namespace SGE.Models
{
    public class NominaViewModel
    {
        public decimal TotalPlanillaMesActual { get; set; }
        public decimal TotalPlanillaMesAnterior { get; set; }
        public int EmpleadosEnPlanilla { get; set; }
        public int EmpleadosNuevosMes { get; set; }
        public DateTime ProximoPago { get; set; }
        public decimal DescuentosTotales { get; set; }

        public int PlanillasPagadas { get; set; }
        public int PlanillasEnProceso { get; set; }
        public int PlanillasPendientes { get; set; }
        public int PlanillasAnuladas { get; set; }

        public List<Planilla> UltimasPlanillas { get; set; } = new();

        public decimal PorcentajeCambio =>
            TotalPlanillaMesAnterior == 0 ? 0 :
            Math.Round(((TotalPlanillaMesActual - TotalPlanillaMesAnterior) / TotalPlanillaMesAnterior) * 100, 1);

        public decimal PorcentajeDescuentos =>
            TotalPlanillaMesActual == 0 ? 0 :
            Math.Round((DescuentosTotales / TotalPlanillaMesActual) * 100, 1);

        public int TotalPlanillas =>
            PlanillasPagadas + PlanillasEnProceso + PlanillasPendientes + PlanillasAnuladas;

        public int DiasParaProximoPago =>
            Math.Max(0, (ProximoPago - DateTime.Today).Days);

        // ── Fuerza Laboral (dashboard) ──
        public int TotalEmpleados { get; set; }
        public int EmpleadosActivos { get; set; }
        public int EmpleadosEnVacaciones { get; set; }
        public decimal MasaSalarial { get; set; }
        public List<Empleado> EmpleadosPreview { get; set; } = new();
    }
    /// <summary>
    /// Resultado agregado que devuelve SgeDb.ObtenerResumenDashboard().
    /// Evita múltiples llamadas a la BD en el action Index.
    /// </summary>
    public class DashboardResumen
    {
        public decimal TotalNetoMesActual { get; set; }
        public decimal TotalBrutoMesActual { get; set; }
        public decimal DescuentosMesActual { get; set; }
        public decimal TotalNetoMesAnterior { get; set; }
        public int PlanillasPagadas { get; set; }
        public int PlanillasEnProceso { get; set; }
        public int PlanillasPendientes { get; set; }
        public int PlanillasAnuladas { get; set; }
        public int EmpleadosActivos { get; set; }
        public int EmpleadosNuevosMes { get; set; }
        public int EmpleadosEnVacaciones { get; set; }
        public decimal MasaSalarial { get; set; }
        public DateTime? ProximoPago { get; set; }
        public List<Planilla> UltimasPlanillas { get; set; } = new();
    }
}