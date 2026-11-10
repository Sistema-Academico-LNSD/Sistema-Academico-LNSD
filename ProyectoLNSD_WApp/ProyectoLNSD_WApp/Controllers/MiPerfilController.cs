using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.Service.Docente;
using ProyectoLNSD_WApp.BLL.Service.Encargado;
using ProyectoLNSD_WApp.BLL.Service.Estudiante;

namespace ProyectoLNSD_WApp.Controllers
{
    /// <summary>
    /// Vistas de consulta para el propio estudiante (MESF-01-11), el encargado (MESF-01-12)
    /// y el docente (MDOF-01-11).
    /// El acceso se controla por rol, y los datos se buscan siempre por la cuenta en sesión:
    /// nunca se recibe un id desde la URL, así nadie puede consultar el expediente de otra persona.
    /// </summary>
    [Authorize]
    public class MiPerfilController : Controller
    {
        private readonly IEstudianteService _estudianteService;
        private readonly IEncargadoService _encargadoService;
        private readonly IExpedienteDocenteService _expedienteDocenteService;

        public MiPerfilController(
            IEstudianteService estudianteService,
            IEncargadoService encargadoService,
            IExpedienteDocenteService expedienteDocenteService)
        {
            _estudianteService = estudianteService;
            _encargadoService = encargadoService;
            _expedienteDocenteService = expedienteDocenteService;
        }

        // MDOF-01-11: el docente consulta su propio expediente (el menú ya enlazaba aquí, pero la acción no existía).
        [HttpGet]
        [Authorize(Roles = RolesSistema.Docente)]
        public async Task<IActionResult> Index()
        {
            var respuesta = await _expedienteDocenteService.GetPorUsuario(IdUsuarioActual());

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
            {
                ViewData["Mensaje"] = respuesta.Mensaje;
                return View(model: null);
            }

            return View(respuesta.Dato);
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Estudiante)]
        public async Task<IActionResult> MiExpediente()
        {
            var respuesta = await _estudianteService.GetMiExpediente(IdUsuarioActual());

            return View(respuesta);
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Encargado)]
        public async Task<IActionResult> MisEstudiantes()
        {
            var respuesta = await _encargadoService.GetMisEstudiantes(IdUsuarioActual());

            return View(respuesta);
        }

        private int IdUsuarioActual() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
