namespace SGE.Models;

public class ImpuestoFinanciero
{
    public int ImpuestoId { get; set; }
    public string CodigoImpuestoSunat { get; set; } = "";
    public string NombreImpuesto { get; set; } = "";
    public decimal Porcentaje { get; set; }
    public bool Estado { get; set; } = true;
}
