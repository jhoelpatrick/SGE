using SGE.Models;

namespace SGE.ViewModels
{
    public class UsuarioViewModel
    {
        public Usuario Usuario { get; set; } = new();

        public List<string> Roles { get; set; } = new();

        public List<Permiso> Permisos { get; set; } = new();
    }
}