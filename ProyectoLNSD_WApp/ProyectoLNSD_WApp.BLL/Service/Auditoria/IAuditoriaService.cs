using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Auditoria
{
    public interface IAuditoriaService
    {
        Task RegistrarLogin(int? idUsuario, string correo, bool exitoso, string? mensaje);

        Task RegistrarLogout(int idUsuario, string correo);

        Task<RespuestaDTO<List<LogAccesoDTO>>> GetHistorial(
            string? correo,
            DateTime? desde,
            DateTime? hasta,
            bool? soloFallidos);
    }
}