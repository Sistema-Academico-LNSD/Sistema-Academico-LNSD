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
    }
}
