using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Rol;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class RolController : BaseController
    {
        private readonly IRolService _rolService;

        public RolController(IRolService rolService)
        {
            _rolService = rolService;
        }

        // GET: /Rol/Index

        [HttpGet]
        [RequierePermiso("Roles", "Ver")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var respuesta = await _rolService.GetRoles();

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Roles", "Ver")]
        public async Task<IActionResult> GetRolById(int id)
        {
            var respuesta = await _rolService.GetRolById(id);

            return Json(respuesta);
        }

        // POST & antiforgery

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Roles", "Crear")]
        public async Task<IActionResult> CreateRol(RolDTO rol)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _rolService.CreateRol(rol);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Roles", "Editar")]
        public async Task<IActionResult> UpdateRol(RolDTO rol)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _rolService.UpdateRol(rol);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Roles", "Eliminar")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var respuesta = await _rolService.DeleteRol(id);

            return Json(respuesta);
        }
    }
}