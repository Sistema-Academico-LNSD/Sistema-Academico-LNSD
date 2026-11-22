using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Notificaciones;
using ProyectoLNSD_WApp.BLL.Service.Pauta;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    /// Pautas de actuación del MEP (MPPAF-01).
    /// La persona y el rol salen siempre de la sesión (claims); el servicio decide qué expedientes puede ver o cambiar.
    [Authorize]
    public class PautaController : BaseController
    {
        private const string Modulo = "Pautas";

        private readonly IPautaService _pautaService;
        private readonly INotificacionService _notificacionService;

        public PautaController(IPautaService pautaService, INotificacionService notificacionService)
        {
            _pautaService = pautaService;
            _notificacionService = notificacionService;
        }

        // ---- Pantallas ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Crear")]
        public IActionResult Nueva(int? origen)
        {
            ViewBag.IdExpedienteOrigen = origen;
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Plantillas()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Expediente(int id)
        {
            ViewBag.IdExpediente = id;
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Formulario(int id)
        {
            ViewBag.IdFormulario = id;
            return View();
        }

        // ---- Catálogo (MPPAF-01-01 y 01-02) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult GetPautas()
        {
            return Json(_pautaService.GetPautas());
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult GetPlantillas()
        {
            return Json(_pautaService.GetPlantillas());
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> BuscarEstudiantes(string? texto)
        {
            return Json(await _pautaService.BuscarEstudiantes(texto));
        }

        // ---- Expedientes (MPPAF-01-02, 01-04, 01-10, 01-14) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> CrearExpediente([FromBody] ExpedienteCrearDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<ExpedienteDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            var resultado = await _pautaService.CrearExpediente(idUsuario, rol, dto);
            if (resultado.EsCorrecto && resultado.Dato != null)
                await _notificacionService.NotificarPauta(resultado.Dato.IdExpediente, "ABIERTA", idUsuario);
            return Json(resultado);
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetExpedientes(FiltroExpedientesDTO filtro)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<List<ExpedienteResumenDTO>>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.GetExpedientes(idUsuario, rol, filtro ?? new FiltroExpedientesDTO()));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetExpediente(int idExpediente)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<ExpedienteDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.GetExpediente(idUsuario, rol, idExpediente));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> ActualizarEstudiantes(int idExpediente, [FromBody] List<EstudianteInvolucradoDTO> estudiantes)
        {
            if (estudiantes == null)
                return RespuestaModeloInvalido();

            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<ExpedienteDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.ActualizarEstudiantes(idUsuario, rol, idExpediente, estudiantes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> CerrarExpediente([FromBody] CierreExpedienteDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<ExpedienteDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            var resultado = await _pautaService.CerrarExpediente(idUsuario, rol, dto);
            if (resultado.EsCorrecto)
                await _notificacionService.NotificarPauta(dto.IdExpediente, "CERRADA", idUsuario);
            return Json(resultado);
        }

        // ---- Formularios (MPPAF-01-03, 01-05 a 01-07, 01-11, 01-13) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> AgregarFormulario([FromBody] AgregarFormularioDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<FormularioDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.AgregarFormulario(idUsuario, rol, dto));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetFormulario(int idFormulario)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<FormularioDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.GetFormulario(idUsuario, rol, idFormulario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> GuardarFormulario([FromBody] FormularioGuardarDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<FormularioDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            var resultado = await _pautaService.GuardarFormulario(idUsuario, rol, dto);
            if (dto.Finalizar && resultado.EsCorrecto && resultado.Dato != null && resultado.Dato.Estado == "Finalizado")
                await _notificacionService.NotificarPauta(resultado.Dato.IdExpediente, "FORMULARIO_FINALIZADO", idUsuario, resultado.Dato.IdFormulario);
            return Json(resultado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> ReabrirFormulario(int idFormulario, string? motivo)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<FormularioDetalleDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            var resultado = await _pautaService.ReabrirFormulario(idUsuario, rol, idFormulario, motivo);
            if (resultado.EsCorrecto && resultado.Dato != null)
                await _notificacionService.NotificarPauta(resultado.Dato.IdExpediente, "FORMULARIO_REABIERTO", idUsuario, idFormulario);
            return Json(resultado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> EliminarFormulario(int idFormulario)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<bool>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.EliminarFormularioBorrador(idUsuario, rol, idFormulario));
        }

        // ---- Bitácora (el servicio la limita a Administrador y Director) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetBitacora(int? idExpediente, int? idUsuarioFiltro, string? accion, DateTime? desde, DateTime? hasta)
        {
            if (!TryObtenerSesion(out int idUsuario, out string rol))
                return Json(RespuestaDTO<List<BitacoraPautaDTO>>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _pautaService.GetBitacora(idUsuario, rol, idExpediente, idUsuarioFiltro, accion, desde, hasta));
        }

        // ---- Auxiliares ----

        /// El id y el rol salen siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerSesion(out int idUsuario, out string rol)
        {
            rol = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idUsuario)
                   && rol.Length > 0;
        }
    }
}
