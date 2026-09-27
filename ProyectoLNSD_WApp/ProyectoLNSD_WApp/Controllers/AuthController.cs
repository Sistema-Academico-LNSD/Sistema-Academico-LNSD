using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoLNSD_WApp.Authorization;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;
using ProyectoLNSD_WApp.BLL.Service.Auditoria;
using ProyectoLNSD_WApp.BLL.Service.Auth;
using ProyectoLNSD_WApp.BLL.Service.Permiso;

namespace ProyectoLNSD_WApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IPermisoService _permisoService;
        private readonly IAuditoriaService _auditoriaService;

        public AuthController(
            IAuthService authService,
            IPermisoService permisoService,
            IAuditoriaService auditoriaService)
        {
            _authService = authService;
            _permisoService = permisoService;
            _auditoriaService = auditoriaService;
        }

        // ---------- Login / Logout ----------

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
                return RespuestaModeloInvalido();
            }

            var respuesta = await _authService.Login(login);

            if (!respuesta.EsCorrecto || respuesta.Dato == null)
            {
                await _auditoriaService.RegistrarLogin(
                    idUsuario: null,
                    correo: login.Correo,
                    exitoso: false,
                    mensaje: respuesta.Mensaje);

                return Json(new
                {
                    esCorrecto = false,
                    mensaje = respuesta.Mensaje
                });
            }

            var usuario = respuesta.Dato;

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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
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
                mensaje = respuesta.Mensaje,
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

        // ---------- Recuperación de contraseña ----------

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
                return RespuestaModeloInvalido();
            }

            string urlBaseRestablecer =
                Url.Action("RestablecerPassword", "Auth", null, Request.Scheme) ?? string.Empty;

            var respuesta = await _authService.SolicitarRecuperacionPassword(dto, urlBaseRestablecer);

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
                return RespuestaModeloInvalido();
            }

            var respuesta = await _authService.RestablecerPassword(dto);

            return Json(new
            {
                esCorrecto = respuesta.EsCorrecto,
                mensaje = respuesta.Mensaje
            });
        }
    }
}