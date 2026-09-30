namespace ProyectoLNSD_WApp.DAL.Repositories.Curso
{
    public interface ICursoRepository
    {
        Task<List<Entities.Curso>> GetCursos(
            string? texto,
            int? idGrado,
            int? idArea,
            bool? estado);

        Task<List<Entities.AreaAcademica>> GetAreasActivas();

        Task<List<Entities.Grado>> GetGradosActivos();

        Task<Entities.Curso?> GetCursoById(int id);

        Task<Entities.Curso?> GetCursoDetalle(int id);

        Task<bool> CreateCurso(
            Entities.Curso curso,
            List<int> idsGrados);

        Task<bool> UpdateCurso(
            Entities.Curso curso,
            List<int> idsGrados);

        Task<bool> CambiarEstado(int id, bool estado);

        Task<bool> EliminarCurso(int id);

        Task<bool> ExisteCodigo(
            string codigo,
            int? excluirId = null);

        Task<bool> ExisteNombre(
            string nombre,
            int idArea,
            int? excluirId = null);
    }
}