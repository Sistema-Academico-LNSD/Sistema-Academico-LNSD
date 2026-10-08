namespace ProyectoLNSD_WApp.DAL.Repositories.DocenteCurso
{
    public interface IDocenteCursoRepository
    {
        Task<List<Entities.DocenteCurso>> GetAsignaciones(int idDocente);
        Task<Entities.DocenteCurso?> GetById(int idDocenteCurso);
        Task<bool> Existe(int idDocente, int idCurso, int idPeriodo);
        Task<List<Entities.Curso>> GetCursosDisponibles(int idDocente, int idPeriodo);
        Task<bool> Crear(Entities.DocenteCurso asignacion);
        Task<bool> Eliminar(int idDocenteCurso);
    }
}