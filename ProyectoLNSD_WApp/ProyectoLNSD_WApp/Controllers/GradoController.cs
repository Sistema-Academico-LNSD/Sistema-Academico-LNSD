using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Grado;
using ProyectoLNSD_WApp.BLL.Service.Seccion;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class GradoController : BaseController
    {
        private readonly IGradoService _gradoService;
        private readonly ISeccionService _seccionService;

        public GradoController(IGradoService gradoService, ISeccionService seccionService)
        {
            _gradoService = gradoService;
            _seccionService = seccionService;
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public IActionResult Detalle(int id)
        {
            ViewBag.IdGrado = id;
            return View();
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public async Task<IActionResult> GetEstructura(string? texto, string? nivel)
        {
            return Json(await _gradoService.GetEstructura(texto, nivel));
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public async Task<IActionResult> GetGradosActivos()
        {
            return Json(await _gradoService.GetGradosActivos());
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public async Task<IActionResult> GetGradoById(int id)
        {
            return Json(await _gradoService.GetGradoById(id));
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public async Task<IActionResult> GetDetalleGrado(int id)
        {
            return Json(await _gradoService.GetDetalleGrado(id));
        }

        [HttpGet]
        [RequierePermiso("Grados", "Ver")]
        public async Task<IActionResult> GetSeccionById(int id)
        {
            return Json(await _seccionService.GetSeccionById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Crear")]
        public async Task<IActionResult> CreateGrado(GradoDTO grado)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _gradoService.CreateGrado(grado));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Editar")]
        public async Task<IActionResult> UpdateGrado(GradoDTO grado)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _gradoService.UpdateGrado(grado));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Editar")]
        public async Task<IActionResult> CambiarEstadoGrado(int id, bool estado)
        {
            return Json(await _gradoService.CambiarEstadoGrado(id, estado));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Crear")]
        public async Task<IActionResult> CreateSeccion(SeccionDTO seccion)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _seccionService.CreateSeccion(seccion));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Editar")]
        public async Task<IActionResult> UpdateSeccion(SeccionDTO seccion)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _seccionService.UpdateSeccion(seccion));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Grados", "Editar")]
        public async Task<IActionResult> CambiarEstadoSeccion(int id, bool estado)
        {
            return Json(await _seccionService.CambiarEstadoSeccion(id, estado));
        }
    }
}