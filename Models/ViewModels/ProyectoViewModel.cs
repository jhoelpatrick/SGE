namespace SyS_ERP.Models.ViewModels
{
    public enum EstadoTarea { PorHacer, EnProceso, Finalizado }

    public class TareaKanban
    {
        public int         Id           { get; set; }
        public string      Titulo       { get; set; } = string.Empty;
        public string      Descripcion  { get; set; } = string.Empty;
        public string      Responsable  { get; set; } = string.Empty;
        public string      Prioridad    { get; set; } = "Media"; // Alta | Media | Baja
        public EstadoTarea Estado       { get; set; } = EstadoTarea.PorHacer;
        public string      FechaVence   { get; set; } = string.Empty;
        public int?        PredecesoraId{ get; set; }
    }

    public class Proyecto
    {
        public int           Id           { get; set; }
        public string        Nombre       { get; set; } = string.Empty;
        public string        Descripcion  { get; set; } = string.Empty;
        public int           Avance       { get; set; } // 0-100%
        public string        FechaInicio  { get; set; } = string.Empty;
        public string        FechaFin     { get; set; } = string.Empty;
        public string        Estado       { get; set; } = "Activo";
        public decimal       Presupuesto  { get; set; } = 50000m;
        public decimal       CostoReal    { get; set; } = 35000m;
        public List<TareaKanban> Tareas   { get; set; } = new();
    }

    public class ProyectosViewModel
    {
        public List<Proyecto> Proyectos         { get; set; } = new();
        public List<TareaKanban> TodasLasTareas { get; set; } = new();
        public int ProyectoActualId             { get; set; } = 1;
    }
}
