using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    /// Consulta de calificaciones por rol. Cada método valida a quién pertenece lo que se consulta.
    public interface IConsultaCalificacionService
    {
        // MCNF-01-09 (estudiante): solo sus propias calificaciones
        Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetMisPeriodos(int idUsuario);
        Task<RespuestaDTO<DetalleEstudianteDTO>> GetMiDetalle(int idUsuario, int idPeriodo);

        // MCNF-01-10 (encargado): solo los estudiantes asociados a su cuenta
        Task<RespuestaDTO<List<EstudianteResumenDTO>>> GetMisEstudiantes(int idUsuario);
        Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetPeriodosDeMiEstudiante(int idUsuario, int idEstudiante);
        Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleDeMiEstudiante(int idUsuario, int idEstudiante, int idPeriodo);

        // MCNF-01-11 (administrador y director): por período, curso, sección, grupo o estudiante
        Task<RespuestaDTO<List<ConsultaFilaDTO>>> GetConsulta(
            int idPeriodo, int? idCurso, int? idSeccion, int? idGrupo, string? texto);

        Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleAdministrativo(int idEstudiante, int idPeriodo);

        // MCNF-01-15 (docente): detalle de un estudiante en el curso y grupo que imparte
        Task<RespuestaDTO<DetalleEstudianteDTO>> GetDetalleDocente(
            int idUsuario, int idEstudiante, int idPeriodo, int idCurso, int idSeccion, int idGrupo);
    }
}
