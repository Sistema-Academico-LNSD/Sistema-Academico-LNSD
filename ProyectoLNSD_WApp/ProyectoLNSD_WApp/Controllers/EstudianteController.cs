using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Encargado;
using ProyectoLNSD_WApp.BLL.Service.Estudiante;
using ProyectoLNSD_WApp.BLL.Service.Grado;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class EstudianteController : BaseController
    {
        private readonly IEstudianteService _estudianteService;
        private readonly IGradoService _gradoService;
        private readonly IEncargadoService _encargadoService;

        public EstudianteController(
            IEstudianteService estudianteService,
            IGradoService gradoService,
            IEncargadoService encargadoService)
        {
            _estudianteService = estudianteService;
            _gradoService = gradoService;
            _encargadoService = encargadoService;
        }

        [HttpGet]
        [RequierePermiso("Estudiantes", "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Estudiantes", "Ver")]
        public async Task<IActionResult> Buscar(string? texto, int? idGrado, bool? estado)
        {
            return Json(await _estudianteService.Buscar(texto, idGrado, estado));
        }

        [HttpGet]
        [RequierePermiso("Estudiantes", "Ver")]
        public async Task<IActionResult> GetEstudiante(int id)
        {
            return Json(await _estudianteService.GetById(id));
        }

        [HttpGet]
        [RequierePermiso("Estudiantes", "Ver")]
        public async Task<IActionResult> GetGrados()
        {
            return Json(await _gradoService.GetGradosActivos());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Crear")]
        public async Task<IActionResult> Crear(EstudianteCrearDTO estudiante)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _estudianteService.Crear(estudiante));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> Actualizar(EstudianteDTO estudiante)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _estudianteService.Actualizar(estudiante));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> CambiarEstado(int id, bool estado)
        {
            return Json(await _estudianteService.CambiarEstado(id, estado));
        }

        // ---- Encargados (MESF-01-03 y MESF-01-04) ----

        [HttpGet]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> GetEncargados(int idEstudiante)
        {
            return Json(await _encargadoService.GetPorEstudiante(idEstudiante));
        }

        [HttpGet]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> BuscarEncargados(string? texto, int idEstudiante)
        {
            return Json(await _encargadoService.Buscar(texto, idEstudiante));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> VincularEncargado(VincularEncargadoDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _encargadoService.Vincular(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> CrearEncargado(CrearEncargadoDTO dto)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _encargadoService.CrearYVincular(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> DefinirEncargadoPrincipal(int idEstudiante, int idEncargado)
        {
            return Json(await _encargadoService.DefinirPrincipal(idEstudiante, idEncargado));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Estudiantes", "Editar")]
        public async Task<IActionResult> DesvincularEncargado(int idEstudiante, int idEncargado)
        {
            return Json(await _encargadoService.Desvincular(idEstudiante, idEncargado));
        }
    }
}
