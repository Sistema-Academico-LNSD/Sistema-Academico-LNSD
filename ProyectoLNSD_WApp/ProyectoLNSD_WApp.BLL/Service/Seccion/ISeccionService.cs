using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Seccion
{
    public interface ISeccionService
    {
        Task<RespuestaDTO<SeccionDTO?>> GetSeccionById(int id);
        Task<RespuestaDTO<SeccionDTO>> CreateSeccion(SeccionDTO seccion);
        Task<RespuestaDTO<SeccionDTO>> UpdateSeccion(SeccionDTO seccion);
        Task<RespuestaDTO<SeccionDTO>> CambiarEstadoSeccion(int id, bool estado);
    }
}
