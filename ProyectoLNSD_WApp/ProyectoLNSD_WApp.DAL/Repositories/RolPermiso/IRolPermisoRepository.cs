namespace ProyectoLNSD_WApp.DAL.Repositories.RolPermiso
{
    public interface IRolPermisoRepository
    {
        Task<List<Entities.RolPermiso>> GetPermisosPorRol(int idRol);
        Task<bool> GuardarPermisos(int idRol, List<Entities.RolPermiso> permisos);
    }
}