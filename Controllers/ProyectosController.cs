using Microsoft.AspNetCore.Mvc;
using SyS_ERP.Models.ViewModels;

namespace SyS_ERP.Controllers
{
    /// <summary>
    /// Módulo de Proyectos — Tablero Kanban interactivo, persistencia estática,
    /// validación de dependencia de tareas y análisis gráfico de desviación presupuestal.
    /// </summary>
    public class ProyectosController : Controller
    {
        private readonly ILogger<ProyectosController> _logger;

        private static readonly List<Proyecto> _proyectos = new()
        {
            new()
            {
                Id=1, Nombre="Migración ERP v2.0", Descripcion="Migración completa del sistema legado", Avance=66,
                FechaInicio="2024-03-01", FechaFin="2024-07-31", Estado="Activo",
                Presupuesto=75000.00m, CostoReal=48200.00m,
                Tareas = new()
                {
                    new() { Id=1, Titulo="Análisis de requerimientos",    Descripcion="Levantar requerimientos del cliente",     Responsable="Carlos R.",  Prioridad="Alta",  Estado=EstadoTarea.Finalizado, FechaVence="2024-03-15", PredecesoraId=null },
                    new() { Id=2, Titulo="Diseño de base de datos",       Descripcion="Modelado entidad-relación del nuevo ERP", Responsable="Ana T.",     Prioridad="Alta",  Estado=EstadoTarea.Finalizado, FechaVence="2024-04-01", PredecesoraId=1 },
                    new() { Id=3, Titulo="Desarrollo módulo Ventas",      Descripcion="CRUD completo y gráficos de tendencias",  Responsable="Luis M.",    Prioridad="Alta",  Estado=EstadoTarea.EnProceso,  FechaVence="2024-05-30", PredecesoraId=2 }, // Depende de DB
                    new() { Id=4, Titulo="Desarrollo módulo Inventario",  Descripcion="Control de stock y alertas",              Responsable="Pedro V.",   Prioridad="Media", Estado=EstadoTarea.EnProceso,  FechaVence="2024-06-15", PredecesoraId=2 }, // Depende de DB
                    new() { Id=5, Titulo="Pruebas de integración",        Descripcion="Testing de todos los módulos",            Responsable="Sandra L.",  Prioridad="Alta",  Estado=EstadoTarea.PorHacer,   FechaVence="2024-07-01", PredecesoraId=3 }, // Depende de Ventas
                    new() { Id=6, Titulo="Capacitación usuarios",         Descripcion="Entrenamiento al personal de la empresa", Responsable="María C.",   Prioridad="Media", Estado=EstadoTarea.PorHacer,   FechaVence="2024-07-20", PredecesoraId=5 }, // Depende de Pruebas
                }
            },
            new()
            {
                Id=2, Nombre="App Móvil Clientes", Descripcion="Aplicación móvil para consultas y pedidos", Avance=25,
                FechaInicio="2024-04-01", FechaFin="2024-09-30", Estado="Activo",
                Presupuesto=45000.00m, CostoReal=12000.00m,
                Tareas = new()
                {
                    new() { Id=7,  Titulo="Wireframes y diseño UI",       Descripcion="Prototipos en Figma",                     Responsable="Ana T.",     Prioridad="Alta",  Estado=EstadoTarea.Finalizado, FechaVence="2024-04-20", PredecesoraId=null },
                    new() { Id=8,  Titulo="API REST Backend",             Descripcion="Endpoints para la app móvil",             Responsable="Luis M.",    Prioridad="Alta",  Estado=EstadoTarea.EnProceso,  FechaVence="2024-06-30", PredecesoraId=7 },
                    new() { Id=9,  Titulo="Desarrollo Flutter",           Descripcion="App iOS y Android en Flutter",            Responsable="Carlos R.",  Prioridad="Alta",  Estado=EstadoTarea.PorHacer,   FechaVence="2024-08-31", PredecesoraId=8 },
                    new() { Id=10, Titulo="QA y pruebas en dispositivos", Descripcion="Testing en emuladores y físicos",         Responsable="Pedro V.",   Prioridad="Media", Estado=EstadoTarea.PorHacer,   FechaVence="2024-09-15", PredecesoraId=9 },
                }
            },
            new()
            {
                Id=3, Nombre="Infraestructura Cloud", Descripcion="Migración a Azure Cloud Services", Avance=75,
                FechaInicio="2024-01-15", FechaFin="2024-06-30", Estado="Activo",
                Presupuesto=60000.00m, CostoReal=58000.00m,
                Tareas = new()
                {
                    new() { Id=11, Titulo="Configuración Azure DevOps",   Descripcion="Pipelines CI/CD configurados",            Responsable="Sandra L.",  Prioridad="Alta",  Estado=EstadoTarea.Finalizado, FechaVence="2024-02-01", PredecesoraId=null },
                    new() { Id=12, Titulo="Migración Base de Datos",      Descripcion="Migración SQL Server a Azure SQL",        Responsable="Pedro V.",   Prioridad="Alta",  Estado=EstadoTarea.Finalizado, FechaVence="2024-03-15", PredecesoraId=11 },
                    new() { Id=13, Titulo="Configuración VMs y Load Bal.",Descripcion="Balanceo de carga y VMs en Azure",        Responsable="Luis M.",    Prioridad="Media", Estado=EstadoTarea.EnProceso,  FechaVence="2024-05-31", PredecesoraId=12 },
                    new() { Id=14, Titulo="Monitoreo y alertas",          Descripcion="Azure Monitor, Application Insights",     Responsable="Carlos R.",  Prioridad="Media", Estado=EstadoTarea.PorHacer,   FechaVence="2024-06-20", PredecesoraId=13 },
                }
            }
        };

