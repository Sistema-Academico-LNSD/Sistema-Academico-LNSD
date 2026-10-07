namespace ProyectoLNSD_WApp.DAL.Repositories.Docente
{
    public interface IDocenteRepository
    {
        Task<List<Entities.Docente>> Buscar(string? texto, int? idArea, int? idCurso, bool? estado);
        Task<Entities.Docente?> GetById(int idDocente);
        Task<Entities.Docente?> GetByIdUsuario(int idUsuario);
        Task<bool> ExisteIdentificacion(string identificacion, int? excluirIdDocente = null);
        Task<bool> ExisteCorreo(string correo, int? excluirIdDocente = null);
        Task<bool> Crear(Entities.Docente docente);
        Task<bool> Actualizar(Entities.Docente docente);
        Task<bool> CambiarEstado(int idDocente, bool estado);
        Task<List<Entities.Usuario>> GetUsuariosDisponibles(string nombreRol);
        Task<bool> UsuarioEstaAsociado(int idUsuario);
        Task<bool> AsignarUsuario(int idDocente, int? idUsuario);
        Task<bool> CrearCuentaYAsociar(int idDocente, Entities.Usuario usuario);

    }
}