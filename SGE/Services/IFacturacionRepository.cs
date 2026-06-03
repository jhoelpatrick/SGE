using SGE.Models;

namespace SGE.Services
{
    public interface IFacturacionRepository
    {
        Task<List<ComprobanteFacturacion>> GetAllInvoicesAsync();
        Task<List<GuiaRemision>> GetAllGuidesAsync();
        Task<int> EmitirFacturaDesdePedidoAsync(int pedidoId, string tipoComprobante, string serie);
        Task<List<PedidoVenta>> GetPendingBillingOrdersAsync();
    }
}
