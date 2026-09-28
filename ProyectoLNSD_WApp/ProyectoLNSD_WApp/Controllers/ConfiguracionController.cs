using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Institucion;
using ProyectoLNSD_WApp.Utilidades;

namespace ProyectoLNSD_WApp.Controllers
{
    // Un solo controller para todo el módulo MCFG; cada HU agrega sus acciones aquí.
    [Authorize]
    public class ConfiguracionController : BaseController
    {
        private readonly IInstitucionService _institucionService;
        private readonly IWebHostEnvironment _entorno;

        public ConfiguracionController(IInstitucionService institucionService, IWebHostEnvironment entorno)
        {
            _institucionService = institucionService;
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
    }
}
