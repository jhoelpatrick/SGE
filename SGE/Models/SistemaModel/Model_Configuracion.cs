namespace SGE.Models.SistemaModel
{
    public class Model_Configuracion
    {
        public class EmpresaModel
        {
            public string RazonSocial { get; set; }
            public string Ruc { get; set; }
            public string DireccionFiscal { get; set; }
            public string Telefono { get; set; }
            public string CorreoCorporativo { get; set; }
            public string SitioWeb { get; set; }
            public string Logo { get; set; }
        }

        public class RegionalModel
        {
            public string Pais { get; set; }
            public string Idioma { get; set; }
            public string ZonaHoraria { get; set; }
        }

        public class CorreoModel
        {
            public string ServidorSmtp { get; set; }
            public int PuertoSmtp { get; set; }
            public string CorreoSistema { get; set; }
            public string Contrasena { get; set; }
        }

        public class SeguridadModel
        {
            public int TiempoSesionMinutos { get; set; }
            public int IntentosMaximosLogin { get; set; }
            public bool ActivarMfa { get; set; }
            public bool BloquearIpSospechosas { get; set; }
        }

        // 2. El contenedor maestro que se envía al Front-End
        public class SistemaConfiguracionDTO
        {
            public EmpresaModel Empresa { get; set; }
            public RegionalModel Regional { get; set; }
            public CorreoModel Correo { get; set; }
            public SeguridadModel Seguridad { get; set; }
        }

    }
}
