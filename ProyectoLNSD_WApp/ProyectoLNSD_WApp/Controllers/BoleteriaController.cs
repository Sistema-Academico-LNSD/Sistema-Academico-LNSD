using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.Service.Tiquete;
using ProyectoLNSD_WApp.Authorization;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class BoleteriaController : BaseController
    {
        private readonly ITiqueteService _tiqueteService;

        public BoleteriaController(ITiqueteService tiqueteService)
        {
            _tiqueteService = tiqueteService;
        }

        // GET: /Boleteria/Index  (pantalla "Mi tiquete")

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMisTiquetes()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            var respuesta = await _tiqueteService.GetMisTiquetes(idUsuario);

            return Json(respuesta);
        }

        [HttpGet]
        public async Task<IActionResult> GetTiqueteActual()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            var respuesta = await _tiqueteService.GetTiqueteActual(idUsuario);

            return Json(respuesta);
        }

        // POST & antiforgery

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarTiquete()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            var respuesta = await _tiqueteService.GenerarTiquete(idUsuario);

            return Json(respuesta);
        }

        // ---- Encargado del comedor (MBLF-01-04 y 01-05) ----

        [HttpGet]
        [RequierePermiso("Boleteria", "Editar")]
        public IActionResult Validar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Boleteria", "Editar")]
        public async Task<IActionResult> ValidarTiquete(string codigo, string correo)
        {
            var respuesta = await _tiqueteService.ValidarTiquete(codigo, correo);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Boleteria", "Editar")]
        public async Task<IActionResult> MarcarUtilizado(string codigo, string correo)
        {
            if (!TryObtenerIdUsuario(out int idValidador))
                return RespuestaSesionInvalida();

            var respuesta = await _tiqueteService.MarcarComoUtilizado(codigo, correo, idValidador);

            return Json(respuesta);
        }

        // ---- Administrador (MBLF-01-06, 01-07 y 01-08) ----

        [HttpGet]
        [RequierePermiso("Boleteria", "Ver")]
        public IActionResult Consulta()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Boleteria", "Ver")]
        public async Task<IActionResult> GetTiquetes(string? estado)
        {
            var respuesta = await _tiqueteService.GetTiquetes(estado);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Boleteria", "Ver")]
        public async Task<IActionResult> GetResumen()
        {
            var respuesta = await _tiqueteService.GetResumen();

            return Json(respuesta);
        }

        // Auxiliares

        /// El id sale siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerIdUsuario(out int idUsuario)
        {
            return int.TryParse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                out idUsuario);
        }

        private IActionResult RespuestaSesionInvalida()
        {
            return Json(new
            {
                esCorrecto = false,
                mensaje = "La sesión no es válida. Inicie sesión nuevamente."
            });
        }
    }
}