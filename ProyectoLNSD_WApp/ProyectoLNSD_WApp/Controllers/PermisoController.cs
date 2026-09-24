using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.BLL.DTO.PermisoActions;
using ProyectoLNSD_WApp.BLL.Service.Permiso;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PermisoController : Controller
    {
        private readonly IPermisoService _permisoService;

        public PermisoController(IPermisoService permisoService)
        {
            _permisoService = permisoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Matriz(int idRol, string? nombreRol)
        {
            ViewData["IdRol"] = idRol;
            ViewData["NombreRol"] = nombreRol;

            return View();
        }

        public async Task<IActionResult> GetMatriz(int idRol)
        {
            var respuesta = await _permisoService.GetMatrizPermisos(idRol);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPermisos([FromBody] GuardarPermisosDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta = await _permisoService.GuardarPermisos(dto);

            return Json(respuesta);
        }
    }
}