using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.DTO.UsuarioActions;

namespace ProyectoLNSD_WApp.BLL.Service.Usuario
{
    public interface IUsuarioService
    {
        Task<RespuestaDTO<List<UsuarioDTO>>> GetUsuarios();
        Task<RespuestaDTO<UsuarioDTO?>> GetUsuarioById(int id);
        Task<RespuestaDTO<UsuarioDTO?>> GetUsuarioByCorreo(string correo);
        Task<RespuestaDTO<List<UsuarioDTO>>> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado);
        Task<RespuestaDTO<UsuarioDTO>> CreateUsuario(
            UsuarioCrearDTO usuario);
        Task<RespuestaDTO<UsuarioDTO>> UpdateUsuario(
            UsuarioActualizarDTO usuario);
        Task<RespuestaDTO<UsuarioDTO>> DeleteUsuario(int id);
        Task<RespuestaDTO<UsuarioDTO>> CambiarEstado(
            int id,
            bool estado);
        Task<RespuestaDTO<ResultadoLoginDTO>> Login(LoginDTO login);
        Task<RespuestaDTO<bool>> SolicitarRecuperacionPassword(
            SolicitarRecuperacionDTO dto,
            string urlBaseRestablecer);
        Task<RespuestaDTO<bool>> RestablecerPassword(RestablecerPasswordDTO dto);
    }
}