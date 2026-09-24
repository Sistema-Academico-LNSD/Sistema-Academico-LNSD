using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Service.Auditoria;
using ProyectoLNSD_WApp.BLL.Service.Permiso;
using ProyectoLNSD_WApp.BLL.Service.Usuario;

namespace ProyectoLNSD_WApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IPermisoService _permisoService;
        private readonly IAuditoriaService _auditoriaService;

        public AuthController(
            IUsuarioService usuarioService,
            IPermisoService permisoService,
            IAuditoriaService auditoriaService)
        {
            _usuarioService = usuarioService;
            _permisoService = permisoService;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO login, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var respuesta = await _usuarioService.Login(login);

            if (!respuesta.EsCorrecto || respuesta.Dato is not { Autenticado: true, Usuario: not null })
            {
                await _auditoriaService.RegistrarLogin(
                    idUsuario: respuesta.Dato?.Usuario?.IdUsuario,
                    correo: login.Correo,
                    exitoso: false,
                    mensaje: respuesta.Dato?.Mensaje ?? respuesta.Mensaje);

                return Json(new
                {
                    esCorrecto = false,
                    mensaje = respuesta.Dato?.Mensaje ?? respuesta.Mensaje
                });
            }

            var usuario = respuesta.Dato.Usuario;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new(ClaimTypes.Email, usuario.Correo),
                new("IdRol", usuario.IdRol.ToString())
            };

            if (usuario.Rol != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            }

            var permisos = await _permisoService.ObtenerPermisosComoClaims(usuario.IdRol);

            foreach (var permiso in permisos)
            {
                claims.Add(new Claim(RequierePermisoAttribute.TipoClaim, permiso));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            await _auditoriaService.RegistrarLogin(
                idUsuario: usuario.IdUsuario,
                correo: usuario.Correo,
                exitoso: true,
                mensaje: null);

            return Json(new
            {
                esCorrecto = true,
                mensaje = "Inicio de sesión exitoso",
                redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var correoClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (int.TryParse(idUsuarioClaim, out int idUsuario) && !string.IsNullOrWhiteSpace(correoClaim))
            {
                await _auditoriaService.RegistrarLogout(idUsuario, correoClaim);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(SolicitarRecuperacionDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string urlBaseRestablecer =
                Url.Action("RestablecerPassword", "Auth", null, Request.Scheme) ?? string.Empty;

            var respuesta = await _usuarioService.SolicitarRecuperacionPassword(dto, urlBaseRestablecer);

            return Json(new
            {
                esCorrecto = respuesta.EsCorrecto,
                mensaje = respuesta.Mensaje
            });
        }

        [HttpGet]
        public IActionResult RestablecerPassword(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["Token"] = token;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var primerError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return Json(new
                {
                    esCorrecto = false,
                    mensaje = primerError ?? "Datos inválidos."
                });
            }

            var respuesta = await _usuarioService.RestablecerPassword(dto);

            return Json(new
            {
                esCorrecto = respuesta.EsCorrecto,
                mensaje = respuesta.Mensaje
            });
        }
    }
}