using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public interface IDocenteCuentaService
    {
        Task<RespuestaDTO<CuentaDocenteDTO>> GetCuenta(int idDocente);
        Task<RespuestaDTO<List<UsuarioDTO>>> GetUsuariosDisponibles();
        Task<RespuestaDTO<bool>> VincularCuentaExistente(int idDocente, int idUsuario);
        Task<RespuestaDTO<bool>> CrearCuenta(CrearCuentaDocenteDTO dto);
        Task<RespuestaDTO<bool>> Desvincular(int idDocente);
    }
}