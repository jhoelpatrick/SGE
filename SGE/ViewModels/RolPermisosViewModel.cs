using SGE.Models;

namespace SGE.ViewModels
{
    public class RolPermisosViewModel
    {
        public string RolSeleccionado { get; set; } = "";

        public List<string> Roles { get; set; } = new();

        public List<Permiso> Permisos { get; set; } = new();
    }
}