        public ProyectosController(ILogger<ProyectosController> logger)
            => _logger = logger;

        // ── GET /Proyectos ────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index(int proyectoId = 1)
        {
            var proyectos = _proyectos;
            var actual    = proyectos.FirstOrDefault(p => p.Id == proyectoId) ?? proyectos.First();
            var todasTareas = proyectos.SelectMany(p => p.Tareas).ToList();

            var vm = new ProyectosViewModel
            {
                Proyectos        = proyectos,
                TodasLasTareas   = todasTareas,
                ProyectoActualId = actual.Id
            };
            return View(vm);
        }

        // ── POST: Mover Tarea en Kanban (con verificación de dependencia) ────
        [HttpPost]
        public IActionResult MoverTarea(int tareaId, int proyectoId, string nuevoEstado)
        {
            var proyecto = _proyectos.FirstOrDefault(p => p.Id == proyectoId);
            if (proyecto == null) return Json(new { success = false, message = "Proyecto no encontrado." });

            var tarea = proyecto.Tareas.FirstOrDefault(t => t.Id == tareaId);
            if (tarea == null) return Json(new { success = false, message = "Tarea no encontrada." });

            if (!Enum.TryParse<EstadoTarea>(nuevoEstado, out var targetEstado))
            {
                return Json(new { success = false, message = "Estado inválido." });
            }

            // Si el estado al que se mueve es "EnProceso" o "Finalizado", verificar Predecesora
            if (targetEstado == EstadoTarea.EnProceso || targetEstado == EstadoTarea.Finalizado)
            {
                if (tarea.PredecesoraId.HasValue)
                {
                    var pred = proyecto.Tareas.FirstOrDefault(t => t.Id == tarea.PredecesoraId.Value);
                    if (pred != null && pred.Estado != EstadoTarea.Finalizado)
                    {
                        return Json(new { 
                            success = false, 
                            depError = true,
                            message = $"Bloqueado por dependencia: La tarea predecesora '{pred.Titulo}' debe estar en estado 'Finalizado' para poder trabajar en '{tarea.Titulo}'." 
                        });
                    }
                }
            }

            // Cambiar estado
            tarea.Estado = targetEstado;

            // Recalcular Avance del Proyecto
            if (proyecto.Tareas.Any())
            {
                int total = proyecto.Tareas.Count;
                int fin   = proyecto.Tareas.Count(t => t.Estado == EstadoTarea.Finalizado);
                proyecto.Avance = (int)Math.Round((double)fin / total * 100);
            }

            return Json(new { success = true, message = "Tarea movida con éxito.", avance = proyecto.Avance });
        }

