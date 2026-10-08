using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public interface IExpedienteDocenteService
    {
        Task<RespuestaDTO<ExpedienteDocenteDTO>> GetPorDocente(int idDocente);
        Task<RespuestaDTO<ExpedienteDocenteDTO>> GetPorUsuario(int idUsuario);
    }
}