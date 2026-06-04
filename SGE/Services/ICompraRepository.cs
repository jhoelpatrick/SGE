﻿using SGE.Models;

namespace SGE.Services
{
    public interface ICompraRepository
    {
        Task<List<OrdenCompra>> GetAllAsync();
        Task<OrdenCompra?> GetByIdAsync(int id);
        Task<int> CreateAsync(OrdenCompra o);
        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        Task<List<OrdenCompraDetalle>> GetDetalleByOrdenIdAsync(int ordenId);
    }
}
