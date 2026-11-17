using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Inventario;
using System.Security.Claims;

namespace ProyectoLNSD_WApp.Controllers
{
    /// Inventario de bodega (MIVF-01).
    [Authorize]
    public class InventarioController : BaseController
    {
        private const string Modulo = "Inventario";

        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        // ---- Artículos (MIVF-01-01 y 01-04 a 01-08) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetArticulos(InventarioFiltroDTO filtro)
        {
            return Json(await _inventarioService.BuscarArticulos(filtro));
        }

        /// Registrar exige "Crear"; editar un artículo existente exige "Editar".
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GuardarArticulo([FromBody] ArticuloInventarioGuardarDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            string accion = dto.IdInventario > 0 ? "Editar" : "Crear";

            if (!User.TienePermiso(Modulo, accion))
                return Json(RespuestaDTO<ArticuloInventarioDTO>.Error("No tiene permiso para realizar esta acción", 403));

            if (!TryObtenerIdUsuario(out int idUsuario))
                return Json(RespuestaDTO<ArticuloInventarioDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _inventarioService.GuardarArticulo(idUsuario, dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> CambiarEstadoArticulo(int idInventario, bool activo)
        {
            return Json(await _inventarioService.CambiarEstadoArticulo(idInventario, activo));
        }


        // ---- Entradas y salidas (MIVF-01-09, 01-10 y 01-11) ----

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Crear")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] MovimientoInventarioGuardarDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            if (!TryObtenerIdUsuario(out int idUsuario))
                return Json(RespuestaDTO<MovimientoInventarioDTO>.Error("La sesión no es válida; inicie sesión de nuevo", 401));

            return Json(await _inventarioService.RegistrarMovimiento(idUsuario, dto));
        }


        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetHistorial(int idInventario)
        {
            return Json(await _inventarioService.GetHistorial(idInventario));
        }

        // ---- Tipos de inventario (MIVF-01-02 y 01-03) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Categorias()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetCategorias(bool incluirInactivas = false)
        {
            return Json(await _inventarioService.GetCategorias(incluirInactivas));
        }

        /// Crear exige "Crear"; editar el nombre exige "Editar".
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GuardarCategoria([FromBody] CategoriaInventarioGuardarDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return RespuestaModeloInvalido();

            string accion = dto.IdCategoria > 0 ? "Editar" : "Crear";

            if (!User.TienePermiso(Modulo, accion))
                return Json(RespuestaDTO<CategoriaInventarioDTO>.Error("No tiene permiso para realizar esta acción", 403));

            return Json(await _inventarioService.GuardarCategoria(dto));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso(Modulo, "Editar")]
        public async Task<IActionResult> CambiarEstadoCategoria(int idCategoria, bool activo)
        {
            return Json(await _inventarioService.CambiarEstadoCategoria(idCategoria, activo));
        }


        // ---- Reportes y estadísticas (MIVF-01-16, 01-17 y 01-18) ----

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public IActionResult Reportes()
        {
            return View();
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetReporteExistencias(ReporteExistenciasFiltroDTO filtro)
        {
            return Json(await _inventarioService.GetReporteExistencias(filtro));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> ExportarExistencias(ReporteExistenciasFiltroDTO filtro)
        {
            var respuesta = await _inventarioService.ExportarExistencias(filtro);

            // Si no hay registros o falla, sale como JSON para mostrarlo como mensaje
            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return Json(respuesta);

            return File(
                respuesta.Dato,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_Existencias_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetReporteMovimientos(ReporteMovimientosFiltroDTO filtro)
        {
            return Json(await _inventarioService.GetReporteMovimientos(filtro));
        }

        [HttpGet]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> ExportarMovimientos(ReporteMovimientosFiltroDTO filtro)
        {
            var respuesta = await _inventarioService.ExportarMovimientos(filtro);

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
                return Json(respuesta);

            return File(
                respuesta.Dato,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Inventario_Movimientos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        /// Estadísticas solo para administración y dirección (MIVF-01-18).
        [HttpGet]
        [Authorize(Roles = "Administrador,Director")]
        [RequierePermiso(Modulo, "Ver")]
        public async Task<IActionResult> GetEstadisticas(ReporteMovimientosFiltroDTO filtro)
        {
            return Json(await _inventarioService.GetEstadisticas(filtro));
        }

        // ---- Auxiliares ----

        /// El id sale siempre de la sesión (claims), nunca de lo que envía el navegador.
        private bool TryObtenerIdUsuario(out int idUsuario)
        {
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idUsuario);
        }
    }
}