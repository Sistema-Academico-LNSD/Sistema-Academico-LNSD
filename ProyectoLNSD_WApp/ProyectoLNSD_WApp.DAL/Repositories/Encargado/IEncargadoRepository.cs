namespace ProyectoLNSD_WApp.DAL.Repositories.Encargado
{
    public interface IEncargadoRepository
    {
        Task<List<Entities.EncargadoEstudiante>> GetPorEstudiante(int idEstudiante);

        /// <summary>Encargados activos que aún no están vinculados a ese estudiante.</summary>
        Task<List<Entities.Encargado>> Buscar(string? texto, int idEstudiante);

        Task<Entities.Encargado?> GetById(int idEncargado);

        /// <summary>Estudiantes asociados a la cuenta de un encargado (MESF-01-12).</summary>
        Task<List<Entities.EncargadoEstudiante>> GetPorUsuario(int idUsuario);
        Task<Entities.EncargadoEstudiante?> GetVinculo(int idEncargado, int idEstudiante);
        Task<int> ContarVinculos(int idEstudiante);

        /// <summary>Vincula un encargado existente. Si es el primero del estudiante, queda como principal.</summary>
        Task<bool> Vincular(int idEncargado, int idEstudiante, string parentesco);

        /// <summary>Crea Usuario + Encargado + vínculo en un solo guardado.</summary>
        Task<bool> CrearYVincular(Entities.Usuario usuario, string? telefono, string parentesco, int idEstudiante);

        /// <summary>Deja un único encargado principal por estudiante (MESF-01-04).</summary>
        Task<bool> DefinirPrincipal(int idEstudiante, int idEncargado);

        Task<bool> Desvincular(int idEncargado, int idEstudiante);
    }
}
