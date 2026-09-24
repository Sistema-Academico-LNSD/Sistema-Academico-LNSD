using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Rol;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class RolController : Controller
    {
        private readonly IRolService _rolService;

        public RolController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [RequierePermiso("Roles", "Ver")]
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetRoles()
        {
            var respuesta =
                await _rolService.GetRoles();

            return Json(respuesta);
        }

        [RequierePermiso("Roles", "Ver")]
        public async Task<IActionResult> GetRolById(int id)
        {
            var respuesta =
                await _rolService.GetRolById(id);

            return Json(respuesta);
        }

        [RequierePermiso("Roles", "Crear")]
        public async Task<IActionResult> CreateRol(RolDTO rol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _rolService.CreateRol(rol);

            return Json(respuesta);
        }

        [RequierePermiso("Roles", "Editar")]
        public async Task<IActionResult> UpdateRol(RolDTO rol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _rolService.UpdateRol(rol);

            return Json(respuesta);
        }

        [RequierePermiso("Roles", "Eliminar")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var respuesta =
                await _rolService.DeleteRol(id);

            return Json(respuesta);
        }
    }
}