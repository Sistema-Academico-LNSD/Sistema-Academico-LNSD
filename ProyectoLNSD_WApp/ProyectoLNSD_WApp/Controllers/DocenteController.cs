using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Curso;
using ProyectoLNSD_WApp.BLL.Service.Docente;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class DocenteController : BaseController
    {
        private readonly IDocenteService _docenteService;
        private readonly IDocenteCuentaService _cuentaService;
        private readonly ICursoService _cursoService;

        public DocenteController(
            IDocenteService docenteService,
            IDocenteCuentaService cuentaService,
            ICursoService cursoService)
        {
            _docenteService = docenteService;
            _cuentaService = cuentaService;
            _cursoService = cursoService;
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Ver")]
        public async Task<IActionResult> Buscar(string? texto, int? idArea, bool? estado)
        {
            var respuesta = await _docenteService.Buscar(texto, idArea, estado);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Ver")]
        public async Task<IActionResult> GetDocente(int id)
        {
            var respuesta = await _docenteService.GetById(id);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Ver")]
        public async Task<IActionResult> GetAreas()
        {
            var respuesta = await _cursoService.GetAreasActivas();

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Crear")]
        public async Task<IActionResult> Crear(DocenteDTO docente)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _docenteService.Crear(docente);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> Actualizar(DocenteDTO docente)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _docenteService.Actualizar(docente);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> CambiarEstado(int id, bool estado)
        {
            var respuesta = await _docenteService.CambiarEstado(id, estado);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> GetCuenta(int id)
        {
            var respuesta = await _cuentaService.GetCuenta(id);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> GetUsuariosDisponibles()
        {
            var respuesta = await _cuentaService.GetUsuariosDisponibles();

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> VincularCuenta(int idDocente, int idUsuario)
        {
            var respuesta = await _cuentaService.VincularCuentaExistente(idDocente, idUsuario);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> CrearCuenta(CrearCuentaDocenteDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _cuentaService.CrearCuenta(dto);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Docentes", "Editar")]
        public async Task<IActionResult> DesvincularCuenta(int idDocente)
        {
            var respuesta = await _cuentaService.Desvincular(idDocente);

            return Json(respuesta);
        }
    }
}