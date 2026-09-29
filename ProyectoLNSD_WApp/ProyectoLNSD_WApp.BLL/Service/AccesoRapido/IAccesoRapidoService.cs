using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.AccesoRapido
{
    public interface IAccesoRapidoService
    {
        // Administración: todos los accesos (activos e inactivos), con sus roles
        Task<RespuestaDTO<List<AccesoRapidoDTO>>> GetAccesos();
        Task<RespuestaDTO<AccesoRapidoDTO?>> GetAcceso(int idAcceso);
        Task<RespuestaDTO<List<RolOpcionDTO>>> GetRoles();
        Task<RespuestaDTO<AccesoRapidoDTO>> GuardarAcceso(AccesoRapidoDTO acceso);
        Task<RespuestaDTO<bool>> EliminarAcceso(int idAcceso);

        // Home: solo los activos que corresponden a los roles del usuario (HU escenario 5)
        Task<RespuestaDTO<List<AccesoRapidoDTO>>> GetActivosParaRoles(IEnumerable<string> roles);
    }
}
