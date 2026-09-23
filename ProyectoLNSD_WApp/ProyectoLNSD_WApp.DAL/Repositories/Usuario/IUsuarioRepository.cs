namespace ProyectoLNSD_WApp.DAL.Repositories.Usuario
{
    public interface IUsuarioRepository
    {
        Task<List<Entities.Usuario>> GetUsuarios();
        Task<Entities.Usuario?> GetUsuarioById(int id);
        Task<Entities.Usuario?> GetUsuarioByCorreo(string correo);
        Task<List<Entities.Usuario>> BuscarUsuarios(
            string? nombre,
            string? correo,
            int? idRol,
            bool? estado);

        Task<bool> CreateUsuario(Entities.Usuario usuario);
        Task<bool> UpdateUsuario(Entities.Usuario usuario);
        Task<bool> DeleteUsuario(int id);
        Task<bool> CambiarEstado(int id, bool estado);
    }
}