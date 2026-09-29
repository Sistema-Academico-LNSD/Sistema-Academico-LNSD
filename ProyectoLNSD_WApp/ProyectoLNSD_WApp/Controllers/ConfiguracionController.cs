using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.AccesoRapido;
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
        private readonly IAccesoRapidoService _accesoService;
        private readonly IWebHostEnvironment _entorno;

        public ConfiguracionController(IInstitucionService institucionService, IPeriodoLectivoService periodoService, IContenidoSitioService contenidoService, IAccesoRapidoService accesoService, IWebHostEnvironment entorno)
        {
            _institucionService = institucionService;
            _periodoService = periodoService;
            _contenidoService = contenidoService;
            _accesoService = accesoService;
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

        // ---------- HU 03: Imágenes y banners ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetBanners()
        {
            var respuesta = await _contenidoService.GetBanners();
            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var respuesta = await _contenidoService.GetBanner(id);
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarBanner(ContenidoSitioDTO contenido, IFormFile? imagen, bool publicar)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            // Ruta de la imagen actual (si se está editando), para borrarla si se reemplaza
            string? rutaAnterior = null;
            if (contenido.IdContenido > 0)
            {
                var actual = await _contenidoService.GetBanner(contenido.IdContenido);
                rutaAnterior = actual.Dato?.RutaImagen;
            }

            // HU escenario 4: archivo que no es imagen (o muy pesado) se rechaza
            string? nuevaRuta = null;
            if (imagen is { Length: > 0 })
            {
                var (ok, resultado) = await ImagenHelper.GuardarAsync(imagen, _entorno, "banners");
                if (!ok)
                    return Json(RespuestaDTO<ContenidoSitioDTO>.Error(resultado, 3204));

                nuevaRuta = resultado;
            }

            var respuesta = await _contenidoService.GuardarBanner(contenido, publicar, nuevaRuta, IdUsuarioActual);

            if (nuevaRuta != null)
            {
                // Si se guardó bien se borra la imagen vieja; si falló, se borra la recién subida
                ImagenHelper.Eliminar(respuesta.EsCorrecto ? rutaAnterior : nuevaRuta, _entorno);
            }

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Eliminar")]
        public async Task<IActionResult> EliminarBanner(int id)
        {
            var actual = await _contenidoService.GetBanner(id);
            string? ruta = actual.Dato?.RutaImagen;

            var respuesta = await _contenidoService.EliminarBanner(id);

            if (respuesta.EsCorrecto)
                ImagenHelper.Eliminar(ruta, _entorno);

            return Json(respuesta);
        }

        // ---------- HU 08: Publicar / despublicar ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> CambiarEstadoContenido(int id, bool publicar)
        {
            var respuesta = await _contenidoService.CambiarEstado(id, publicar, IdUsuarioActual);
            return Json(respuesta);
        }

        // ---------- HU 07: Vista previa ----------

        // Muestra la landing con los borradores incluidos; no cambia nada por sí sola.
        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> VistaPrevia()
        {
            var respuesta = await _contenidoService.GetVistaPrevia();
            return View(respuesta.Dato ?? new LandingDTO());
        }

        // Botón "Confirmar publicación" de la vista previa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> PublicarPendientes()
        {
            var respuesta = await _contenidoService.PublicarPendientes(IdUsuarioActual);
            return Json(respuesta);
        }

        // ---------- HU 06: Accesos rápidos ----------

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetAccesosRapidos()
        {
            var respuesta = await _accesoService.GetAccesos();
            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetAccesoRapido(int id)
        {
            var respuesta = await _accesoService.GetAcceso(id);
            return Json(respuesta);
        }

        // Lista de roles del formulario (casillas "Visible para")
        [HttpGet]
        [RequierePermiso("Configuracion", "Ver")]
        public async Task<IActionResult> GetRolesAcceso()
        {
            var respuesta = await _accesoService.GetRoles();
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Editar")]
        public async Task<IActionResult> GuardarAccesoRapido(AccesoRapidoDTO acceso)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _accesoService.GuardarAcceso(acceso);
            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Configuracion", "Eliminar")]
        public async Task<IActionResult> EliminarAccesoRapido(int id)
        {
            var respuesta = await _accesoService.EliminarAcceso(id);
            return Json(respuesta);
        }
    }
}