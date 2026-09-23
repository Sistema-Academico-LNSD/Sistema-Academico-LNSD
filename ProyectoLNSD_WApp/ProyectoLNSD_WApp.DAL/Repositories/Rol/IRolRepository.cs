namespace ProyectoLNSD_WApp.DAL.Repositories.Rol
{
    public interface IRolRepository
    {
        Task<List<Entities.Rol>> GetRoles();
        Task<Entities.Rol?> GetRolById(int id);
        Task<bool> CreateRol(Entities.Rol rol);
        Task<bool> UpdateRol(Entities.Rol rol);
        Task<bool> DeleteRol(int id);
    }
}