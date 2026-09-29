using Microsoft.AspNetCore.Mvc;

namespace ProyectoLNSD_WApp.Controllers
{

    /// Controlador base con JSON response a las pantallas (Usuario, Rol, etc.).
    public abstract class BaseController : Controller
    {
        /// Devuelve el error de validación del ModelState en formato de RespuestaDTO, JS lo muestra al usuario igual que cualquier otro error de negocio.
        protected IActionResult RespuestaModeloInvalido()
        {
            string mensaje = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "Los datos enviados no son válidos.";

            return Json(new
            {
                esCorrecto = false,
                mensaje
            });
        }
    }
}