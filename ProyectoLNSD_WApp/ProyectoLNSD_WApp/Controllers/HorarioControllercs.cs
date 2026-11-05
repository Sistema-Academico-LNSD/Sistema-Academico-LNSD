using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Horario;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class HorarioController : BaseController
    {
        private readonly IHorarioService _horarioService;

        public HorarioController(IHorarioService horarioService)
        {
            _horarioService = horarioService;
        }

        // ---- Administrador y director: gestión de horarios (MHOF-01-01 a 01-04) ----

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetOpciones()
        {
            return Json(await _horarioService.GetOpciones());
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetGrupos(int idSeccion)
        {
            return Json(await _horarioService.GetGrupos(idSeccion));
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetCursos(int idSeccion)
        {
            return Json(await _horarioService.GetCursos(idSeccion));
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetDocentes(int idCurso)
        {
            return Json(await _horarioService.GetDocentes(idCurso));
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetHorarios(HorarioFiltroDTO filtro)
        {
            return Json(await _horarioService.GetHorarios(filtro));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> Registrar(HorarioGuardarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _horarioService.Registrar(dto));
        }


        // ---- Consulta, edición y estado (MHOF-01-05, 01-06, 01-11 y 01-12) ----

        [HttpGet]
        [RequierePermiso("Horarios", "Ver")]
        public async Task<IActionResult> GetPeriodos()
        {
            return Json(await _horarioService.GetPeriodos());
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetHorario(int idHorario)
        {
            return Json(await _horarioService.GetHorario(idHorario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Horarios", "Editar")]
        public async Task<IActionResult> Actualizar(HorarioGuardarDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _horarioService.Actualizar(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Horarios", "Editar")]
        public async Task<IActionResult> CambiarEstado(int idHorario, bool activar)
        {
            return Json(await _horarioService.CambiarEstado(idHorario, activar));
        }

        // ---- Docente: Mi horario (MHOF-01-10) ----

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Horarios", "Ver")]
        public IActionResult MiHorario()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Docente")]
        [RequierePermiso("Horarios", "Ver")]
        public async Task<IActionResult> GetMiHorario(int? idPeriodo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            // El docente siempre consulta lo suyo: el id sale de la sesión, no del navegador
            return Json(await _horarioService.GetMiHorarioDocente(idUsuario, idPeriodo));
        }

        // ---- Administración: horarios por docente (MHOF-01-07) ----

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public IActionResult PorDocente()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetDocentesConsulta()
        {
            return Json(await _horarioService.GetDocentesConsulta());
        }

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public async Task<IActionResult> GetHorariosDeDocente(int idDocente, int? idPeriodo)
        {
            return Json(await _horarioService.GetHorariosDeDocente(idDocente, idPeriodo));
        }

        // ---- Estudiante y encargado: horario según su matrícula (MHOF-01-08 y 01-09) ----

        [HttpGet]
        [Authorize(Roles = "Estudiante,Encargado")]
        [RequierePermiso("Horarios", "Ver")]
        public IActionResult PorMatricula()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Estudiante,Encargado")]
        [RequierePermiso("Horarios", "Ver")]
        public async Task<IActionResult> GetHorarioPorMatricula()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            bool esEncargado = User.IsInRole("Encargado") && !User.IsInRole("Estudiante");

            return Json(await _horarioService.GetHorariosDeMatriculas(idUsuario, esEncargado));
        }

        // ---- Administración: horarios por grado, grupo o sección (MHOF-01-08) ----

        [HttpGet]
        [RequierePermiso("Horarios", "Crear")]
        public IActionResult PorGrupo()
        {
            return View();
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