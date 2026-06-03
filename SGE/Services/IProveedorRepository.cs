using SGE.Models;

namespace SGE.Services
{
    /// <summary>Contrato para el acceso a datos de la tabla comercial.proveedores.</summary>
    public interface IProveedorRepository
    {
        Task<List<Proveedor>> GetAllAsync();
        Task<Proveedor?> GetByIdAsync(int id);
        Task<int> CreateAsync(Proveedor proveedor);
        Task UpdateAsync(Proveedor proveedor);
        Task DeleteAsync(int id);
        Task ToggleEstadoAsync(int id, bool estado);
    }
}
