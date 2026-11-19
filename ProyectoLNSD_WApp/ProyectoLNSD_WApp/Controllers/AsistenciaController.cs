using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Asistencia;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    /// Control de asistencia (MCAF-01). Tres pantallas:
    /// - Registrar: el docente pasa lista de sus clases.
    /// - Consulta: administración, dirección y docentes (estos últimos solo ven sus clases).
    /// - MiAsistencia: estudiante y encargado ven solo lo propio.
    [Authorize]
    public class AsistenciaController : BaseController
    {
        private const string Modulo = "Asistencia";

        private readonly IAsistenciaService _asistenciaService;

        public AsistenciaController(IAsistenciaService asistenciaService)
        {
            _asistenciaService = asistenciaService;
        }

        // ---- Docente: pasar lista (MCAF-01-01 a 01-06) ----

        [HttpGet]
        [Authorize(Roles = RolesSistema.Docente)]
        [RequierePermiso(Modulo, "Crear")]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Docente)]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> GetMisClases()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetMisClases(idUsuario));
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Docente)]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> GetLista(int idHorario, DateOnly fecha)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetLista(idUsuario, idHorario, fecha, PuedeEditar()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesSistema.Docente)]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> Guardar([FromBody] AsistenciaGuardarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.Guardar(idUsuario, dto, PuedeEditar()));
        }

        // MCAF-01-06: lo usan el docente (sus clases) y administración/dirección (todo)
        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetHistorial(int idMatricula, int idCurso)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetHistorial(idUsuario, SoloPropias(), idMatricula, idCurso));
        }

        // ---- Consulta y reportes (MCAF-01-09, 01-10 y 01-12) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Consulta()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetOpciones(int? idPeriodo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetOpciones(idUsuario, SoloPropias(), idPeriodo));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetResumen(AsistenciaFiltroDTO filtro)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetResumen(idUsuario, SoloPropias(), filtro));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetRegistros(AsistenciaFiltroDTO filtro)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.GetRegistros(idUsuario, SoloPropias(), filtro));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> ExportarResumen(AsistenciaFiltroDTO filtro)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            var respuesta = await _asistenciaService.ExportarResumen(idUsuario, SoloPropias(), filtro);

            // Sin datos o con error sale como JSON, para mostrarlo como mensaje
            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return Json(respuesta);

            return File(
                respuesta.Dato,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Asistencia_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // ---- Cierre del período (MCAF-01-13) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetCierre(int idPeriodo)
        {
            return Json(await _asistenciaService.GetCierre(idPeriodo));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesSistema.Administrador + "," + RolesSistema.Director)]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> CerrarPeriodo(int idPeriodo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _asistenciaService.CerrarPeriodo(idUsuario, idPeriodo));
        }

        // ---- Estudiante y encargado (MCAF-01-07 y 01-08) ----

        [HttpGet]
        [Authorize(Roles = RolesSistema.Estudiante + "," + RolesSistema.Encargado)]
        public IActionResult MiAsistencia()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Estudiante + "," + RolesSistema.Encargado)]
        public async Task<IActionResult> GetMiAsistencia(int? idPeriodo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            bool esEncargado = User.IsInRole(RolesSistema.Encargado) && !User.IsInRole(RolesSistema.Estudiante);

            return Json(await _asistenciaService.GetMiAsistencia(idUsuario, esEncargado, idPeriodo));
        }

        // ---- Auxiliares ----

        /// Un docente solo consulta sus propias clases; administración y dirección ven todo.
        private bool SoloPropias() =>
            User.IsInRole(RolesSistema.Docente) &&
            !User.IsInRole(RolesSistema.Administrador) &&
            !User.IsInRole(RolesSistema.Director);

        /// MCAF-01-05: corregir una lista ya guardada
        private bool PuedeEditar() => User.TienePermiso(Modulo, "Editar");

        /// El id sale siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerIdUsuario(out int idUsuario)
        {
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idUsuario);
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
