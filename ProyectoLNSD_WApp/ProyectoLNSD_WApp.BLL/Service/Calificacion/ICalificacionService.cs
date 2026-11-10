using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Calificacion
{
    public interface ICalificacionService
    {
        Task<RespuestaDTO<List<PeriodoCalificacionDTO>>> GetPeriodos();

        // MCNF-01-02: solo los cursos y grupos asignados al docente de la sesión
        Task<RespuestaDTO<List<ClaseDocenteDTO>>> GetMisClases(int idUsuario, int? idPeriodo);

        // MCNF-01-01: estudiantes de un curso/sección/grupo con sus actividades y notas
        Task<RespuestaDTO<PlanillaDTO>> GetPlanilla(int idUsuario, int idPeriodo, int idCurso, int idSeccion, int idGrupo);

        // MCNF-01-03: el docente crea las prácticas, exámenes y tareas de su curso
        Task<RespuestaDTO<ActividadDTO>> CrearActividad(int idUsuario, ActividadCrearDTO dto);

        // MCNF-01-03 y 01-12: registrar la nota de un estudiante en una actividad
        Task<RespuestaDTO<NotaCeldaDTO>> GuardarNota(int idUsuario, NotaGuardarDTO dto);

        // Guarda de una vez todas las notas que el docente digitó y revisó
        Task<RespuestaDTO<List<NotaLoteResultadoDTO>>> GuardarNotas(int idUsuario, List<NotaGuardarDTO> notas);
    }
}
