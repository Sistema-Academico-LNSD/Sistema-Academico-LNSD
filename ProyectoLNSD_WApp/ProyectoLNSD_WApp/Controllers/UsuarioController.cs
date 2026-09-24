using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Service.Usuario;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [RequierePermiso("Usuarios", "Ver")]
        public IActionResult Index()
        {
            return View(new List<UsuarioDTO>());
        }

        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> GetUsuarios()
        {
            var respuesta = await _usuarioService.GetUsuarios();

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> GetUsuarioById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _usuarioService.GetUsuarioById(id);

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado)
        {
            var respuesta =
                await _usuarioService.BuscarUsuarios(
                    nombre,
                    correo,
                    idRol,
                    estado);

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Crear")]
        public async Task<IActionResult> CreateUsuario(
            UsuarioCrearDTO usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _usuarioService.CreateUsuario(usuario);

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Editar")]
        public async Task<IActionResult> UpdateUsuario(
            UsuarioActualizarDTO usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _usuarioService.UpdateUsuario(usuario);

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Eliminar")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta =
                await _usuarioService.DeleteUsuario(id);

            return Json(respuesta);
        }

        [RequierePermiso("Usuarios", "Editar")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            bool estado)
        {
            var respuesta =
                await _usuarioService.CambiarEstado(id, estado);

            return Json(respuesta);
        }
    }
}