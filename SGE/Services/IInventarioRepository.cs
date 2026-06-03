using SGE.Models;

namespace SGE.Services
{
    public interface IInventarioRepository
    {
        Task<List<Producto>> GetStockSummaryAsync();
        Task<List<KardexMovimiento>> GetKardexByProductoIdAsync(int productoId);
        Task RegistrarMovimientoManualAsync(int productoId, string tipoMovimiento, decimal cantidad, string motivo);
    }
}
