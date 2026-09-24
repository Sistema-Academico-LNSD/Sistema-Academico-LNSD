using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.PermisoActions;

namespace ProyectoLNSD_WApp.BLL.Service.Permiso
{
    public interface IPermisoService
    {
        Task<RespuestaDTO<List<ModuloDTO>>> GetModulos();
        Task<RespuestaDTO<List<ModuloPermisoDTO>>> GetMatrizPermisos(int idRol);

        Task<RespuestaDTO<bool>> GuardarPermisos(GuardarPermisosDTO dto);
        Task<List<string>> ObtenerPermisosComoClaims(int idRol);
    }
}