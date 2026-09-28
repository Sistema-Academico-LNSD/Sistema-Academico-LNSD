using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Institucion;
using ProyectoLNSD_WApp.BLL.Service.PeriodoLectivo;
using ProyectoLNSD_WApp.Utilidades;

namespace ProyectoLNSD_WApp.Controllers
{
    // Un solo controller para todo el módulo MCFG; cada HU agrega sus acciones aquí.
    [Authorize]
    public class ConfiguracionController : BaseController
    {
        private readonly IInstitucionService _institucionService;
        private readonly IPeriodoLectivoService _periodoService;
        private readonly IWebHostEnvironment _entorno;

        public ConfiguracionController(IInstitucionService institucionService, IPeriodoLectivoService periodoService, IWebHostEnvironment entorno)
        {
            _institucionService = institucionService;
            _periodoService = periodoService;
            _entorno = entorno;
        }

        // GET: /Configuracion/Index  (pantalla única del módulo; cada HU agrega su pestaña)
        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public IActionResult Index()
        {
            return View(new InstitucionDTO());
        }

        // ---------- HU 01: Institución ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetInstitucion()
        {
            var respuesta = await _institucionService.GetInstitucion();
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarInstitucion(InstitucionDTO institucion, IFormFile? logo)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            string? nuevaRutaLogo = null;

            if (logo is { Length: > 0 })
            {
                var (ok, resultado) = await ImagenHelper.GuardarAsync(logo, _entorno, "logo");
                if (!ok)
                    return Json(RespuestaDTO<InstitucionDTO>.Error(resultado, 3004));

                nuevaRutaLogo = resultado;
            }

            var respuesta = await _institucionService.GuardarInstitucion(institucion, nuevaRutaLogo);
            return Json(respuesta);
        }

        // ---------- HU 02: Períodos lectivos ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetPeriodos()
        {
            var respuesta = await _periodoService.GetPeriodos();
            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetPeriodo(int id)
        {
            var respuesta = await _periodoService.GetPeriodo(id);
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarPeriodo(PeriodoLectivoDTO periodo, bool confirmarCambio = false)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _periodoService.GuardarPeriodo(periodo, confirmarCambio);
            return Json(respuesta);
        }
    }
}
