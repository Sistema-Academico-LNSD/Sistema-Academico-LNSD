namespace ProyectoLNSD_WApp.DAL.Repositories.AccesoRapido
{
    public interface IAccesoRapidoRepository
    {
        Task<List<Entities.AccesoRapido>> GetAccesos();
        Task<Entities.AccesoRapido?> GetAcceso(int idAcceso);

        // Qué roles ven cada acceso (incluye el nombre del rol)
        Task<List<Entities.AccesoRapidoRol>> GetAsignaciones();

        // Home: accesos activos visibles para alguno de los roles indicados
        Task<List<Entities.AccesoRapido>> GetActivosPorRol(IEnumerable<string> nombresRol);

        Task<bool> CreateAcceso(Entities.AccesoRapido acceso, IEnumerable<int> idsRoles);
        Task<bool> UpdateAcceso(Entities.AccesoRapido acceso, IEnumerable<int> idsRoles);
        Task<bool> DeleteAcceso(int idAcceso);
    }
}
