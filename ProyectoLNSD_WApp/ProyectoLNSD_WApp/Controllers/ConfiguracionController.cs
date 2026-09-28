using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.ContenidoSitio;
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
        private readonly IContenidoSitioService _contenidoService;
        private readonly IWebHostEnvironment _entorno;

        public ConfiguracionController(IInstitucionService institucionService, IPeriodoLectivoService periodoService, IContenidoSitioService contenidoService, IWebHostEnvironment entorno)
        {
            _institucionService = institucionService;
            _periodoService = periodoService;
            _contenidoService = contenidoService;
            _entorno = entorno;
        }

        // Id del usuario en sesión (claim NameIdentifier), para registrar quién modificó el contenido.
        private int? IdUsuarioActual =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

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

        // ---------- HU 05: Misión, visión e historia ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetContenidosInstitucionales()
        {
            var respuesta = await _contenidoService.GetInstitucionales();
            return Json(respuesta);
        }

        // publicar = true (botón Publicar) / false (botón Guardar borrador)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarContenidoInstitucional(ContenidoSitioDTO contenido, bool publicar)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _contenidoService.GuardarInstitucional(contenido, publicar, IdUsuarioActual);
            return Json(respuesta);
        }

        // ---------- HU 04: Contenido de la página principal (bloques) ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetBloques()
        {
            var respuesta = await _contenidoService.GetBloques();
            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetBloque(int id)
        {
            var respuesta = await _contenidoService.GetBloque(id);
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarBloque(ContenidoSitioDTO contenido, bool publicar)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _contenidoService.GuardarBloque(contenido, publicar, IdUsuarioActual);
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Eliminar")]
        public async Task<IActionResult> EliminarBloque(int id)
        {
            var respuesta = await _contenidoService.EliminarBloque(id);
            return Json(respuesta);
        }
    }
}