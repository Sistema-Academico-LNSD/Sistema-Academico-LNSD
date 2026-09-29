using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Service.Usuario;

namespace ProyectoLNSD_WApp.Controllers
{
    [Authorize]
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // GET: /Usuario/Index

        [HttpGet]
        [RequierePermiso("Usuarios", "Ver")]
        public IActionResult Index()
        {
            return View(new List<UsuarioDTO>());
        }

        [HttpGet]
        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> GetUsuarios()
        {
            var respuesta = await _usuarioService.GetUsuarios();

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> GetUsuarioById(int id)
        {
            var respuesta = await _usuarioService.GetUsuarioById(id);

            return Json(respuesta);
        }

        [HttpGet]
        [RequierePermiso("Usuarios", "Ver")]
        public async Task<IActionResult> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado)
        {
            var respuesta = await _usuarioService.BuscarUsuarios(nombre, correo, idRol, estado);

            return Json(respuesta);
        }

        // POST & antiforgery

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Usuarios", "Crear")]
        public async Task<IActionResult> CreateUsuario(UsuarioCrearDTO usuario)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _usuarioService.CreateUsuario(usuario);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Usuarios", "Editar")]
        public async Task<IActionResult> UpdateUsuario(UsuarioActualizarDTO usuario)
        {
            if (!ModelState.IsValid)
            {
                return RespuestaModeloInvalido();
            }

            var respuesta = await _usuarioService.UpdateUsuario(usuario);

            return Json(respuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("Usuarios", "Editar")]
        public async Task<IActionResult> CambiarEstado(int id, bool estado)
        {
            var respuesta = await _usuarioService.CambiarEstado(id, estado);

            return Json(respuesta);
        }
    }
}