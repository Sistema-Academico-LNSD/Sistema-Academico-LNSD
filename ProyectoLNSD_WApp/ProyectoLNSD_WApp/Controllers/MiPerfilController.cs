using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.Service.Encargado;
using ProyectoLNSD_WApp.BLL.Service.Estudiante;

namespace ProyectoLNSD_WApp.Controllers
{
    /// <summary>
    /// Vistas de consulta para el propio estudiante (MESF-01-11) y para el encargado (MESF-01-12).
    /// El acceso se controla por rol, y los datos se buscan siempre por la cuenta en sesión:
    /// nunca se recibe un id desde la URL, así nadie puede consultar el expediente de otra persona.
    /// </summary>
    [Authorize]
    public class MiPerfilController : Controller
    {
        private readonly IEstudianteService _estudianteService;
        private readonly IEncargadoService _encargadoService;

        public MiPerfilController(
            IEstudianteService estudianteService,
            IEncargadoService encargadoService)
        {
            _estudianteService = estudianteService;
            _encargadoService = encargadoService;
        }

        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> MiExpediente()
        {
            var respuesta = await _estudianteService.GetMiExpediente(IdUsuarioActual());

            return View(respuesta);
        }

        [HttpGet]
        [Authorize(Roles = "Encargado")]
        public async Task<IActionResult> MisEstudiantes()
        {
            var respuesta = await _encargadoService.GetMisEstudiantes(IdUsuarioActual());

            return View(respuesta);
        }

        private int IdUsuarioActual() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
