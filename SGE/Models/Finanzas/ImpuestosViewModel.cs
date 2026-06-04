namespace SGE.Models;

public class ImpuestosViewModel
{
    public List<FinanzasKpi> Kpis { get; set; } = new();
    public List<ImpuestoFinanciero> Impuestos { get; set; } = new();
    public decimal DebitoFiscal { get; set; }
    public decimal CreditoFiscal { get; set; }
    public decimal Retenciones { get; set; }
    public decimal Percepciones { get; set; }
    public decimal IgvNeto => DebitoFiscal - CreditoFiscal;
    public decimal CreditosDeducibles => Retenciones + Percepciones;
    public decimal MontoEstimadoSunat => IgvNeto - CreditosDeducibles;
}
