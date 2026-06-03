using SGE.Models;

namespace SGE.Services
{
    /// <summary>Contrato para el acceso a datos de la tabla comercial.clientes.</summary>
    public interface IClienteRepository
    {
        /// <summary>Devuelve todos los clientes usando la vista optimizada vw_crm_clientes_bandeja.</summary>
        Task<List<Cliente>> GetAllAsync();

        /// <summary>Devuelve un cliente por su ID, o null si no existe.</summary>
        Task<Cliente?> GetByIdAsync(int id);

        /// <summary>Inserta un nuevo cliente y devuelve el ID generado por SCOPE_IDENTITY().</summary>
        Task<int> CreateAsync(Cliente cliente);

        /// <summary>Actualiza los datos de un cliente existente.</summary>
        Task UpdateAsync(Cliente cliente);

        /// <summary>Elimina (o desactiva) un cliente por su ID.</summary>
        Task DeleteAsync(int id);

        /// <summary>Activa o desactiva el estado de un cliente (toggle).</summary>
        Task ToggleEstadoAsync(int id, bool estado);
    }
}
