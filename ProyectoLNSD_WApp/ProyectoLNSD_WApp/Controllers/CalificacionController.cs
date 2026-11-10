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
        private readonly IResultadoCalificacionService _resultadoService;

        public CalificacionController(
            ICalificacionService calificacionService,
            IResultadoCalificacionService resultadoService)
        {
            _calificacionService = calificacionService;
            _resultadoService = resultadoService;
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

        // ---- Docente: editar o corregir notas, observaciones e historial (MCNF-01-06, 01-07 y 01-08) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Editar")]
        public async Task<IActionResult> EditarNota([FromBody] NotaEditarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.EditarNota(idUsuario, dto));
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Calificaciones", "Ver")]
        public async Task<IActionResult> GetHistorial(int idMatricula, int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.GetHistorial(idUsuario, idMatricula, idPeriodo, idCurso, idSeccion, idGrupo));
        }

        // ---- Administrador y director: autorizar correcciones con el período cerrado (MCNF-01-13) ----

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Editar")]
        public IActionResult Correcciones()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Editar")]
        public async Task<IActionResult> GetPeriodosCorreccion()
        {
            return Json(await _calificacionService.GetPeriodos());
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Editar")]
        public async Task<IActionResult> GetClasesCorreccion(int idPeriodo)
        {
            return Json(await _calificacionService.GetClasesDelPeriodo(idPeriodo));
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Editar")]
        public async Task<IActionResult> GetNotasCorreccion(int idPeriodo, int idCurso, int idSeccion, int idGrupo)
        {
            return Json(await _calificacionService.GetNotasParaCorreccion(idPeriodo, idCurso, idSeccion, idGrupo));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Editar")]
        public async Task<IActionResult> AutorizarCorreccion([FromBody] CorreccionAutorizarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            // Quien autoriza sale de la sesión, no del navegador
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _calificacionService.AutorizarCorreccion(idUsuario, dto));
        }

        // ---- Administrador y director: nota del año y promoción de un estudiante (MCNF-01-05 y 01-14) ----
        // Las pantallas de consulta (parte 5) se apoyan en esta ruta.

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Calificaciones", "Ver")]
        public async Task<IActionResult> GetPromocion(int idEstudiante, int idPeriodo)
        {
            return Json(await _resultadoService.GetPromocion(idEstudiante, idPeriodo));
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
