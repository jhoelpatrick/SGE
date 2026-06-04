﻿using SGE.Models;

namespace SGE.Services
{
    public interface IProyectoRepository
    {
        Task<List<Proyecto>> GetAllAsync();
        Task<Proyecto?> GetByIdAsync(int id);
        Task<int> CreateAsync(Proyecto p);
        Task<List<ProyectoTarea>> GetTareasByProyectoIdAsync(int proyectoId);
        Task<int> CreateTareaAsync(ProyectoTarea t);
        Task UpdateTareaEstadoAsync(int tareaId, decimal progreso, string estado);
    }
}
