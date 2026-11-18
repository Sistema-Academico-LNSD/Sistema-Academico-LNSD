using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Notificaciones;

public interface INotificacionService
{
    Task<NotificacionPanelDTO> ObtenerPanel(int idUsuario);
    Task<bool> MarcarLeida(int idUsuario, long idNotificacion);
    Task<List<NotificacionTipoDTO>> ObtenerTipos();
    Task<bool> GuardarTipo(NotificacionTipoGuardarDTO dto);
    Task<bool> CambiarEstado(int idTipo, bool activo);
    Task<int> NotificarUsuarios(string codigo, string claveEvento, string titulo, string mensaje, string? referencia, IEnumerable<int> idsUsuarios);
    Task NotificarCalificacion(int idNota, bool actualizada);
    Task NotificarAsistencia(int idHorario, DateOnly fecha);
    Task NotificarHorario(int idHorario, string evento, int? idDocenteAnterior = null, HorarioDTO? horarioAnterior = null);
    Task NotificarInformativo(int idInformativo, string evento, string? claveCambio = null);
    Task NotificarBeca(int idBeca, string evento);
}
