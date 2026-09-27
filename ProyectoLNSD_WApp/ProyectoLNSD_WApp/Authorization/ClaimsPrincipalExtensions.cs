using System.Security.Claims;

namespace ProyectoLNSD_WApp.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TienePermiso(this ClaimsPrincipal usuario, string modulo, string accion)
        {
            string permisoRequerido = $"{modulo}:{accion}";

            return usuario.Claims.Any(c =>
                c.Type == RequierePermisoAttribute.TipoClaim &&
                c.Value == permisoRequerido);
        }
    }
}