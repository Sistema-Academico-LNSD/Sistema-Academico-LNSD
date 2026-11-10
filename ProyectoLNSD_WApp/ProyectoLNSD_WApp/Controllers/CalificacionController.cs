using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Calificacion;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class CalificacionController : BaseController
    {
        private readonly ICalificacionService _calificacionService;

        public CalificacionController(ICalificacionService calificacionService)
        {
            _calificacionService = calificacionService;
        }

        // ---- Docente: registro de calificaciones (MCNF-01-01, 01-02, 01-03 y 01-12) ----

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> GetPeriodos()
        {
            return Json(await _calificacionService.GetPeriodos());
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> GetMisClases(int? idPeriodo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            // El docente siempre consulta lo suyo: el id sale de la sesión, no del navegador
            return Json(await _calificacionService.GetMisClases(idUsuario, idPeriodo));
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> GetPlanilla(int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.GetPlanilla(idUsuario, idPeriodo, idCurso, idSeccion, idGrupo));
        }

        // Los POST reciben JSON (FromBody) para que un decimal como 85.5 no dependa de la cultura del servidor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> CrearActividad([FromBody] ActividadCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.CrearActividad(idUsuario, dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> GuardarNota([FromBody] NotaGuardarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.GuardarNota(idUsuario, dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Crear")]
        public async Task<IActionResult> GuardarNotas([FromBody] List<NotaGuardarDTO> notas)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.GuardarNotas(idUsuario, notas));
        }

        // ---- Auxiliares ----

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
