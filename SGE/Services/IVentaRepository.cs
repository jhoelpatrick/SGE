﻿using SGE.Models;

namespace SGE.Services
{
    public interface IVentaRepository
    {
        Task<List<PedidoVenta>> GetAllAsync();
        Task<PedidoVenta?> GetByIdAsync(int id);
        Task<int> CreateAsync(PedidoVenta p);
        Task ApproveAsync(int id);
        Task CancelAsync(int id);
        Task DispatchAsync(int pedId, int vehId, int condId, string serie, string corr);
        Task<List<PedidoVentaDetalle>> GetDetalleByPedidoIdAsync(int pedidoId);
    }
}
