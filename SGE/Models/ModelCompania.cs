using System;

namespace SGE.Models
{
    public class ModelCompania
    {
        public int id_comp { get; set; }

        public string razon_social { get; set; }
        public string RUC { get; set; }
        public string Direc_Fiscal { get; set; }
        public string Telef { get; set; }
        public string Celular { get; set; }
        public string Correo { get; set; }
        public string Sitio_web { get; set; }
        public string logo { get; set; }

        public int? id_pais { get; set; }
        public int? id_idioma { get; set; }
        public string zona_horaria { get; set; }

        public DateTime? fec_crea { get; set; }
        public DateTime? fec_act { get; set; }

        public string estado { get; set; }

        public string Pais { get; set; }
        public string Idioma { get; set; }
    }
}