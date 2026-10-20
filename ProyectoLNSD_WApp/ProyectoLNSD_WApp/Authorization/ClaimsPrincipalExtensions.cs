using System.Security.Claims;

namespace ProyectoLNSD_WApp.Authorization
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TienePermiso(this ClaimsPrincipal usuario, string modulo, string accion)
        {
            // El rol Administrador es el rol raíz del sistema y puede consultar
            // todos los módulos/acciones sin depender de la matriz de permisos.
            if (usuario.Identity?.IsAuthenticated == true && usuario.IsInRole("Administrador"))
            {
                return true;
            }

            string permisoRequerido = $"{modulo}:{accion}";

            return usuario.Claims.Any(c =>
                c.Type == RequierePermisoAttribute.TipoClaim &&
                string.Equals(c.Value, permisoRequerido, StringComparison.OrdinalIgnoreCase));
        }
    }
}
