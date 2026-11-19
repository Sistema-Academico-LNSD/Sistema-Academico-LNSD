using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Beca;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    /// Becas socioeconómicas (MBEF-01). Tres pantallas:
    /// - Index: lista, búsqueda, registro, activación e historial (administración y dirección).
    /// - Requisitos: requisitos de elegibilidad (administración).
    /// - MiBeca: estudiante y encargado ven solo lo propio.
    [Authorize]
    public class BecaController : BaseController
    {
        private const string Modulo = "Becas";

        private readonly IBecaService _becaService;

        public BecaController(IBecaService becaService)
        {
            _becaService = becaService;
        }

        // ---- Lista, búsqueda e historial (MBEF-01-08, 01-09 y 01-10) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetOpciones()
        {
            return Json(await _becaService.GetOpciones());
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> Buscar(BecaFiltroDTO filtro)
        {
            return Json(await _becaService.Buscar(filtro));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetDetalle(int idBeca)
        {
            return Json(await _becaService.GetDetalle(idBeca));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetHistorial(int idEstudiante)
        {
            return Json(await _becaService.GetHistorial(idEstudiante));
        }

        // ---- Registro y actualización (MBEF-01-02, 01-03 y 01-06) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> BuscarEstudiantes(string? texto)
        {
            return Json(await _becaService.BuscarEstudiantes(texto));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> Preparar(int idEstudiante, int idPeriodo)
        {
            return Json(await _becaService.Preparar(idEstudiante, idPeriodo));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar([FromBody] BecaGuardarDTO dto)
        {
            // Registrar pide "Crear"; actualizar una existente pide "Editar"
            string accion = dto?.IdBeca > 0 ? "Editar" : "Crear";

            if (!User.TienePermiso(Modulo, accion))
                return Json(new
                {
                    esCorrecto = false,
                    mensaje = accion == "Editar"
                        ? "No tiene permiso para actualizar becas."
                        : "No tiene permiso para registrar becas."
                });

            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _becaService.Guardar(idUsuario, dto));
        }

        // ---- Activar y desactivar (MBEF-01-04 y 01-05) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> Activar(int idBeca)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _becaService.Activar(idUsuario, idBeca));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> Desactivar(int idBeca, string? motivo)
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            return Json(await _becaService.Desactivar(idUsuario, idBeca, motivo));
        }

        // ---- Requisitos de elegibilidad (MBEF-01-01) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Editar")]
        public IActionResult Requisitos()
        {
            return View();
        }

        /// Lo usa también el formulario de registro, por eso basta con "Ver"
        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetRequisitos(bool incluirInactivos = false)
        {
            return Json(await _becaService.GetRequisitos(incluirInactivos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> GuardarRequisito([FromBody] RequisitoBecaGuardarDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            return Json(await _becaService.GuardarRequisito(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> CambiarEstadoRequisito(int idRequisito, bool activo)
        {
            return Json(await _becaService.CambiarEstadoRequisito(idRequisito, activo));
        }

        // ---- Estudiante y encargado (MBEF-01-07) ----

        [HttpGet]
        [Authorize(Roles = RolesSistema.Estudiante + "," + RolesSistema.Encargado)]
        public IActionResult MiBeca()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RolesSistema.Estudiante + "," + RolesSistema.Encargado)]
        public async Task<IActionResult> GetMisBecas()
        {
            if (!TryObtenerIdUsuario(out int idUsuario))
                return RespuestaSesionInvalida();

            bool esEncargado = User.IsInRole(RolesSistema.Encargado) && !User.IsInRole(RolesSistema.Estudiante);

            return Json(await _becaService.GetMisBecas(idUsuario, esEncargado));
        }

        // ---- Auxiliares ----

        /// El id sale siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerIdUsuario(out int idUsuario)
        {
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idUsuario);
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
