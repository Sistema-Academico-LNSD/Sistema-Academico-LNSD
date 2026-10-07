using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Docente
{
    public interface IDocenteCursoService
    {
        Task<RespuestaDTO<CursosDocenteDTO>> GetCursosDocente(int idDocente);
        Task<RespuestaDTO<List<CursoDisponibleDTO>>> GetCursosDisponibles(int idDocente);
        Task<RespuestaDTO<bool>> Asignar(int idDocente, int idCurso);
        Task<RespuestaDTO<bool>> Quitar(int idDocenteCurso);
    }
}