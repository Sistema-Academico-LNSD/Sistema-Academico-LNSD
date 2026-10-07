using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.Service.Docente;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class MiPerfilController : BaseController
    {
        private readonly IExpedienteDocenteService _expedienteService;

        public MiPerfilController(IExpedienteDocenteService expedienteService)
        {
            _expedienteService = expedienteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int idUsuario))
            {
                return RedirectToAction("Login", "Auth");
            }

            var respuesta = await _expedienteService.GetPorUsuario(idUsuario);

            if (!respuesta.EsCorrecto)
            {
                ViewData["Mensaje"] = respuesta.Mensaje;

                return View(model: null);
            }

            return View(respuesta.Dato);
        }
    }
}