using SGE.Models;

namespace SGE.Services
{
    /// <summary>Contrato para el acceso a datos de la tabla comercial.productos.</summary>
    public interface IProductoRepository
    {
        Task<List<Producto>> GetAllAsync();
        Task<Producto?> GetByIdAsync(int id);
        Task<int> CreateAsync(Producto producto);
        Task UpdateAsync(Producto producto);
        Task DeleteAsync(int id);
        Task ToggleEstadoAsync(int id, bool estado);
    }
}
