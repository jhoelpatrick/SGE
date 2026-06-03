using Microsoft.AspNetCore.Mvc;

namespace SGE.Extensions;

public static class ControllerExtensions
{
    public static bool EsSolicitudAjax(this Controller controller) =>
        string.Equals(controller.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    public static IActionResult RespuestaGuardado(this Controller controller, bool exito, string mensaje, string? redirectUrl = null)
    {
        if (!controller.EsSolicitudAjax())
        {
            if (exito)
            {
                controller.TempData["Success"] = mensaje;
                return redirectUrl is not null
                    ? controller.Redirect(redirectUrl)
                    : controller.RedirectToAction("Index");
            }

            controller.TempData["Error"] = mensaje;
            return controller.View();
        }

        if (exito)
        {
            return controller.Json(new
            {
                success = true,
                message = mensaje,
                redirectUrl = redirectUrl ?? controller.Url.Action("Index")
            });
        }

        return controller.BadRequest(new { success = false, message = mensaje });
    }
}
