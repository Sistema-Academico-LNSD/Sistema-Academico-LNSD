using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Pauta;
using ProyectoLNSD_WApp.Utilidades;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    /// Documento imprimible de las pautas (MPPAF-01-08 y 01-09).
    /// La página se abre en otra pestaña; desde ahí se imprime o se guarda como PDF.
    [Authorize]
    public class PautaDocumentoController : Controller
    {
        private const string Modulo = "Pautas";

        private readonly IPautaDocumentoService _documentoService;

        public PautaDocumentoController(IPautaDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Formulario(int id)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Challenge();

            return Vista(await _documentoService.GetFormulario(idUsuario, rol, id));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Expediente(int id)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Challenge();

            return Vista(await _documentoService.GetExpediente(idUsuario, rol, id));
        }

        private IActionResult Vista(RespuestaDTO<DocumentoPautaDTO> respuesta)
        {
            if (!respuesta.EsCorrecto || respuesta.Dato == null)
            {
                ViewData["Error"] = respuesta.Mensaje;
                return View("Documento", string.Empty);
            }

            var doc = respuesta.Dato;

            ViewData["Titulo"] = doc.EsExpedienteCompleto
                ? $"Expediente {doc.CodigoExpediente}"
                : $"Formulario N°{doc.Formularios[0].NumeroFormulario} - {doc.CodigoExpediente}";

            return View("Documento", PautaDocumentoRenderer.Renderizar(doc));
        }

        /// El id y el rol salen siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerSesion(out int idUsuario, out string rol)
        {
            rol = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idUsuario)
                   && rol.Length > 0;
        }
    }
}
