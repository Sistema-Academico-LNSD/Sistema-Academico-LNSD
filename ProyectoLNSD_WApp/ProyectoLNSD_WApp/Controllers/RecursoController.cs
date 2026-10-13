using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Recurso;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class RecursoController : BaseController
    {
        private readonly IRecursoService _recursoService;

        public RecursoController(IRecursoService recursoService)
        {
            _recursoService = recursoService;
        }

        // ---- Docente: publicar recursos (MRAF-01-01, 01-02 y 01-03) ----

        [HttpGet]
        [RequierePermiso("Recursos", "Crear")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso("Recursos", "Crear")]
        public async Task<IActionResult> GetMisCursos()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            var respuesta = await _recursoService.GetMisCursos(idUsuario);

            return Json(respuesta);
        }

        // 12 MB: el servicio limita el archivo a 10 MB y el resto es margen del formulario.
        // Si una solicitud lo supera, el servidor responde 413 antes de llegar aquí.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Recursos", "Crear")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Publicar(RecursoGuardarDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            if (archivo is { Length: > 0 })
            {
                await using var contenido = archivo.OpenReadStream();

                var respuesta = await _recursoService.Publicar(idUsuario, dto, new ArchivoRecursoDTO
                {
                    NombreOriginal = archivo.FileName,
                    Tamano = archivo.Length,
                    Contenido = contenido
                });

                return Json(respuesta);
            }

            return Json(await _recursoService.Publicar(idUsuario, dto, null));
        }

        // ---- Docente: mis recursos (MRAF-01-04 a 01-07) ----

        [HttpGet]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetMisRecursos(RecursoFiltroDTO filtro)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _recursoService.GetMisRecursos(idUsuario, filtro));
        }

        [HttpGet]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetRecurso(int idRecurso)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _recursoService.GetRecurso(idUsuario, idRecurso));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Recursos", "Editar")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> Actualizar(RecursoGuardarDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            if (archivo is { Length: > 0 })
            {
                await using var contenido = archivo.OpenReadStream();

                var respuesta = await _recursoService.Actualizar(idUsuario, dto, new ArchivoRecursoDTO
                {
                    NombreOriginal = archivo.FileName,
                    Tamano = archivo.Length,
                    Contenido = contenido
                });

                return Json(respuesta);
            }

            return Json(await _recursoService.Actualizar(idUsuario, dto, null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Recursos", "Eliminar")]
        public async Task<IActionResult> Eliminar(int idRecurso)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _recursoService.Eliminar(idUsuario, idRecurso));
        }


        // ---- Estudiante: material de estudio (MRAF-01-08) ----

        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        [RequierePermiso("Recursos", "Ver")]
        public IActionResult Material()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetFiltrosMaterial()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _recursoService.GetFiltrosMaterial(idUsuario));
        }

        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetMaterial(int? idCurso)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _recursoService.GetMaterialEstudiante(idUsuario, idCurso));
        }

        // ---- Descarga (MRAF-01-09): la usan estudiante, docente y supervisores ----

        [HttpGet]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> Descargar(int idRecurso)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            bool esSupervisor = User.IsInRole("Administrador") || User.IsInRole("Director");

            var respuesta = await _recursoService.Descargar(idUsuario, idRecurso, esSupervisor);

            // Los errores salen como JSON para que la pantalla los muestre como mensaje
            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return Json(respuesta);

            Response.Headers["X-Content-Type-Options"] = "nosniff";

            // "File" con nombre de descarga lo entrega siempre como adjunto, nunca se abre en el navegador
            return File(respuesta.Dato.Contenido, respuesta.Dato.ContentType, respuesta.Dato.NombreArchivo);
        }

        // ---- Administrador y director: supervisión (MRAF-01-10) ----

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Recursos", "Ver")]
        public IActionResult Supervision()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetFiltrosSupervision()
        {
            return Json(await _recursoService.GetFiltrosSupervision());
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> GetRecursosSupervision(RecursoFiltroDTO filtro)
        {
            return Json(await _recursoService.GetRecursosSupervision(filtro));
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso("Recursos", "Ver")]
        public async Task<IActionResult> ExportarReporte(RecursoFiltroDTO filtro)
        {
            var respuesta = await _recursoService.ExportarReporte(filtro);

            // Si no hay registros o falla, sale como JSON para mostrarlo como mensaje
            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return Json(respuesta);

            return File(
                respuesta.Dato,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Recursos_Academicos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }


        // ---- Auxiliares ----

        /// El id sale siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerIdUsuario(out int idUsuario)
        {
            return int.TryParse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                out idUsuario);
        }

        private IActionResult RespuestaSesionInvalida()
        {
            return Json(new
            {
                esCorrecto = false,
                mensaje = "La sesión no es válida. Inicie sesión nuevamente."
            });
        }
    }
}