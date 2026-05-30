using Microsoft.AspNetCore.Mvc;

namespace SGE.Controllers
{
    /// <summary>
    /// Redirige las rutas /Gestion/* del Dashboard al módulo real GestionUsuarios.
    /// El Dashboard usa AJAX con data-url="/Gestion/Usuario" y "/Gestion/Roles",
    /// por lo que este controlador actúa como alias/proxy de rutas.
    /// </summary>
    public class GestionController : Controller
    {
        // /Gestion/Usuario  →  GestionUsuarios/Index (lista de usuarios)
        public IActionResult Usuario()
            => RedirectToAction("Index", "GestionUsuarios");

        // /Gestion/Roles  →  GestionUsuarios/RolesPermisos
        public IActionResult Roles()
            => RedirectToAction("RolesPermisos", "GestionUsuarios");
    }
}
