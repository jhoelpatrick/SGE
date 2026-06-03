using System.ComponentModel.DataAnnotations;

namespace SGE.Models
{
    public class ProveedorViewModel
    {
        public int Id { get; set; }

        [Required]
        public string TipoDocumentoId { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroDocumento { get; set; }

        [Required]
        [StringLength(250)]
        public string RazonSocial { get; set; }

        [Required]
        [StringLength(500)]
        public string DireccionFiscal { get; set; }

        [Required]
        public string UbigeoId { get; set; }

        public bool EsEliminado { get; set; }
    }
}