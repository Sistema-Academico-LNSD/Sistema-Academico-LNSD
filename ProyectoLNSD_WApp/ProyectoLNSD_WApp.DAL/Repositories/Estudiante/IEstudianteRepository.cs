namespace ProyectoLNSD_WApp.DAL.Repositories.Estudiante
{
    public interface IEstudianteRepository
    {
        Task<List<Entities.Estudiante>> Buscar(string? texto, int? idGrado, bool? estado);
        Task<Entities.Estudiante?> GetById(int idEstudiante);

        /// <summary>Expediente del estudiante dueño de la cuenta (MESF-01-11).</summary>
        Task<Entities.Estudiante?> GetByUsuario(int idUsuario);

        Task<bool> ExisteIdentificacion(string identificacion, int? excluirIdEstudiante = null);

        /// <summary>
        /// Crea la cuenta (Usuario) y el expediente (Estudiante) en una sola transacción,
        /// generando el carné único con formato LNSD-AAAA-NNNN (MESF-01-10).
        /// </summary>
        Task<string?> CrearConUsuario(Entities.Estudiante estudiante, Entities.Usuario usuario);

        /// <summary>
        /// Actualiza el expediente y los datos reales del usuario (nombre, apellidos, correo)
        /// en un solo guardado. El carné no se modifica (MESF-01-06).
        /// </summary>
        Task<bool> Actualizar(Entities.Estudiante datos, string nombre, string apellido, string correo);

        Task<bool> CambiarEstado(int idEstudiante, bool estado);
    }
}
