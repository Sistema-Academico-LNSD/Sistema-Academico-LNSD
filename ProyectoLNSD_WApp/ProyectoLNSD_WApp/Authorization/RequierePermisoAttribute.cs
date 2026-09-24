using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProyectoLNSD_WApp.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequierePermisoAttribute : Attribute, IAuthorizationFilter
    {
        public const string TipoClaim = "Permiso";

        private readonly string _modulo;
        private readonly string _accion;

        public RequierePermisoAttribute(string modulo, string accion)
        {
            _modulo = modulo;
            _accion = accion;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var usuario = context.HttpContext.User;

            if (usuario.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();

                return;
            }

            string permisoRequerido = $"{_modulo}:{_accion}";

            bool tienePermiso = usuario.Claims.Any(c =>
                c.Type == TipoClaim &&
                c.Value == permisoRequerido);

            if (!tienePermiso)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}