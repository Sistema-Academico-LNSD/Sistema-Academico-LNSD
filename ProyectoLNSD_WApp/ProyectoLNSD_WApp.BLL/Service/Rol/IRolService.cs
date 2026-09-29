using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Rol
{
    public interface IRolService
    {
        Task<RespuestaDTO<List<RolDTO>>> GetRoles();

        Task<RespuestaDTO<RolDTO?>> GetRolById(int id);

        Task<RespuestaDTO<RolDTO>> CreateRol(RolDTO rol);

        Task<RespuestaDTO<RolDTO>> UpdateRol(RolDTO rol);

        Task<RespuestaDTO<RolDTO>> DeleteRol(int id);
    }
}