namespace SyS_ERP.Models.ViewModels
{
    public class Pedido
    {
        public int     Id            { get; set; }
        public string  NroPedido     { get; set; } = string.Empty;
        public string  Cliente       { get; set; } = string.Empty;
        public string  Fecha         { get; set; } = string.Empty;
        public decimal Total         { get; set; }
        public string  Estado        { get; set; } = "Pendiente"; // Pendiente | Aprobado | Despachado | Cancelado
        public string  Moneda        { get; set; } = "PEN"; // PEN | USD | EUR
        public decimal Descuento     { get; set; } = 0.00m;
        public string  TransaccionId { get; set; } = string.Empty;
        public string  MetodoPago    { get; set; } = string.Empty; // Visa, Mastercard, Paypal, Crédito
    }

    public class VentasViewModel
    {
        public List<Pedido> Pedidos            { get; set; } = new();
        public decimal      TotalMes           { get; set; }
        public int          PedidosPendientes  { get; set; }
        public int          PedidosAprobados   { get; set; }
    }
}
