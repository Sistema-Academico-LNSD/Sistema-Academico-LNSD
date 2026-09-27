using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;

namespace ProyectoLNSD_WApp.BLL.Service.Auth
{
    public interface IAuthService
    {
        Task<RespuestaDTO<ResultadoLoginDTO>> Login(LoginDTO login);

        Task<RespuestaDTO<bool>> SolicitarRecuperacionPassword(
            SolicitarRecuperacionDTO dto,
            string urlBaseRestablecer);

        Task<RespuestaDTO<bool>> RestablecerPassword(RestablecerPasswordDTO dto);
    }
}