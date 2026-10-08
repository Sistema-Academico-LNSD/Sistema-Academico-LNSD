using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.Service.Docente;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class AsignacionCursoController : BaseController
    {
        private readonly IDocenteCursoService _docenteCursoService;

        public AsignacionCursoController(IDocenteCursoService docenteCursoService)
        {
            _docenteCursoService = docenteCursoService;
        }

        // ---------- Lectura (GET) ----------
        [HttpGet]
        [RequierePermiso("AsignacionCursos", "Ver")]
        public async Task<IActionResult> GetCursosDocente(int idDocente)
        {
            var respuesta = await _docenteCursoService.GetCursosDocente(idDocente);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("AsignacionCursos", "Crear")]
        public async Task<IActionResult> GetCursosDisponibles(int idDocente)
        {
            var respuesta = await _docenteCursoService.GetCursosDisponibles(idDocente);

            return Json(respuesta);
        }

        // ---------- Escritura (POST + antiforgery) ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("AsignacionCursos", "Crear")]
        public async Task<IActionResult> Asignar(int idDocente, int idCurso)
        {
            var respuesta = await _docenteCursoService.Asignar(idDocente, idCurso);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("AsignacionCursos", "Eliminar")]
        public async Task<IActionResult> Quitar(int idDocenteCurso)
        {
            var respuesta = await _docenteCursoService.Quitar(idDocenteCurso);

            return Json(respuesta);
        }
    }
}