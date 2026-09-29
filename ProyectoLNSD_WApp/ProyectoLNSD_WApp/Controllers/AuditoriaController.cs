using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.Service.Auditoria;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetHistorial(
            string? correo,
            DateTime? desde,
            DateTime? hasta,
            bool? soloFallidos)
        {
            var respuesta = await _auditoriaService.GetHistorial(correo, desde, hasta, soloFallidos);

            return Json(respuesta);
        }
    }
}