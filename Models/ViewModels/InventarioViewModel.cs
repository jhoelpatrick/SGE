namespace SyS_ERP.Models.ViewModels
{
    public enum EstadoStock { Normal, Critico, SinStock }

    public class ProductoStock
    {
        public int         Id         { get; set; }
        public string      Codigo     { get; set; } = string.Empty;
        public string      Nombre     { get; set; } = string.Empty;
        public string      Categoria  { get; set; } = string.Empty;
        public int         StockActual{ get; set; }
        public int         StockMinimo{ get; set; }
        public string      Unidad     { get; set; } = "UND";
        public string      Lote       { get; set; } = "LT-2024-001";
        public string      FechaVencimiento { get; set; } = "2026-12-31";
        public string      Almacen    { get; set; } = "Lima Central"; // Lima Central | Arequipa Sur | Trujillo Norte
        public string      NroSerie   { get; set; } = "SN-9988-1122";
        public EstadoStock Estado     => StockActual == 0
                                          ? EstadoStock.SinStock
                                          : StockActual <= StockMinimo
                                              ? EstadoStock.Critico
                                              : EstadoStock.Normal;
    }

    public class MovimientoAlmacen
    {
        public int    Id       { get; set; }
        public string Fecha    { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string Tipo     { get; set; } = "Entrada"; // Entrada | Salida
        public int    Cantidad { get; set; }
        public string Usuario  { get; set; } = string.Empty;
    }

    public class InventarioViewModel
    {
        public List<ProductoStock>    Productos    { get; set; } = new();
        public List<MovimientoAlmacen> Movimientos { get; set; } = new();
        public int AlertasSinStock { get; set; }
        public int AlertasCriticas { get; set; }
    }
}
