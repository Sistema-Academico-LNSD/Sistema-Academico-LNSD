namespace ProyectoLNSD_WApp.DAL.Repositories.Calificacion
{
    public interface ICalificacionRepository
    {
        // ---- Clases del docente (MCNF-01-02) ----
        // Horarios ACTIVOS del docente en el período, solo de cursos que tiene asignados en Docente_Curso.
        Task<List<Entities.Horario>> GetClasesActivasDeDocente(int idDocente, int idPeriodo);

        Task<bool> DocenteImparteClase(int idDocente, int idPeriodo, int idCurso, int idSeccion, int idGrupo);

        // ---- Planilla (MCNF-01-01) ----
        Task<List<Entities.Matricula>> GetMatriculasDeClase(int idPeriodo, int idCurso, int idSeccion, int idGrupo);

        Task<List<Entities.RubroEvaluacion>> GetRubros();

        Task<List<Entities.ActividadEvaluacion>> GetActividadesDeClase(int idPeriodo, int idCurso, int idSeccion, int idGrupo);

        Task<List<Entities.Nota>> GetNotasDeActividades(List<int> idsActividad);

        // ---- Actividades (MCNF-01-03) ----
        Task<Entities.RubroEvaluacion?> GetRubro(int idRubro);

        Task<bool> ExisteActividadConNombre(int idPeriodo, int idCurso, int idSeccion, int idGrupo, int idRubro, string nombre);

        Task<Entities.ActividadEvaluacion> CrearActividad(Entities.ActividadEvaluacion actividad);

        Task<Entities.ActividadEvaluacion?> GetActividad(int idActividad);

        // ---- Notas (MCNF-01-03) ----
        Task<Entities.Matricula?> GetMatricula(int idMatricula);

        Task<Entities.Nota?> GetNota(int idMatricula, int idActividad);

        Task<bool> CrearNota(Entities.Nota nota, Entities.NotaHistorial historial);

        // ---- Editar y corregir notas (MCNF-01-06, 01-07, 01-08, 01-13) ----
        // Con seguimiento de cambios: el servicio la modifica y se guarda junto con su historial
        Task<Entities.Nota?> GetNotaConActividad(int idNota);

        // Guarda los cambios pendientes (nota y autorización usada) junto con el nuevo registro de historial
        Task<bool> GuardarCambioNota(Entities.NotaHistorial historial);

        Task<List<Entities.NotaHistorial>> GetHistorialDeNotas(List<int> idsNota);

        Task<Dictionary<int, string>> GetNombresUsuarios(List<int> idsUsuario);

        // Autorización sin usar de una nota (con seguimiento de cambios, para marcarla como usada)
        Task<Entities.CorreccionAutorizada?> GetCorreccionPendiente(int idNota);

        Task<List<Entities.CorreccionAutorizada>> GetCorreccionesDeNotas(List<int> idsNota);

        Task<bool> CrearCorreccion(Entities.CorreccionAutorizada correccion);

        // Horarios del período cuya clase (curso, sección y grupo) ya tiene actividades
        Task<List<Entities.Horario>> GetClasesConActividades(int idPeriodo);

        // ---- Promedio final, asistencia y resultado (MCNF-01-04, 01-05 y 01-14) ----
        Task<Entities.ConfiguracionCalificacion?> GetConfiguracion();

        // Asistencia de la clase dentro de las fechas del período
        Task<List<Entities.Asistencia>> GetAsistenciasDeClase(
            int idPeriodo, int idCurso, int idSeccion, int idGrupo, DateOnly desde, DateOnly hasta);

        // Matrícula activa del estudiante en el período que incluye el curso (con el nombre del curso)
        Task<Entities.Matricula?> GetMatriculaDeEstudiante(int idEstudiante, int idPeriodo, int idCurso);

        // Matrículas activas del estudiante en esos períodos, con sus cursos
        Task<List<Entities.Matricula>> GetMatriculasDeEstudiante(int idEstudiante, List<int> idsPeriodo);
    }
}
