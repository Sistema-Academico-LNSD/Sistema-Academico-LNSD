using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.PermisoActions;

namespace ProyectoLNSD_WApp.BLL.Service.Permiso
{
    public interface IPermisoService
    {
        /// <summary>
        /// Devuelve, para el rol indicado, una fila por cada módulo existente
        /// (con sus 4 flags en false si el rol no tiene permisos asignados
        /// todavía para ese módulo). Pensado para alimentar la matriz de checkboxes.
        /// </summary>
        Task<RespuestaDTO<List<ModuloPermisoDTO>>> GetMatrizPermisos(int idRol);

        Task<RespuestaDTO<bool>> GuardarPermisos(GuardarPermisosDTO dto);

        /// <summary>
        /// Aplana los permisos de un rol en strings "Modulo:Accion", listos
        /// para agregarse como claims a la cookie de sesión en el login.
        /// </summary>
        Task<List<string>> ObtenerPermisosComoClaims(int idRol);
    }
}