using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO.Curso;
using ProyectoLNSD_WApp.BLL.Service.Curso;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class CursoController : BaseController
    {
        private readonly ICursoService _cursoService;

        public CursoController(ICursoService cursoService)
        {
            _cursoService = cursoService;
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public async Task<IActionResult> Listar(
            string? texto,
            int? idGrado,
            int? idArea,
            bool? estado)
        {
            var respuesta = await _cursoService.GetCursos(
                texto,
                idGrado,
                idArea,
                estado);

            return Ok(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public async Task<IActionResult> Obtener(
            int id)
        {
            var respuesta = await _cursoService.GetCursoById(id);

            return StatusCode(
                respuesta.Codigo == 0 ? 200 : respuesta.Codigo,
                respuesta);
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public async Task<IActionResult> Detalle(
            int id)
        {
            CursoDetalleDTO? detalle = null;

            var respuesta =
                await _cursoService.GetDetalleCurso(id);

            if (respuesta.EsCorrecto)
            {
                detalle = respuesta.Dato;
            }

            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public async Task<IActionResult> ObtenerAreas()
        {
            var respuesta =
                await _cursoService.GetAreasActivas();

            return Ok(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Cursos", "Ver")]
        public async Task<IActionResult> ObtenerGrados()
        {
            var respuesta =
                await _cursoService.GetGradosActivos();

            return Ok(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Cursos", "Crear")]
        public async Task<IActionResult> Crear(
            CursoDTO curso)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    esCorrecto = false,
                    mensaje = "Complete correctamente los campos requeridos"
                });
            }

            var respuesta =
                await _cursoService.CreateCurso(curso);

            return StatusCode(
                respuesta.Codigo == 0 ? 200 : 400,
                respuesta);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Cursos", "Editar")]
        public async Task<IActionResult> Editar(
            CursoDTO curso)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    esCorrecto = false,
                    mensaje = "Complete correctamente los campos requeridos"
                });
            }

            var respuesta =
                await _cursoService.UpdateCurso(curso);

            return StatusCode(
                respuesta.Codigo == 0 ? 200 : 400,
                respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Cursos", "Editar")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            bool estado)
        {
            var respuesta =
                await _cursoService.CambiarEstadoCurso(
                    id,
                    estado);

            return StatusCode(
                respuesta.Codigo == 0 ? 200 : 400,
                respuesta);
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Cursos", "Editar")]
        public async Task<IActionResult> Eliminar(
            int id)
        {
            var respuesta =
                await _cursoService.EliminarCurso(id);

            return StatusCode(
                respuesta.Codigo == 0 ? 200 : 400,
                respuesta);
        }
    }
}