        // ── POST: Agregar Tarea ───────────────────────────────────────────────
        [HttpPost]
        public IActionResult AgregarTarea(int proyectoId, string titulo, string descripcion, string responsable, string prioridad, int? predecesoraId, string fechaVence)
        {
            var proyecto = _proyectos.FirstOrDefault(p => p.Id == proyectoId);
            if (proyecto == null) return Json(new { success = false, message = "Proyecto no encontrado." });

            int nextId = _proyectos.SelectMany(p => p.Tareas).Any()
                ? _proyectos.SelectMany(p => p.Tareas).Max(t => t.Id) + 1
                : 1;

            var nueva = new TareaKanban
            {
                Id = nextId,
                Titulo = titulo,
                Descripcion = descripcion,
                Responsable = responsable,
                Prioridad = prioridad,
                Estado = EstadoTarea.PorHacer,
                FechaVence = string.IsNullOrEmpty(fechaVence) ? DateTime.Now.AddDays(7).ToString("yyyy-MM-dd") : fechaVence,
                PredecesoraId = predecesoraId == 0 ? null : predecesoraId
            };

            proyecto.Tareas.Add(nueva);

            // Recalcular Avance
            int total = proyecto.Tareas.Count;
            int fin   = proyecto.Tareas.Count(t => t.Estado == EstadoTarea.Finalizado);
            proyecto.Avance = (int)Math.Round((double)fin / total * 100);

            // Simular aumento de costo real por registrar tarea (gestión costos)
            proyecto.CostoReal += 1500.00m;

            return Json(new { success = true, message = "Tarea agregada correctamente al Kanban." });
        }

        // ── JsonResult: Avance general por proyecto (Anillo) ──────────────────
        [HttpGet]
        public IActionResult GetAvanceProyectos()
        {
            return Json(new
            {
                labels = _proyectos.Select(p => p.Nombre).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label           = "Avance (%)",
                        data            = _proyectos.Select(p => p.Avance).ToArray(),
                        backgroundColor = new[] { "#7c3aed","#06b6d4","#10b981","#f59e0b","#e85d9c" },
                        borderWidth     = 2
                    }
                }
            });
        }

        // ── JsonResult: Carga de trabajo por responsable ─────────────
        [HttpGet]
        public IActionResult GetCargaResponsable()
        {
            var tareas = _proyectos.SelectMany(p => p.Tareas).ToList();
            var agrupado = tareas.GroupBy(t => t.Responsable)
                                 .Select(g => new
                                 {
                                     Responsable = g.Key,
                                     PorHacer    = g.Count(t => t.Estado == EstadoTarea.PorHacer),
                                     EnProceso   = g.Count(t => t.Estado == EstadoTarea.EnProceso),
                                     Finalizado  = g.Count(t => t.Estado == EstadoTarea.Finalizado)
                                 }).ToList();

            return Json(new
            {
                labels = agrupado.Select(a => a.Responsable).ToArray(),
                datasets = new[]
                {
                    new { label="Por Hacer",  data=agrupado.Select(a=>a.PorHacer).ToArray(),   backgroundColor="#f59e0b" },
                    new { label="En Proceso", data=agrupado.Select(a=>a.EnProceso).ToArray(),  backgroundColor="#06b6d4" },
                    new { label="Finalizado", data=agrupado.Select(a=>a.Finalizado).ToArray(), backgroundColor="#10b981" }
                }
            });
        }

        // ── JsonResult: Desviación Presupuestal por Proyecto ─────────────
        [HttpGet]
        public IActionResult GetDesviacionPresupuesto(int proyectoId)
        {
            var p = _proyectos.FirstOrDefault(x => x.Id == proyectoId);
            if (p == null) return NotFound();

            return Json(new
            {
                nombre = p.Nombre,
                presupuesto = p.Presupuesto,
                costoReal = p.CostoReal,
                desviacion = p.Presupuesto - p.CostoReal,
                porcentajeConsumido = Math.Round((p.CostoReal / p.Presupuesto) * 100, 1)
            });
        }
    }
}
