using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    public interface IResultadoCalificacionService
    {
        // Nota mínima de aprobación y reglas de la asistencia
        Task<ConfiguracionCalificacionDTO> GetConfiguracion();

        // MCNF-01-04, 01-05 y 01-14: promedio del semestre (con asistencia), nota del año y si aprueba o reprueba el curso.
        // El año son los períodos con el mismo año de inicio que idPeriodo.
        Task<ResultadoCursoDTO> GetResultadoCurso(int idEstudiante, int idCurso, int idPeriodo);

        // Resultado de todos los cursos del estudiante en el año y su promoción
        Task<RespuestaDTO<PromocionDTO>> GetPromocion(int idEstudiante, int idPeriodo);
    }
}
